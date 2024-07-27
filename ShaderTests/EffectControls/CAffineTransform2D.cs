using SharpDX.Direct2D1;
using SharpDX.Direct2D1.Effects;
using SharpDX.Mathematics.Interop;

namespace ShaderTests.EffectControls;

internal class CAffineTransform2D : EffectEditor<AffineTransform2D>
{
    const int RotRes = 10;

    public CAffineTransform2D(ActiveEffect ae) : base(ae)
    {
        C(new Label { Text = "Set Rotation + Translate" });
        C(new TrackBar { Minimum = 0, Maximum = 360 * RotRes, SmallChange = 1, LargeChange = RotRes, TickFrequency = RotRes }, out var rotationNum);
        C(new NumericUpDownEx { Minimum = -10000, Maximum = 10000, DecimalPlaces = 1, Increment = 1m }, out var translateXNum);
        C(new NumericUpDownEx { Minimum = -10000, Maximum = 10000, DecimalPlaces = 1, Increment = 1m }, out var translateYNum);

        C(new Label { Text = "Set Matrix" });
        C(new TextBox { Text = "1, 0, 0, 1, 0, 0" }, out var matrixTxt);

        var matrix = GetModel(x => x.TransformMatrix);

        rotationNum.Value = MatrixToRotSliderVal(matrix);
        translateXNum.CurrentEditValue = (decimal)matrix.M31;
        translateYNum.CurrentEditValue = (decimal)matrix.M32;
        matrixTxt.Text = $"{matrix.M11}, {matrix.M12}, {matrix.M21}, {matrix.M22}, {matrix.M31}, {matrix.M32}";

        rotationNum.ValueChanged += ApplyRotation;
        translateXNum.CurrentEditValueChanged += ApplyRotation;
        translateYNum.CurrentEditValueChanged += ApplyRotation;
        matrixTxt.TextChanged += ApplyRawMatrix;

        void ApplyRotation(object? sender, EventArgs e)
        {
            var angle = (rotationNum.Value / (float)RotRes) / 360f * MathF.PI * 2;
            var offX = (float)translateXNum.CurrentEditValue;
            var offY = (float)translateYNum.CurrentEditValue;
            var (sin, cos) = MathF.SinCos(angle);

            var matrix = new RawMatrix3x2(
                cos, -sin,
                sin, cos,
                offX, offY);

            SetModel(x => x.TransformMatrix, matrix);

            matrixTxt.TextChanged -= ApplyRawMatrix;
            matrixTxt.Text = $"{matrix.M11}; {matrix.M12}; {matrix.M21}; {matrix.M22}; {matrix.M31}; {matrix.M32}";
            matrixTxt.TextChanged += ApplyRawMatrix;
        }

        void ApplyRawMatrix(object? sender, EventArgs e)
        {
            var mm = matrixTxt.Text.Split(';').Select(x => float.TryParse(x, out var f) ? (float?)f : null).ToArray();
            if (mm.Any(x => x == null)) return;
            var matrix = new RawMatrix3x2(mm[0]!.Value, mm[1]!.Value, mm[2]!.Value, mm[3]!.Value, mm[4]!.Value, mm[5]!.Value);
            SetModel(x => x.TransformMatrix, matrix);

            rotationNum.ValueChanged -= ApplyRotation;
            translateXNum.CurrentEditValueChanged -= ApplyRotation;
            translateYNum.CurrentEditValueChanged -= ApplyRotation;
            rotationNum.Value = MatrixToRotSliderVal(matrix);
            translateXNum.CurrentEditValue = (decimal)matrix.M31;
            translateYNum.CurrentEditValue = (decimal)matrix.M32;
            rotationNum.ValueChanged += ApplyRotation;
            translateXNum.CurrentEditValueChanged += ApplyRotation;
            translateYNum.CurrentEditValueChanged += ApplyRotation;
        }

        C(new Label { Text = "Interpolation" });
        C(new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList }, out var interpolationMode);
        var interpolation = GetModel(x => x.InterpolationMode);
        interpolationMode.Items.AddRange(Enum.GetValues<InterpolationMode>().Cast<object>().ToArray());
        interpolationMode.SelectedItem = interpolation;
        interpolationMode.SelectedIndexChanged += (s, e) => SetModel(x => x.InterpolationMode, (InterpolationMode)interpolationMode.SelectedItem);

        C(new Label { Text = "Border" });
        C(new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList }, out var borderMode);
        var border = GetModel(x => x.BorderMode);
        borderMode.Items.AddRange(Enum.GetValues<BorderMode>().Cast<object>().ToArray());
        borderMode.SelectedItem = border;
        borderMode.SelectedIndexChanged += (s, e) => SetModel(x => x.BorderMode, (BorderMode)borderMode.SelectedItem);

        C(new Label { Text = "Sharpness" });
        C(new TrackBar { Minimum = 0, Maximum = 100 }, out var sharpness);
        sharpness.Value = (int)(GetModel(x => x.Sharpness) * 100);
        sharpness.ValueChanged += (s, e) => SetModel(x => x.Sharpness, sharpness.Value / 100f);
    }

    private static int MatrixToRotSliderVal(RawMatrix3x2 matrix)
    {
        var angleDeg = MathF.Atan2(matrix.M21, matrix.M11) * 180 / MathF.PI;
        angleDeg = (angleDeg + 360) % 360;
        return (int)(angleDeg * RotRes);
    }
}
