using DesktopDuplication;
using SharpDX.DXGI;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Interop;
using WledLightbox.Render;

namespace WledLightbox;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly PeriodicTimer _updateTimer = new(TimeSpan.FromMilliseconds(1000 / 60));
    private Task? _workTask = null;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
    {
        if (!Equals(field, newValue))
        {
            field = newValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        return false;
    }

    public DesktopDuplicator Capture { get; }
    public WledCore Wled { get; }

    public Bitmap bitmap { get; }

    public bool RenderDebugImage { get; set; }
    public bool SendToWled { get; set; }

    public IReadOnlyList<OutputViewModel> Outputs { get; }
    public OutputViewModel? SelectedOutput
    {
        get => Outputs.FirstOrDefault(x => x.Output == Capture.SelectedOutput);
        set => Capture.SelectedOutput = value?.Output;
    }

    private object content;
    public object Content
    {
        get
        {
            return content;
        }
        set
        {
            SetProperty(ref content, value);
        }
    }

    public MainViewModel()
    {
        Capture = new();
        Outputs = Capture.Outputs.Select(x => new OutputViewModel(x)).ToList();
        Wled = new();
    }

    public void Run()
    {
        _workTask = Task.Run(RunWorker);
    }

    private async Task RunWorker()
    {
        while (await _updateTimer.WaitForNextTickAsync())
        {
            if (!Capture.GetLatestFrame())
            {
                continue;
            }

            if (RenderDebugImage)
            {
                //if (Monitor.TryEnter(pictureBox2.PaintLock))
                //{
                //    try
                //    {
                //        using var g = Graphics.FromImage(bitmap);
                //        g.DrawImage(desktopDuplicator.GdiOutImage, 0, 0);
                //    }
                //    finally
                //    {
                //        Monitor.Exit(pictureBox2.PaintLock);
                //    }
                //}

                //Invoke(() =>
                //{
                //    pictureBox2.Refresh();
                //});
            }

            if (SendToWled)
            {
                await Wled.Send(Capture.GdiOutImage);
            }
        }
    }
}

public class OutputViewModel
{
    public Output Output { get; }
    public string Name { get; }

    public OutputViewModel(Output output)
    {
        Output = output;

        var details = ScreenInterrogatory.GetDeviceDetails(output.Description.DeviceName);
        var friendlyName = ScreenInterrogatory.CachedScreenNames
            .FirstOrDefault(x => x.Screen.DeviceName == output.Description.DeviceName)
            .Name;
        Name = $"{friendlyName} ({details.DeviceString})";
    }
}