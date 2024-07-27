using SharpDX;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using d2 = SharpDX.Direct2D1;
using d3d = SharpDX.Direct3D11;
using dxgi = SharpDX.DXGI;
using sd = System.Drawing;
using sdi = System.Drawing.Imaging;

namespace DesktopDuplication;

/// <summary>
/// Provides access to frame-by-frame updates of a particular desktop (i.e. one monitor), with image and cursor information.
/// </summary>
public class DesktopDuplicator
{
    private readonly dxgi.Adapter1 dxgiAdapter;
    private readonly dxgi.Factory1 dxgiFactory;
    private readonly d3d.Device1 d3dDevice;
    private readonly d3d.DeviceContext1 d3dContext;
    private readonly dxgi.Device dxgiDevice;
    private readonly d2.Device d2dDevice;
    private readonly d2.DeviceContext d2dContext;
    private dxgi.OutputDuplication? _outputDuplication;

    private TextureHelper _mainDataStage;
    private TextureHelper _mainDataWrite;

    private TextureHelper _miniImageWrite;
    private TextureHelper _miniImageStage;

    public Size2 InputTexSize { get; } = new(239, 196);
    public Size2 OutputTexSize { get; } = new(239, 3);

    public ImmutableArray<dxgi.Output> Outputs { get; }

    private dxgi.Output? _activeOutput = null;
    private dxgi.OutputDescription _activeOutputDescription;
    private Pipeline? _pipeline;
    private EffectBox? _effectBox;

    public dxgi.Output? SelectedOutput { get; set; }
    public bool CopyMiniImageOut { get; set; } = false;
    public sd.Bitmap MiniImage { get; private set; }
    public BGRAPixel[] OutData { get; }
    public TextureHelper OutTexture => _mainDataStage;

    public RawRectangle PreCrop { get; set; } = new(0, 0, 0, 0);
    private RawVector4 PreCropFloat => new(
        PreCrop.Left,
        PreCrop.Top,
        _activeOutputDescription.DesktopBounds.Width() - PreCrop.Right,
        _activeOutputDescription.DesktopBounds.Height() - PreCrop.Bottom);
    private RawVector2 PreScaleFloat => new(
        InputTexSize.Width / PreCropFloat.Z,
        InputTexSize.Height / PreCropFloat.W);
    public float BlurAmount { get; set; } = 3f;
    public RawVector3 Gamma { get; set; } = new(1f, 1f, 1f);
    public (RawVector2 Black, RawVector2 White) Brightness { get; set; } = (new(0f, 0f), new(1f, 1f));

    /// <summary>
    /// Duplicates the output of the specified monitor.
    /// </summary>
    /// <param name="whichMonitor">The output device to duplicate (i.e. monitor). Begins with zero, which seems to correspond to the primary monitor.</param>
    public DesktopDuplicator() : this(0) { }

    /// <summary>
    /// Duplicates the output of the specified monitor on the specified graphics adapter.
    /// </summary>
    /// <param name="whichGraphicsCardAdapter">The adapter which contains the desired outputs.</param>
    /// <param name="whichOutputDevice">The output device to duplicate (i.e. monitor). Begins with zero, which seems to correspond to the primary monitor.</param>
    public DesktopDuplicator(int whichGraphicsCardAdapter)
    {
        try
        {
            dxgiFactory = new dxgi.Factory1();
            dxgiAdapter = dxgiFactory.GetAdapter1(whichGraphicsCardAdapter);
        }
        catch (SharpDXException)
        {
            throw new DesktopDuplicationException("Could not find the specified graphics card adapter.");
        }

        var device = new d3d.Device(dxgiAdapter, d3d.DeviceCreationFlags.VideoSupport | d3d.DeviceCreationFlags.BgraSupport);

        d3dDevice = device.QueryInterface<d3d.Device1>();
        d3dContext = d3dDevice.ImmediateContext.QueryInterface<d3d.DeviceContext1>();
        dxgiDevice = d3dDevice.QueryInterface<dxgi.Device>();
        d2dDevice = new d2.Device(dxgiDevice);
        d2dContext = new d2.DeviceContext(d2dDevice, d2.DeviceContextOptions.None);

        _mainDataWrite = CreateTexture(OutputTexSize, TextureHelperType.Write);
        _mainDataStage = CreateTexture(OutputTexSize, TextureHelperType.Stage);
        _miniImageWrite = CreateTexture(InputTexSize, TextureHelperType.Write);
        _miniImageStage = CreateTexture(InputTexSize, TextureHelperType.Stage);

        Outputs = [.. dxgiDevice.Adapter.Outputs];
        SelectedOutput = Outputs.FirstOrDefault();

        MiniImage = new sd.Bitmap(InputTexSize.Width, InputTexSize.Height, sdi.PixelFormat.Format32bppRgb);
        OutData = new BGRAPixel[OutputTexSize.Width * OutputTexSize.Height];
    }

    private TextureHelper CreateTexture(Size2 size, TextureHelperType type)
    {
        var texture = new d3d.Texture2D(d3dDevice, new d3d.Texture2DDescription()
        {
            CpuAccessFlags = type == TextureHelperType.Stage ? d3d.CpuAccessFlags.Read : d3d.CpuAccessFlags.None,
            BindFlags = type switch
            {
                TextureHelperType.Write => d3d.BindFlags.RenderTarget,
                TextureHelperType.ReadWrite => d3d.BindFlags.RenderTarget | d3d.BindFlags.ShaderResource,
                TextureHelperType.Stage => d3d.BindFlags.None,
            },
            Format = dxgi.Format.B8G8R8A8_UNorm,
            Width = size.Width,
            Height = size.Height,
            OptionFlags = d3d.ResourceOptionFlags.None,
            MipLevels = 1,
            ArraySize = 1,
            SampleDescription = { Count = 1, Quality = 0 },
            Usage = type == TextureHelperType.Stage ? d3d.ResourceUsage.Staging : d3d.ResourceUsage.Default,
        });

        var surface = texture.QueryInterface<dxgi.Surface>();
        var bitmap = new d2.Bitmap1(d2dContext, surface);

        return new TextureHelper(texture, surface, bitmap);
    }

    /// <summary>
    /// Retrieves the latest desktop image and associated metadata.
    /// </summary>
    public bool GetLatestFrame()
    {
        CheckCurrentOutputDevice();

        if (_activeOutput == null)
            return false;

        // Try to get the latest frame; this may timeout
        using var release = RetrieveFrame();

        if (!release.Ok)
            return false;

        WriteOutputBuffer();
        WriteMiniInputBuffer();
        return true;
    }

    private void CheckCurrentOutputDevice()
    {
        if (SelectedOutput != _activeOutput)
        {
            // Cleanup
            if (_outputDuplication != null)
            {
                _outputDuplication.Dispose();
                _outputDuplication = null;
            }
            _activeOutput = null;
            _activeOutputDescription = default;

            if (_pipeline != null)
            {
                _pipeline.Dispose();
                _pipeline = null;
            }
            _effectBox = null;

            // Create
            if (SelectedOutput != null)
            {
                var output1 = SelectedOutput.QueryInterface<dxgi.Output1>();

                try
                {
                    _outputDuplication = output1.DuplicateOutput(d3dDevice);
                }
                catch (SharpDXException ex)
                {
                    if (ex.ResultCode.Code == dxgi.ResultCode.NotCurrentlyAvailable.Result.Code)
                    {
                        throw new DesktopDuplicationException("There is already the maximum number of applications using the Desktop Duplication API running, please close one of the applications and try again.");
                    }

                    throw;
                }

                _activeOutput = SelectedOutput;
                _activeOutputDescription = SelectedOutput.Description;
            }

            CreatePipeline();
        }
    }

    private void CreatePipeline()
    {
        _pipeline = new Pipeline(d2dContext);
        _effectBox = new EffectBox();

        var inputT1 = _pipeline.EffectRotate(null!, 0, new Size2(0, 0));
        var inputT2 = _pipeline.EffectScale(inputT1.Output, new RawVector2(1, 1));

        _effectBox.PreCrop = ((d2.Effects.AffineTransform2D)inputT1, (d2.Effects.Scale)inputT2);
        _effectBox.TrackInput(inputT1);

        var baseImage = inputT2;

        var preview = baseImage;
        preview = _pipeline.EffectBrightness(preview.Output, Brightness).TrackBrightness(_effectBox);
        preview = _pipeline.EffectGamma(preview.Output, Gamma).TrackGamma(_effectBox);
        preview = _pipeline.EffectBlur(preview.Output, BlurAmount).TrackBlur(_effectBox);
        _effectBox.TrackOutput(preview);

        var top = baseImage;
        top = _pipeline.EffectCrop(top.Output, new RawVector4(0f, 0f, InputTexSize.Width, InputTexSize.Height / 2f));
        top = _pipeline.EffectScale(top.Output, new RawVector2(1f, 2f / InputTexSize.Height));
        top = _pipeline.EffectCrop(top.Output, new RawVector4(0f, 0f, InputTexSize.Width, 1));
        top = _pipeline.EffectClamp(top.Output);
        top = _pipeline.EffectBrightness(top.Output, Brightness).TrackBrightness(_effectBox);
        top = _pipeline.EffectGamma(top.Output, Gamma).TrackGamma(_effectBox);
        top = _pipeline.EffectBlur(top.Output, BlurAmount).TrackBlur(_effectBox);
        top = _pipeline.EffectCrop(top.Output, new RawVector4(0f, 0f, InputTexSize.Width, 1));
        _effectBox.TrackOutput(top);

        var left = baseImage;
        left = _pipeline.EffectCrop(left.Output, new RawVector4(0f, 0f, InputTexSize.Width / 4f, InputTexSize.Height));
        left = _pipeline.EffectRotate(left.Output, 3 * MathF.PI / 2f, new Size2(InputTexSize.Height, 0));
        left = _pipeline.EffectScale(left.Output, new RawVector2(1f, 4f / InputTexSize.Width));
        left = _pipeline.EffectCrop(left.Output, new RawVector4(0f, 0f, InputTexSize.Height, 1));
        left = _pipeline.EffectClamp(left.Output);
        left = _pipeline.EffectBrightness(left.Output, Brightness).TrackBrightness(_effectBox);
        left = _pipeline.EffectGamma(left.Output, Gamma).TrackGamma(_effectBox);
        left = _pipeline.EffectBlur(left.Output, BlurAmount).TrackBlur(_effectBox);
        left = _pipeline.EffectCrop(left.Output, new RawVector4(0f, 0f, InputTexSize.Height, 1));
        _effectBox.TrackOutput(left);

        var right = baseImage;
        right = _pipeline.EffectCrop(right.Output, new RawVector4(3f * InputTexSize.Width / 4f, 0f, InputTexSize.Width, InputTexSize.Height));
        right = _pipeline.EffectRotate(right.Output, MathF.PI / 2f, new Size2(0, InputTexSize.Width));
        right = _pipeline.EffectScale(right.Output, new RawVector2(1f, 4f / InputTexSize.Width));
        right = _pipeline.EffectCrop(right.Output, new RawVector4(0f, 0f, InputTexSize.Height, 1));
        right = _pipeline.EffectClamp(right.Output);
        right = _pipeline.EffectBrightness(right.Output, Brightness).TrackBrightness(_effectBox);
        right = _pipeline.EffectGamma(right.Output, Gamma).TrackGamma(_effectBox);
        right = _pipeline.EffectBlur(right.Output, BlurAmount).TrackBlur(_effectBox);
        right = _pipeline.EffectCrop(right.Output, new RawVector4(0f, 0f, InputTexSize.Height, 1));
        _effectBox.TrackOutput(right);
    }

    private FrameInfo RetrieveFrame()
    {
        var result = _outputDuplication.TryAcquireNextFrame(500, out var frameInfo, out var desktopResource);

        if (result.Failure)
        {
            if (result.Code == dxgi.ResultCode.AccessLost.Result.Code)
            {
                _activeOutput = null;
            }

            return FrameInfo.Empty;
        }

        using var __desktopResource = desktopResource;
        using var desktopSurface = desktopResource.QueryInterface<dxgi.Surface>();
        using var desktopBitmap = new d2.Bitmap1(d2dContext, desktopSurface);

        _effectBox.SetInput(desktopBitmap);
        if (_effectBox.PreCrop is ({ } trans, { } scale))
        {
            var sourceSize = new RawVector2(
                _activeOutputDescription.DesktopBounds.Width(),
                _activeOutputDescription.DesktopBounds.Height());
            //var sourceCenter = new RawVector2(
            //    sourceSize.X / 2f,
            //    sourceSize.Y / 2f);

            var croppedRectSize = new RawVector2(
                sourceSize.X - (PreCrop.Left + PreCrop.Right),
                sourceSize.Y - (PreCrop.Top + PreCrop.Bottom));
            //var croppedRectCenter = new RawVector2(
            //    PreCrop.Left + croppedRectSize.X / 2f,
            //    PreCrop.Top + croppedRectSize.Y / 2f);
            //var croppedCenterInSource = new RawVector2(
            //    PreCrop.Left + croppedRectCenter.X,
            //    PreCrop.Top + croppedRectCenter.Y);

            var ratio = new RawVector2(
                InputTexSize.Width / croppedRectSize.X,
                InputTexSize.Height / croppedRectSize.Y);

            //var inputTexCenter = new RawVector2(
            //    InputTexSize.Width / 2f,
            //    InputTexSize.Height / 2f);

            trans.TransformMatrix = new RawMatrix3x2(
                1, 0,
                0, 1,
                -PreCrop.Left, -PreCrop.Top);

            scale.ScaleAmount = ratio;
        }
        _effectBox.SetBlur(BlurAmount);
        _effectBox.SetGamma(Gamma);
        _effectBox.SetBrightness(Brightness);

        var miniView = _effectBox.Outputs[0];
        var top = _effectBox.Outputs[1];
        var left = _effectBox.Outputs[2];
        var right = _effectBox.Outputs[3];

        d2dContext.Target = _mainDataWrite.Bitmap;
        d2dContext.BeginDraw();
        d2dContext.DrawImage(top, new RawVector2(0, 0), d2.InterpolationMode.Linear, d2.CompositeMode.SourceOver);
        d2dContext.DrawImage(left, new RawVector2(0, 1), d2.InterpolationMode.Linear, d2.CompositeMode.SourceOver);
        d2dContext.DrawImage(right, new RawVector2(0, 2), d2.InterpolationMode.Linear, d2.CompositeMode.SourceOver);

        //d2dContext.DrawBitmap(desktopBitmap, new RawRectangleF(0, 0, small.Width, small.Height), 1, d2.InterpolationMode.HighQualityCubic, null, null);
        d2dContext.EndDraw();

        //d2dContext.Target = intermediateAltBitmap;
        //d2dContext.BeginDraw();
        //d2dContext.DrawBitmap(intermediateBitmap, small, 1, d2.InterpolationMode.HighQualityCubic, small, null);
        //d2dContext.DrawBitmap(intermediateBitmap, new RawRectangleF(small.Right, 0, small.Right + 300, 300), 1, d2.InterpolationMode.NearestNeighbor, small, null);
        //d2dContext.EndDraw();

        d3dContext.CopyResource(_mainDataWrite.Texture, _mainDataStage.Texture);

        if (CopyMiniImageOut)
        {
            d2dContext.Target = _miniImageWrite.Bitmap;
            d2dContext.BeginDraw();
            d2dContext.DrawImage(miniView, new RawVector2(0, 0), d2.InterpolationMode.Linear, d2.CompositeMode.SourceOver);
            d2dContext.EndDraw();

            d3dContext.CopyResource(_miniImageWrite.Texture, _miniImageStage.Texture);
        }

        return new FrameInfo(_outputDuplication.ReleaseFrame);
    }

    private unsafe void WriteMiniInputBuffer()
    {
        if (!CopyMiniImageOut)
            return;

        // Get the desktop capture texture
        var mapSource = d3dContext.MapSubresource(_miniImageStage.Texture, 0, d3d.MapMode.Read, d3d.MapFlags.None);

        var boundsRect = new sd.Rectangle(0, 0, MiniImage.Width, MiniImage.Height);
        // Copy pixels from screen capture Texture to GDI bitmap
        var mapDest = MiniImage.LockBits(boundsRect, sdi.ImageLockMode.WriteOnly, MiniImage.PixelFormat);
        var sourcePtr = mapSource.DataPointer;
        var destPtr = mapDest.Scan0;
        for (int y = 0; y < MiniImage.Height; y++)
        {
            // Copy a single line 
            Utilities.CopyMemory(destPtr, sourcePtr, MiniImage.Width * 4);

            // Advance pointers
            sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);
            destPtr = IntPtr.Add(destPtr, mapDest.Stride);
        }

        // Release source and dest locks
        MiniImage.UnlockBits(mapDest);
        d3dContext.UnmapSubresource(_miniImageStage.Texture, 0);
    }

    private unsafe void WriteOutputBuffer()
    {
        // Get the desktop capture texture
        var mapSource = d3dContext.MapSubresource(_mainDataStage.Texture, 0, d3d.MapMode.Read, d3d.MapFlags.None);

        var sourcePtr = mapSource.DataPointer;

        fixed (BGRAPixel* outDataPtr = OutData)
        {
            nint destPtr = (nint)outDataPtr;
            int destStride = OutputTexSize.Width * 4;

            for (int i = 0; i < OutputTexSize.Height; i++)
            {
                Utilities.CopyMemory(destPtr, sourcePtr, destStride);

                destPtr += destStride;
                sourcePtr += mapSource.RowPitch;
            }
        }

        d3dContext.UnmapSubresource(_mainDataStage.Texture, 0);
    }
}

readonly struct FrameInfo(Action? release) : IDisposable
{
    public static readonly FrameInfo Empty = new(null);
    public readonly bool Ok => release != null;
    public readonly void Dispose() => release?.Invoke();
}

class Pipeline : IDisposable
{
    private readonly d2.DeviceContext d2dContext;
    private readonly List<d2.Effect> Effects = [];

    public Pipeline(d2.DeviceContext d2dContext)
    {
        this.d2dContext = d2dContext;
    }

    public d2.Effect AddEffect(d2.Effect effect)
    {
        Effects.Add(effect);
        return effect;
    }

    // Effects

    public d2.Effect EffectBlur(d2.Image image, float value)
    {
        var effect = new d2.Effects.GaussianBlur(d2dContext);
        effect.SetInput(0, image, true);
        effect.StandardDeviation = value;
        effect.Optimization = d2.GaussianBlurOptimization.Speed;
        effect.BorderMode = d2.BorderMode.Hard;
        return AddEffect(effect);
    }

    public d2.Effect EffectCrop(d2.Image image, RawVector4 rectangle)
    {
        var effect = new d2.Effects.Crop(d2dContext);
        effect.SetInput(0, image, true);
        effect.Rectangle = rectangle;
        return AddEffect(effect);
    }

    public d2.Effect EffectScale(d2.Image image, RawVector2 scaleAmount, RawVector2 center = default)
    {
        var effect = new d2.Effects.Scale(d2dContext);
        effect.SetInput(0, image, true);
        effect.InterpolationMode = d2.InterpolationMode.HighQualityCubic;
        effect.CenterPoint = center;
        effect.ScaleAmount = scaleAmount;
        return AddEffect(effect);
    }

    public d2.Effect EffectClamp(d2.Image image)
    {
        var effect = new d2.Effects.Border(d2dContext);
        effect.SetInput(0, image, true);
        effect.EdgeModeX = d2.BorderEdgeMode.Clamp;
        effect.EdgeModeY = d2.BorderEdgeMode.Clamp;
        return AddEffect(effect);
    }

    public d2.Effect EffectRotate(d2.Image image, float angle, Size2 offset = default)
    {
        var effect = new d2.Effects.AffineTransform2D(d2dContext);
        effect.SetInput(0, image, true);
        var (sin, cos) = MathF.SinCos(angle);
        sin = MathF.Round(sin);
        cos = MathF.Round(cos);
        effect.TransformMatrix = new RawMatrix3x2(
            cos, -sin,
            sin, cos,
            offset.Width, offset.Height);
        return AddEffect(effect);
    }

    public d2.Effect EffectGamma(d2.Image image, RawVector3 gamma)
    {
        var effect = new d2.Effects.GammaTransfer(d2dContext);
        effect.SetInput(0, image, true);
        effect.RedAmplitude = gamma.X;
        effect.GreenAmplitude = gamma.Y;
        effect.BlueAmplitude = gamma.Z;
        return AddEffect(effect);
    }

    public d2.Effect EffectBrightness(d2.Image image, (RawVector2 black, RawVector2 white) brightness)
    {
        var effect = new d2.Effects.Brightness(d2dContext);
        effect.SetInput(0, image, true);
        effect.BlackPoint = brightness.black;
        effect.WhitePoint = brightness.white;
        return AddEffect(effect);
    }

    public void Dispose()
    {
        foreach (var effect in Effects)
        {
            effect.Dispose();
        }
    }
}

public class EffectBox
{
    public List<d2.Effect> Inputs { get; } = [];
    public List<d2.Effect> Outputs { get; } = [];
    public (d2.Effects.AffineTransform2D, d2.Effects.Scale)? PreCrop { get; set; } = null;
    public List<d2.Effects.GaussianBlur> Blurs { get; } = [];
    public List<d2.Effects.GammaTransfer> Gammas { get; } = [];
    public List<d2.Effects.Brightness> Brightnesses { get; } = [];

    public void SetInput(d2.Image effect) => Inputs.ForEach(i => i.SetInput(0, effect, true));
    public void SetBlur(float amount) => Blurs.ForEach(b => b.StandardDeviation = amount);
    public void SetGamma(RawVector3 gamma) => Gammas.ForEach(g => { g.RedAmplitude = gamma.X; g.GreenAmplitude = gamma.Y; g.BlueAmplitude = gamma.Z; });
    public void SetBrightness((RawVector2 Black, RawVector2 White) brightness) => Brightnesses.ForEach(b => { b.BlackPoint = brightness.Black; b.WhitePoint = brightness.White; });

    public T TrackInput<T>(T effect) where T : d2.Effect
    {
        Inputs.Add(effect);
        return effect;
    }

    public T TrackOutput<T>(T effect) where T : d2.Effect
    {
        Outputs.Add(effect);
        return effect;
    }

    public T TrackBlur<T>(T effect) where T : d2.Effects.GaussianBlur
    {
        Blurs.Add(effect);
        return effect;
    }

    public T TrackGamma<T>(T effect) where T : d2.Effects.GammaTransfer
    {
        Gammas.Add(effect);
        return effect;
    }

    public T TrackBrightness<T>(T effect) where T : d2.Effects.Brightness
    {
        Brightnesses.Add(effect);
        return effect;
    }
}

public static class EffectBoxExtensions
{
    public static d2.Effect TrackBlur(this d2.Effect effect, EffectBox box) => box.TrackBlur((d2.Effects.GaussianBlur)effect);
    public static d2.Effect TrackGamma(this d2.Effect effect, EffectBox box) => box.TrackGamma((d2.Effects.GammaTransfer)effect);
    public static d2.Effect TrackBrightness(this d2.Effect effect, EffectBox box) => box.TrackBrightness((d2.Effects.Brightness)effect);
}


public readonly record struct TextureHelper(d3d.Texture2D Texture, dxgi.Surface Surface, d2.Bitmap1 Bitmap) : IDisposable
{
    public void Dispose()
    {
        Bitmap.Dispose();
        Surface.Dispose();
        Texture.Dispose();
    }
}

enum TextureHelperType
{
    Write,
    ReadWrite,
    Stage,
}