using SharpDX;
using SharpDX.Mathematics.Interop;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using d2 = SharpDX.Direct2D1;
using d3d = SharpDX.Direct3D11;
using dxgi = SharpDX.DXGI;
using sd = System.Drawing;
using sdi = System.Drawing.Imaging;

namespace ShaderTests;

/// <summary>
/// Provides access to frame-by-frame updates of a particular desktop (i.e. one monitor), with image and cursor information.
/// </summary>
public class DesktopDuplicator
{
    public dxgi.Adapter1 dxgiAdapter { get; }
    public dxgi.Factory1 dxgiFactory { get; }
    public d3d.Device1 d3dDevice { get; }
    public d3d.DeviceContext1 d3dContext { get; }
    public dxgi.Device dxgiDevice { get; }
    public d2.Device d2dDevice { get; }
    public d2.DeviceContext d2dContext { get; }

    private dxgi.OutputDuplication? _outputDuplication;

    private TextureHelper _miniImageWrite;
    private TextureHelper _miniImageStage;

    public Size2 OutputTexSize { get; } = new(1280, 720);

    public ImmutableArray<dxgi.Output> Outputs { get; }

    private dxgi.Output? _activeOutput = null;
    private dxgi.OutputDescription _activeOutputDescription;

    public dxgi.Output? SelectedOutput { get; set; }
    public sd.Bitmap MiniImage { get; private set; }
    public TextureHelper OutTexture => _miniImageStage;

    public d2.Effect? InputEffect { get; set; }
    public d2.Effect? OutputEffect { get; set; }

    public event Action? RebuildPipeline;

    public void RequestRebuild() => _activeOutput = null;

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

        _miniImageWrite = CreateTexture(OutputTexSize, TextureHelperType.Write);
        _miniImageStage = CreateTexture(OutputTexSize, TextureHelperType.Stage);

        Outputs = [.. dxgiDevice.Adapter.Outputs];
        SelectedOutput = Outputs.FirstOrDefault();

        MiniImage = new sd.Bitmap(OutputTexSize.Width, OutputTexSize.Height, sdi.PixelFormat.Format32bppRgb);
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

            InputEffect = null;
            OutputEffect = null;
            RebuildPipeline?.Invoke();
        }
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

        d2.Image output = desktopBitmap;

        if (InputEffect != null && OutputEffect != null)
        {
            InputEffect.SetInput(0, output, true);
            output = OutputEffect.Output;
        }

        d2dContext.Target = _miniImageWrite.Bitmap;
        d2dContext.BeginDraw();
        d2dContext.DrawImage(output, new RawVector2(0, 0), d2.InterpolationMode.Linear, d2.CompositeMode.SourceOver);
        d2dContext.EndDraw();

        d3dContext.CopyResource(_miniImageWrite.Texture, _miniImageStage.Texture);

        return new FrameInfo(_outputDuplication.ReleaseFrame);
    }

    private unsafe void WriteMiniInputBuffer()
    {
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
}

readonly struct FrameInfo(Action? release) : IDisposable
{
    public static readonly FrameInfo Empty = new(null);
    public readonly bool Ok => release != null;
    public readonly void Dispose() => release?.Invoke();
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

public class DesktopDuplicationException : Exception
{
    public DesktopDuplicationException(string message)
        : base(message) { }
}