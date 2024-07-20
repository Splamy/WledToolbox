using DesktopDuplication;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D9;
using SharpDX.Mathematics.Interop;

namespace WledLightbox.Render;

class Sample2DRenderer(DesktopDuplicator? caputre) : Direct2DComponent(caputre)
{
    private RawVector2 _position = new RawVector2(30, 30);
    private RawVector2 _speed = new RawVector2(5, 2);
    private SolidColorBrush _circleColor;

    protected override void InternalInitialize()
    {
        base.InternalInitialize();

        _circleColor = new SolidColorBrush(RenderTarget2D, new RawColor4(1, 0.2f, 0.2f, 1));
    }

    protected override void InternalUninitialize()
    {
        Utilities.Dispose(ref _circleColor);

        base.InternalUninitialize();
    }

    protected override void Render()
    {
        UpdatePosition();

        RenderTarget2D.Clear(new RawColor4(1.0f, 0, 1.0f, 1));
        RenderTarget2D.FillEllipse(new Ellipse(_position, 20, 20), _circleColor);

        var sharedResource = caputre.OutTexture.Texture.QueryInterface<SharpDX.DXGI.Resource>();
        using var texture = _device.OpenSharedResource<SharpDX.Direct3D11.Texture2D>(sharedResource.SharedHandle);
        using var surface = texture.QueryInterface<SharpDX.DXGI.Surface>();
        using var bitmap = new SharpDX.Direct2D1.Bitmap1(d2dContext, surface);

        RenderTarget2D.DrawBitmap(bitmap, 1, BitmapInterpolationMode.Linear);
    }

    private void UpdatePosition()
    {
        _position = new RawVector2(_position.X + _speed.X, _position.Y + _speed.Y);

        if (_position.X > SurfaceWidth)
        {
            _position.X = SurfaceWidth;
            _speed.X = -_speed.X;
        }
        else if (_position.X < 0)
        {
            _position.X = 0;
            _speed.X = -_speed.X;
        }

        if (_position.Y > SurfaceHeight)
        {
            _position.Y = SurfaceHeight;
            _speed.Y = -_speed.Y;
        }
        else if (_position.Y < 0)
        {
            _position.Y = 0;
            _speed.Y = -_speed.Y;
        }
    }
}
