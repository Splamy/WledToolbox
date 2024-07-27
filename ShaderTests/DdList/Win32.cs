using System.Runtime.InteropServices;

namespace ShaderTests.DdList;

/// <summary>
/// Win 32 API stuff used in this Project.
/// </summary>
internal static class Win32
{
    [DllImport("gdi32.dll", EntryPoint = "SetROP2", CallingConvention = CallingConvention.StdCall)]
    public extern static int SetROP2(nint hdc, int fnDrawMode);

    [DllImport("user32.dll", EntryPoint = "GetDC", CallingConvention = CallingConvention.StdCall)]
    public extern static nint GetDC(nint hWnd);

    [DllImport("user32.dll", EntryPoint = "ReleaseDC", CallingConvention = CallingConvention.StdCall)]
    public extern static nint ReleaseDC(nint hWnd, nint hDC);

    [DllImport("gdi32.dll", EntryPoint = "MoveToEx", CallingConvention = CallingConvention.StdCall)]
    public extern static bool MoveToEx(nint hdc, int x, int y, nint lpPoint);

    [DllImport("gdi32.dll", EntryPoint = "LineTo", CallingConvention = CallingConvention.StdCall)]
    public extern static bool LineTo(nint hdc, int x, int y);

    public const int R2_NOT = 6;  // Inverted drawing mode

}
