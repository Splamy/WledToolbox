using ShaderTests.EffectControls;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShaderTests;

public partial class Form1 : Form
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        IncludeFields = true,
    };

    private PeriodicTimer updateTimer = new(TimeSpan.FromMilliseconds(1000 / 60));
    private DesktopDuplicator desktopDuplicator;
    private System.Drawing.Bitmap outputBitmap;

    public List<EffectFactory> effectFactories = [];

    //public BindingList<ActiveEffect> pipeline = [];

    public Form1()
    {
        InitializeComponent();

        desktopDuplicator = new DesktopDuplicator();
        desktopDuplicator.RebuildPipeline += RebuildPipline;

        selectMonitorDropDown.Items.AddRange(desktopDuplicator.Outputs.Cast<object>().ToArray());
        selectMonitorDropDown.SelectedItem = desktopDuplicator.SelectedOutput;


        outputBitmap = new System.Drawing.Bitmap(
            desktopDuplicator.OutputTexSize.Width,
            desktopDuplicator.OutputTexSize.Height,
            System.Drawing.Imaging.PixelFormat.Format32bppRgb);
        customPictureBox1.Image = outputBitmap;

        //dragDropListBox1.DataSource = pipeline;
        dragDropListBox1.DisplayMember = "DisplayName";
        dragDropListBox1.Format += (s, e) => e.Value = ((ActiveEffect)e.ListItem).DisplayName;
        dragDropListBox1.DragDrop += OnReorder;

        effectFactories.AddRange([
            new EffectFactory("Crop", () => new SharpDX.Direct2D1.Effects.Crop(desktopDuplicator.d2dContext), typeof(CCrop)),
            new EffectFactory("AffineTransform2D", () => new SharpDX.Direct2D1.Effects.AffineTransform2D(desktopDuplicator.d2dContext), typeof(CAffineTransform2D)),
            //new EffectFactory("ArithmeticComposite", () => new SharpDX.Direct2D1.Effects.ArithmeticComposite(desktopDuplicator.d2dContext)),
            //new EffectFactory("HueRotation", () => new SharpDX.Direct2D1.Effects.HueRotation(desktopDuplicator.d2dContext)),
        ]);

        listBox1.DataSource = effectFactories;
        listBox1.DisplayMember = "Name";

    }

    private void RebuildPipline()
    {
        Effect? firstEffect = null;
        Effect? lastEffect = null;

        foreach (var effect in dragDropListBox1.Items.OfType<ActiveEffect>())
        {
            effect.Effect?.Dispose();
            effect.Effect = effect.Factory.Create();
            effect.ApplyModel();

            if (firstEffect == null)
            {
                firstEffect = effect.Effect;
            }

            if (lastEffect != null)
            {
                effect.Effect.SetInput(0, lastEffect.Output, true);
            }

            lastEffect = effect.Effect;
        }

        desktopDuplicator.InputEffect = firstEffect;
        desktopDuplicator.OutputEffect = lastEffect;
    }

    private void Form1_Show(object sender, EventArgs e)
    {
        _ = Task.Run(WorkTask);
    }

    private async Task WorkTask()
    {
        while (await updateTimer.WaitForNextTickAsync())
        {
            if (!desktopDuplicator.GetLatestFrame())
            {
                continue;
            }

            ShowDebugOutputImage();
        }
    }

    private unsafe void ShowDebugOutputImage()
    {
        if (!Monitor.TryEnter(customPictureBox1.PaintLock))
        {
            return;
        }

        try
        {
            using var g = Graphics.FromImage(outputBitmap);
            g.DrawImageUnscaled(desktopDuplicator.MiniImage, 0, 0);
        }
        finally
        {
            Monitor.Exit(customPictureBox1.PaintLock);
        }

        Invoke(() =>
        {
            customPictureBox1.Refresh();
        });
    }

    private void selectMonitorDropDown_SelectedIndexChanged(object sender, EventArgs e)
    {
        desktopDuplicator.SelectedOutput = (Output)selectMonitorDropDown.SelectedItem;
    }

    private void listBox1_DoubleClick(object sender, EventArgs e)
    {
        var ae = new ActiveEffect((EffectFactory)listBox1.SelectedItem, true);
        dragDropListBox1.Items.Add(ae);

        desktopDuplicator.RequestRebuild();
    }

    private void OnReorder(object? sender, DragEventArgs e)
    {
        desktopDuplicator.RequestRebuild();
    }

    private void dragDropListBox1_SelectedValueChanged(object sender, EventArgs e)
    {
        panel1.Controls.Clear();
        if (dragDropListBox1.SelectedItem is not ActiveEffect selected)
        {
            return;
        }

        var editor = (Control)Activator.CreateInstance(selected.Factory.EditorControl, [selected])!;
        panel1.Controls.Add(editor);
    }

    private void button1_Click(object sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "Json files|*.json"
        };
        if (dialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        var data = new SaveData
        {
            Effects = dragDropListBox1.Items.OfType<ActiveEffect>().Select(x => new SafeEffect
            {
                Name = x.Factory.Name,
                Model = new(x.Model),
            }).ToList(),
        };

        File.WriteAllText(dialog.FileName, JsonSerializer.Serialize(data, JsonOptions));
    }

    private void button2_Click(object sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Json files|*.json"
        };
        if (dialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        var data = JsonSerializer.Deserialize<SaveData>(File.ReadAllText(dialog.FileName), JsonOptions)!;

        dragDropListBox1.Items.Clear();
        foreach (var safeEffect in data.Effects)
        {
            var factory = effectFactories.First(x => x.Name == safeEffect.Name);
            var ae = new ActiveEffect(factory, true)
            {
                Model = new(safeEffect.Model),
            };
            dragDropListBox1.Items.Add(ae);
        }

        desktopDuplicator.RequestRebuild();
    }
}

public record EffectFactory(string Name, Func<Effect> Create, Type EditorControl);

public record ActiveEffect(EffectFactory Factory, bool Enabled)
{
    public string DisplayName => $"{Factory.Name} {(Enabled ? "Enabled" : "Disabled")}";
    public Effect Effect { get; set; } = Factory.Create.Invoke();
    public Dictionary<string, object> Model { get; init; } = [];

    public void ApplyModel()
    {
        if (Effect == null)
        {
            return;
        }

        var type = Effect.GetType();

        foreach (var kvp in Model)
        {
            var prop = type.GetProperty(kvp.Key);
            if (prop == null)
            {
                continue;
            }

            prop.SetValue(Effect, kvp.Value);
        }
    }
}

public class SaveData
{
    public List<SafeEffect> Effects { get; set; }
}

public class SafeEffect
{
    public string Name { get; set; }
    public ValueModel Model { get; set; }
}

[JsonConverter(typeof(ValueModelConverter))]
public class ValueModel : Dictionary<string, object>
{
    public ValueModel() { }
    public ValueModel(IDictionary<string, object> dictionary) : base(dictionary) { }
}

public class ValueModelConverter : JsonConverter<ValueModel>
{
    public override ValueModel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonElements = JsonSerializer.Deserialize<Dictionary<string, DeserializationTuple>>(ref reader, options)!;
        return new ValueModel(jsonElements.ToDictionary(x => x.Key, x =>
        {
            var type = Type.GetType(x.Value.Type)!;
            return x.Value.Value.Deserialize(type, options)!;
        }));
    }

    public override void Write(Utf8JsonWriter writer, ValueModel value, JsonSerializerOptions options)
    {
        var serializeDict = value.ToDictionary(x => x.Key, x => new SerializationTuple(x.Value.GetType().AssemblyQualifiedName!, x.Value));
        JsonSerializer.Serialize(writer, serializeDict, options);
    }

    record struct SerializationTuple(string Type, object Value);
    record struct DeserializationTuple(string Type, JsonElement Value);
}