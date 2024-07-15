using SharpDX.Mathematics.Interop;

static class RectExt
{
    public static int Width(this RawRectangle rect)
    {
        return rect.Right - rect.Left;
    }

    public static int Height(this RawRectangle rect)
    {
        return rect.Bottom - rect.Top;
    }
}