using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace WledToolbox;
internal class FramtimeView : UserControl
{
    private Lock _lock = new();
    private const int MaxFrames = 100;
    private readonly Stopwatch _frameStopwatch = new();
    private FrameStruct frameData = default;
    private readonly Queue<FrameStruct> _frametimes = [];


    private Brush[] Colors = [Brushes.Green, Brushes.Cyan];

    public FramtimeView()
    {
        this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
        _frameStopwatch.Start();
    }

    public void TrackWait()
    {
        frameData[0] += (float)_frameStopwatch.Elapsed.TotalMilliseconds;
        _frameStopwatch.Restart();
    }

    public void TrackProcess()
    {
        frameData[1] += (float)_frameStopwatch.Elapsed.TotalMilliseconds;
        _frameStopwatch.Restart();
    }

    public void AddFrame()
    {
        lock (_lock)
        {
            _frametimes.Enqueue(frameData);
            frameData = default;
            while (_frametimes.Count > MaxFrames)
            {
                _frametimes.Dequeue();
            }
        }
        Invalidate();
    }

    private readonly FrameStruct[] _frametimesBuffer = new FrameStruct[MaxFrames];
    private readonly float[] off = new float[MaxFrames];
    private readonly RectangleF[] _rectangles = new RectangleF[MaxFrames];

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
        e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;

        lock (_lock)
        {
            _frametimes.CopyTo(_frametimesBuffer, 0);
        }

        var frametimes = _frametimesBuffer.AsSpan();

        if (frametimes.IsEmpty)
        {
            return;
        }

        var width = Width;
        var height = Height;
        var barWidth = width / MaxFrames;

        off.AsSpan().Fill(height);

        for (int t = 0; t < 2; t++)
        {
            for (var i = 0; i < MaxFrames; i++)
            {
                var time = frametimes[i][t];
                var offBase = off[i];
                var newBase = offBase - time;
                off[i] = newBase;
                _rectangles[i] = new RectangleF(i * barWidth, newBase, barWidth, time);
            }
            e.Graphics.FillRectangles(Colors[t], _rectangles);
        }

        var min = _frametimesBuffer.Min(x => x.Sum);
        var max = _frametimesBuffer.Max(x => x.Sum);
        var avg = _frametimesBuffer.Average(x => x.Sum);
        var sb = new StringBuilder();
        sb.AppendLine($"Max: {max:0.00}ms = {(1000 / max)}FPS");
        sb.AppendLine($"Min: {min:0.00}ms = {(1000 / min)}FPS");
        sb.AppendLine($"Avg: {avg:0.00}ms = {(1000 / avg)}FPS");
        var metaString = sb.ToString();

        e.Graphics.DrawString(metaString, Font, Brushes.Black, 0, 0);
    }
}

static class MathExt
{
    public static T Min<T>(this Span<T> source) where T : INumber<T>
    {
        if (source.Length == 0)
        {
            throw new ArgumentException("Span must not be empty");
        }

        T min = source[0];
        int count = Vector<T>.Count;
        if (count <= source.Length)
        {
            var vMin = new Vector<T>(source);
            for (int i = count; i <= source.Length - count; i += count)
            {
                var v = new Vector<T>(source[i..]);
                vMin = Vector.Min(v, vMin);
            }

            min = vMin[0];
            for (int i = 1; i < count; i++)
            {
                if (vMin[i] < min) min = vMin[i];
            }
        }

        for (int i = source.Length - source.Length % count; i < source.Length; i++)
        {
            if (source[i] < min) min = source[i];
        }
        return min;
    }

    public static T Max<T>(this Span<T> source) where T : INumber<T>
    {
        if (source.Length == 0)
        {
            throw new ArgumentException("Span must not be empty");
        }

        T max = source[0];
        int count = Vector<T>.Count;
        if (count <= source.Length)
        {
            var vMax = new Vector<T>(source);
            for (int i = count; i <= source.Length - count; i += count)
            {
                var v = new Vector<T>(source[i..]);
                vMax = Vector.Max(v, vMax);
            }

            max = vMax[0];
            for (int i = 1; i < count; i++)
            {
                if (vMax[i] > max) max = vMax[i];
            }
        }

        for (int i = source.Length - source.Length % count; i < source.Length; i++)
        {
            if (source[i] > max) max = source[i];
        }
        return max;
    }

    public static T Sum<T>(this Span<T> source) where T : INumber<T>
    {
        T sum = T.AdditiveIdentity;
        int count = Vector<T>.Count;
        if (count <= source.Length)
        {
            var vSum = new Vector<T>(source);
            for (int i = count; i <= source.Length - count; i += count)
            {
                var v = new Vector<T>(source[i..]);
                vSum += v;
            }
            sum = vSum[0];
            for (int i = 1; i < count; i++)
            {
                sum += vSum[i];
            }
        }
        for (int i = source.Length - source.Length % count; i < source.Length; i++)
        {
            sum += source[i];
        }
        return sum;
    }

    public static T Average<T>(this Span<T> source) where T : INumber<T> => source.Sum() / T.CreateChecked(source.Length);
}

//[System.Runtime.CompilerServices.InlineArray(2)]
[DebuggerDisplay("{_wait} {_process}")]
public struct FrameStruct
{
    private float _wait;
    private float _process;

    public float this[int index]
    {
        readonly get => index switch
        {
            0 => _wait,
            1 => _process,
            _ => throw new IndexOutOfRangeException()
        };

        set
        {
            switch (index)
            {
            case 0:
                _wait = value;
                break;
            case 1:
                _process = value;
                break;
            default:
                throw new IndexOutOfRangeException();
            }
        }
    }

    public readonly float Sum => _wait + _process;
}