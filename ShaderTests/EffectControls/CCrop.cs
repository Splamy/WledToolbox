using SharpDX.Direct2D1.Effects;
using SharpDX.Mathematics.Interop;

namespace ShaderTests.EffectControls;

internal class CCrop : EffectEditor<Crop>
{
    public CCrop(ActiveEffect ae) : base(ae)
    {
        C(new Label { Text = "Left" });
        C(new NumericUpDown() { Minimum = 0, Maximum = 10000, DecimalPlaces = 0, Increment = 1 }, out var cropLeft);
        C(new Label { Text = "Top" });
        C(new NumericUpDown() { Minimum = 0, Maximum = 10000, DecimalPlaces = 0, Increment = 1 }, out var cropTop);
        C(new Label { Text = "Right" });
        C(new NumericUpDown() { Minimum = 0, Maximum = 10000, DecimalPlaces = 0, Increment = 1 }, out var cropRight);
        C(new Label { Text = "Bottom" });
        C(new NumericUpDown() { Minimum = 0, Maximum = 10000, DecimalPlaces = 0, Increment = 1 }, out var cropBottom);

        var rect = GetModel(x => x.Rectangle);
        cropLeft.Value = float.IsFinite(rect.X) ? (decimal)rect.X : 0m;
        cropTop.Value = float.IsFinite(rect.Y) ? (decimal)rect.Y : 0m;
        cropRight.Value = float.IsFinite(rect.Z) ? (decimal)rect.Z : 0m;
        cropBottom.Value = float.IsFinite(rect.W) ? (decimal)rect.W : 0m;

        cropLeft.ValueChanged += SetRect;
        cropTop.ValueChanged += SetRect;
        cropRight.ValueChanged += SetRect;
        cropBottom.ValueChanged += SetRect;

        void SetRect(object? sender, EventArgs e)
        {
            SetModel(x => x.Rectangle, new RawVector4((float)cropLeft.Value, (float)cropTop.Value, (float)cropRight.Value, (float)cropBottom.Value));
        }
    }
}
