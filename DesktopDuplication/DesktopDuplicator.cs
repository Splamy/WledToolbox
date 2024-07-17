using SharpDX;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Immutable;
using System.Linq;
using d2 = SharpDX.Direct2D1;
using d3d = SharpDX.Direct3D11;
using dxgi = SharpDX.DXGI;
using sd = System.Drawing;
using sdi = System.Drawing.Imaging;

namespace DesktopDuplication
{
    /// <summary>
    /// Provides access to frame-by-frame updates of a particular desktop (i.e. one monitor), with image and cursor information.
    /// </summary>
    public class DesktopDuplicator
    {
        private d3d.Device1 d3dDevice;
        private d3d.DeviceContext1 d3dContext;
        private dxgi.Device dxgiDevice;
        private d2.Device d2dDevice;
        private d2.DeviceContext d2dContext;
        private dxgi.OutputDuplication? _outputDuplication;

        private d3d.Texture2D _readTexture2d = null;
        private d3d.Texture2D _intermediateTexture2d;
        private d3d.Texture2D _intermediateTextureAlt2d;

        private Size2 _readTextureSize = new(239, 100);

        public ImmutableArray<dxgi.Output> Outputs { get; }

        private dxgi.Output? _activeOutput = null;
        private dxgi.OutputDescription _activeOutputDescription;
        public dxgi.Output? SelectedOutput { get; set; }

        public sd.Bitmap GdiOutImage { get; }

        public float BlurAmount { get; set; } = 3f;

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
            dxgi.Adapter1 adapter;
            try
            {
                adapter = new dxgi.Factory1().GetAdapter1(whichGraphicsCardAdapter);
            }
            catch (SharpDXException)
            {
                throw new DesktopDuplicationException("Could not find the specified graphics card adapter.");
            }
            var device = new d3d.Device(adapter, d3d.DeviceCreationFlags.VideoSupport | d3d.DeviceCreationFlags.BgraSupport);
            d3dDevice = device.QueryInterface<d3d.Device1>();

            d3dContext = d3dDevice.ImmediateContext.QueryInterface<d3d.DeviceContext1>();

            _intermediateTexture2d = new d3d.Texture2D(d3dDevice, new d3d.Texture2DDescription()
            {
                CpuAccessFlags = d3d.CpuAccessFlags.None,
                // RenderTarget allows writing, ShaderResource allows reading
                BindFlags = d3d.BindFlags.RenderTarget | d3d.BindFlags.ShaderResource,
                Format = dxgi.Format.B8G8R8A8_UNorm,
                Width = _readTextureSize.Width,
                Height = _readTextureSize.Height,
                OptionFlags = d3d.ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = d3d.ResourceUsage.Default,
            });

            _intermediateTextureAlt2d = new d3d.Texture2D(d3dDevice, new d3d.Texture2DDescription()
            {
                CpuAccessFlags = d3d.CpuAccessFlags.None,
                BindFlags = d3d.BindFlags.RenderTarget,
                Format = dxgi.Format.B8G8R8A8_UNorm,
                Width = _readTextureSize.Width,
                Height = _readTextureSize.Height,
                OptionFlags = d3d.ResourceOptionFlags.Shared,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = d3d.ResourceUsage.Default,
            });

            _readTexture2d = new d3d.Texture2D(d3dDevice, new d3d.Texture2DDescription()
            {
                // Read lets the cpu read the texture after processing
                CpuAccessFlags = d3d.CpuAccessFlags.Read,
                BindFlags = d3d.BindFlags.None,
                Format = dxgi.Format.B8G8R8A8_UNorm,
                Width = _readTextureSize.Width,
                Height = _readTextureSize.Height,
                OptionFlags = d3d.ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                // Staging lets the cpu read the texture after processing
                Usage = d3d.ResourceUsage.Staging,
            });

            dxgiDevice = d3dDevice.QueryInterface<dxgi.Device>();
            d2dDevice = new d2.Device(dxgiDevice);
            d2dContext = new d2.DeviceContext(d2dDevice, d2.DeviceContextOptions.None);

            Outputs = [.. dxgiDevice.Adapter.Outputs];
            SelectedOutput = Outputs.FirstOrDefault();

            GdiOutImage = new sd.Bitmap(_readTextureSize.Width, _readTextureSize.Height, sdi.PixelFormat.Format32bppRgb);
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
            if (!RetrieveFrame())
                return false;

            try
            {
                ProcessFrame();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                ReleaseFrame();
            }
        }

        private void CheckCurrentOutputDevice()
        {
            if (SelectedOutput != _activeOutput)
            {
                if (_outputDuplication != null)
                {
                    _outputDuplication.Dispose();
                    _outputDuplication = null;
                }
                _activeOutput = null;
                _activeOutputDescription = default;

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
            }
        }

        private bool RetrieveFrame()
        {
            var result = _outputDuplication.TryAcquireNextFrame(500, out var frameInfo, out var desktopResource);

            if (result.Failure)
            {
                if (result.Code == dxgi.ResultCode.AccessLost.Result.Code)
                {
                    _activeOutput = null;
                }

                return false;
            }

            using var __desktopResource = desktopResource;
            //using var desktopTexture2d = desktopResource.QueryInterface<d3d.Texture2D>();
            using var desktopSurface = desktopResource.QueryInterface<dxgi.Surface>();
            using var desktopBitmap = new d2.Bitmap1(d2dContext, desktopSurface);

            //using var intermediateSurface = _intermediateTexture2d.QueryInterface<dxgi.Surface>();
            //using var intermediateBitmap = new d2.Bitmap1(d2dContext, intermediateSurface);

            using var intermediateAltSurface = _intermediateTextureAlt2d.QueryInterface<dxgi.Surface>();
            using var intermediateAltBitmap = new d2.Bitmap1(d2dContext, intermediateAltSurface);

            var small = _readTextureSize;
            d2.Image pipeImage = desktopBitmap;

            var cropEffect = new d2.Effects.Crop(d2dContext);
            cropEffect.SetInput(0, pipeImage, true);
            cropEffect.Rectangle = new RawVector4(0, 0,
                _activeOutputDescription.DesktopBounds.Width(),
                _activeOutputDescription.DesktopBounds.Height() / 2);
            pipeImage = cropEffect.Output;

            var scaleEffect = new d2.Effects.Scale(d2dContext);
            scaleEffect.SetInput(0, pipeImage, true);
            scaleEffect.InterpolationMode = d2.InterpolationMode.HighQualityCubic;
            scaleEffect.CenterPoint = new RawVector2(0, 0);
            scaleEffect.ScaleAmount = new RawVector2(
                small.Width / (float)_activeOutputDescription.DesktopBounds.Width(),
                small.Height / (float)_activeOutputDescription.DesktopBounds.Height());
            pipeImage = scaleEffect.Output;

            if (BlurAmount > 0)
            {
                var gaussianBlurEffect = new d2.Effects.GaussianBlur(d2dContext);
                gaussianBlurEffect.SetInput(0, pipeImage, true);
                gaussianBlurEffect.StandardDeviation = BlurAmount;
                pipeImage = gaussianBlurEffect.Output;
            }

            d2dContext.Target = intermediateAltBitmap;
            d2dContext.BeginDraw();
            d2dContext.DrawImage(pipeImage, d2.InterpolationMode.Linear, d2.CompositeMode.SourceOver);
            //d2dContext.DrawBitmap(desktopBitmap, new RawRectangleF(0, 0, small.Width, small.Height), 1, d2.InterpolationMode.HighQualityCubic, null, null);
            d2dContext.EndDraw();

            //d2dContext.Target = intermediateAltBitmap;
            //d2dContext.BeginDraw();
            //d2dContext.DrawBitmap(intermediateBitmap, small, 1, d2.InterpolationMode.HighQualityCubic, small, null);
            //d2dContext.DrawBitmap(intermediateBitmap, new RawRectangleF(small.Right, 0, small.Right + 300, 300), 1, d2.InterpolationMode.NearestNeighbor, small, null);
            //d2dContext.EndDraw();

            d3dContext.CopyResource(_intermediateTextureAlt2d, _readTexture2d);

            return true;
        }

        private void ProcessFrame()
        {
            // Get the desktop capture texture
            var mapSource = d3dContext.MapSubresource(_readTexture2d, 0, d3d.MapMode.Read, d3d.MapFlags.None);

            var boundsRect = new sd.Rectangle(0, 0, GdiOutImage.Width, GdiOutImage.Height);
            // Copy pixels from screen capture Texture to GDI bitmap
            var mapDest = GdiOutImage.LockBits(boundsRect, sdi.ImageLockMode.WriteOnly, GdiOutImage.PixelFormat);
            var sourcePtr = mapSource.DataPointer;
            var destPtr = mapDest.Scan0;
            for (int y = 0; y < GdiOutImage.Height; y++)
            {
                // Copy a single line 
                Utilities.CopyMemory(destPtr, sourcePtr, GdiOutImage.Width * 4);

                // Advance pointers
                sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);
                destPtr = IntPtr.Add(destPtr, mapDest.Stride);
            }

            // Release source and dest locks
            GdiOutImage.UnlockBits(mapDest);
            d3dContext.UnmapSubresource(_readTexture2d, 0);
        }

        private void ReleaseFrame()
        {
            try
            {
                _outputDuplication.ReleaseFrame();
            }
            catch (SharpDXException ex)
            {
                if (ex.ResultCode.Failure)
                {
                    throw new DesktopDuplicationException("Failed to release frame.");
                }
            }
        }
    }
}
