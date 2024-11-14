using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Threading;

namespace System.Windows.Forms;

public class CustomPictureBox : PictureBox
{
    public Lock PaintLock { get; } = new();

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
