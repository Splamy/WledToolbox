using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;

namespace WledToolbox;

// Points are draggable in 2D space
// Values are between 0f and 1f

internal class PointPicker2D : Control
{
    const int Radius = 7;
    const int GrabRadius = Radius + 3;
    public List<PointData> Points { get; } = [];
    private bool IsDragging;
    private int DragIndex;

    private Matrix3x2 _pointTransform = Matrix3x2.Identity;
    private Matrix3x2 _pointTransformInverse = Matrix3x2.Identity;

    public Matrix3x2 PointTransform
    {
        get => _pointTransform; set
        {
            _pointTransform = value;
            if (!Matrix3x2.Invert(value, out var inverse))
            {
                throw new InvalidOperationException("Matrix is not invertible");
            }
            _pointTransformInverse = inverse;
        }
    }

    public event Action? PointsChanged;
    public event Action<Graphics>? PreDraw;
    public event Action<Graphics>? PostDraw;

    public PointPicker2D()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
    }

    public PointF ToUiCoordinates(Vector2 point)
    {
        // Apply transformation

        point = Vector2.Transform(point, _pointTransform);
        return new PointF(point.X * Width, point.Y * Height);
    }

    public Vector2 ToNormalizedCoordinates(PointF point)
    {
        // Apply inverse transformation

        var scaledPoint = new Vector2(point.X / Width, point.Y / Height);
        var transformedPoint = Vector2.Transform(scaledPoint, _pointTransformInverse);
        return transformedPoint;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        PreDraw?.Invoke(e.Graphics);
        foreach (var point in Points)
        {
            var uiPoint = ToUiCoordinates(point.Point);
            e.Graphics.FillEllipse(point.Color, uiPoint.X - Radius, uiPoint.Y - Radius, Radius * 2, Radius * 2);
        }
        PostDraw?.Invoke(e.Graphics);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        // Find point near the mouse
        for (int i = 0; i < Points.Count; i++)
        {
            var point = ToUiCoordinates(Points[i].Point);
            if (Math.Abs(point.X - e.X) < GrabRadius && Math.Abs(point.Y - e.Y) < GrabRadius)
            {
                IsDragging = true;
                DragIndex = i;
                return;
            }
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (IsDragging)
        {
            var pointData = Points[DragIndex];
            var pt = ToNormalizedCoordinates(new PointF(e.X, e.Y));
            if (pointData.Constraint != null)
            {
                pt = pointData.Constraint(Points, pt);
            }
            pointData.Point = Clamp(pt);

            PointsChanged?.Invoke();

            Invalidate();
        }
    }

    private static Vector2 Clamp(Vector2 value) => Vector2.Clamp(value, Vector2.Zero, Vector2.One);

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        IsDragging = false;
    }

    public class PointData(Brush Color, Vector2 point)
    {
        public Brush Color { get; } = Color;
        public Vector2 Point { get; set; } = point;

        public Func<IReadOnlyList<PointData>, Vector2, Vector2>? Constraint { get; set; }

        public override string ToString() => $"{Point} ({Color})";
    }
}
