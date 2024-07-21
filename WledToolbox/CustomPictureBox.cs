using System.Drawing.Drawing2D;

namespace System.Windows.Forms;

public class CustomPictureBox : PictureBox
{
    public object PaintLock { get; } = new();

    public InterpolationMode InterpolationMode { get; set; }

    public CustomPictureBox() : base()
    {

    }

    protected override void OnPaint(PaintEventArgs paintEventArgs)
    {
        lock (PaintLock)
        {
            paintEventArgs.Graphics.InterpolationMode = InterpolationMode;
            paintEventArgs.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            base.OnPaint(paintEventArgs);
        }
    }
}
