using System;

using SharpDX;
using SharpDX.Mathematics.Interop;

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
        private d3d.Texture2DDescription _readTextureDescr;
        private d3d.Texture2DDescription _intermediateTextureDescr;
        private d3d.Texture2DDescription _intermediateTextureAltDescr;
        private dxgi.Device dxgiDevice;
        private d2.Device d2dDevice;
        private d2.DeviceContext d2dContext;
        private dxgi.OutputDescription mOutputDesc;
        private dxgi.OutputDuplication mDeskDupl;

        private d3d.Texture2D _readTexture2d = null;
        private d3d.Texture2D _intermediateTexture2d;
        private d3d.Texture2D _intermediateTextureAlt2d;
        private int mWhichOutputDevice = -1;

        private readonly sd.Bitmap gdiOutImage;

        private Size2 _readTextureSize = new(239, 2);

        /// <summary>
        /// Duplicates the output of the specified monitor.
        /// </summary>
        /// <param name="whichMonitor">The output device to duplicate (i.e. monitor). Begins with zero, which seems to correspond to the primary monitor.</param>
        public DesktopDuplicator(int whichMonitor)
            : this(0, whichMonitor) { }

        /// <summary>
        /// Duplicates the output of the specified monitor on the specified graphics adapter.
        /// </summary>
        /// <param name="whichGraphicsCardAdapter">The adapter which contains the desired outputs.</param>
        /// <param name="whichOutputDevice">The output device to duplicate (i.e. monitor). Begins with zero, which seems to correspond to the primary monitor.</param>
        public DesktopDuplicator(int whichGraphicsCardAdapter, int whichOutputDevice)
        {
            this.mWhichOutputDevice = whichOutputDevice;
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

            dxgi.Output output;
            try
            {
                output = adapter.GetOutput(whichOutputDevice);
            }
            catch (SharpDXException)
            {
                throw new DesktopDuplicationException("Could not find the specified output device.");
            }

            var output1 = output.QueryInterface<dxgi.Output1>();
            this.mOutputDesc = output.Description;

            _intermediateTextureDescr = new d3d.Texture2DDescription()
            {
                CpuAccessFlags = d3d.CpuAccessFlags.None,
                BindFlags = d3d.BindFlags.RenderTarget | d3d.BindFlags.ShaderResource,
                Format = dxgi.Format.B8G8R8A8_UNorm,
                Width = _readTextureSize.Width,
                Height = _readTextureSize.Height,
                OptionFlags = d3d.ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = d3d.ResourceUsage.Default,
            };

            _intermediateTextureAltDescr = new d3d.Texture2DDescription()
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
            };

            _readTextureDescr = new d3d.Texture2DDescription()
            {
                CpuAccessFlags = d3d.CpuAccessFlags.Read,
                BindFlags = d3d.BindFlags.None,
                Format = dxgi.Format.B8G8R8A8_UNorm,
                Width = _readTextureSize.Width,
                Height = _readTextureSize.Height,
                OptionFlags = d3d.ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = d3d.ResourceUsage.Staging,
            };

            dxgiDevice = d3dDevice.QueryInterface<dxgi.Device>();
            d2dDevice = new d2.Device(dxgiDevice);
            d2dContext = new d2.DeviceContext(d2dDevice, d2.DeviceContextOptions.None);

            try
            {
                this.mDeskDupl = output1.DuplicateOutput(this.d3dDevice);
            }
            catch (SharpDXException ex)
            {
                if (ex.ResultCode.Code == dxgi.ResultCode.NotCurrentlyAvailable.Result.Code)
                {
                    throw new DesktopDuplicationException("There is already the maximum number of applications using the Desktop Duplication API running, please close one of the applications and try again.");
                }
            }

            gdiOutImage = new sd.Bitmap(_readTextureSize.Width, _readTextureSize.Height, sdi.PixelFormat.Format32bppRgb);
        }

        /// <summary>
        /// Retrieves the latest desktop image and associated metadata.
        /// </summary>
        public DesktopFrame GetLatestFrame()
        {
            var frame = new DesktopFrame();
            // Try to get the latest frame; this may timeout
            bool retrievalTimedOut = RetrieveFrame();
            if (retrievalTimedOut)
                return null;

            try
            {
                ProcessFrame(frame);
                return frame;
            }
            catch
            {
                return null;
            }
            finally
            {
                ReleaseFrame();
            }
        }

        private bool RetrieveFrame()
        {
            _readTexture2d ??= new d3d.Texture2D(d3dDevice, _readTextureDescr);
            _intermediateTexture2d ??= new d3d.Texture2D(d3dDevice, _intermediateTextureDescr);
            _intermediateTextureAlt2d ??= new d3d.Texture2D(d3dDevice, _intermediateTextureAltDescr);

            var result = mDeskDupl.TryAcquireNextFrame(500, out var frameInfo, out var desktopResource);

            if (result.Failure)
            {
                if (result.Code == dxgi.ResultCode.WaitTimeout.Result.Code)
                {
                    return true;
                }
                if (result.Code == dxgi.ResultCode.AccessLost.Result.Code)
                {
                    _readTexture2d.Dispose();
                    _readTexture2d = null;
                    return false;
                }
                throw new DesktopDuplicationException("Failed to acquire next frame.");
            }

            using var __desktopResource = desktopResource;
            //using var desktopTexture2d = desktopResource.QueryInterface<d3d.Texture2D>();
            using var desktopSurface = desktopResource.QueryInterface<dxgi.Surface>();
            using var desktopBitmap = new d2.Bitmap1(d2dContext, desktopSurface);

            //using var intermediateSurface = _intermediateTexture2d.QueryInterface<dxgi.Surface>();
            //using var intermediateBitmap = new d2.Bitmap1(d2dContext, intermediateSurface);

            using var intermediateAltSurface = _intermediateTextureAlt2d.QueryInterface<dxgi.Surface>();
            using var intermediateAltBitmap = new d2.Bitmap1(d2dContext, intermediateAltSurface);

            //var gaussianBlurEffect = new d2.Effects.GaussianBlur(d2dContext);
            //gaussianBlurEffect.SetInput(0, desktopBitmap, true);
            //gaussianBlurEffect.StandardDeviation = 5f;

            //var scaleEffect = new d2.Effects.Scale(d2dContext);
            //scaleEffect.SetInput(0, gaussianBlurEffect.Output, true);
            //scaleEffect.CenterPoint = new RawVector2(0, 0);
            //scaleEffect.ScaleAmount = new RawVector2(mOutputDesc.DesktopBounds.Width(), 1);

            var small = _readTextureSize;

            d2dContext.Target = intermediateAltBitmap;
            d2dContext.BeginDraw();
            //d2dContext.DrawImage(desktopBitmap, d2.InterpolationMode.HighQualityCubic, d2.CompositeMode.SourceOver);
            d2dContext.DrawBitmap(desktopBitmap, new RawRectangleF(0, 0, small.Width, small.Height), 1, d2.InterpolationMode.HighQualityCubic, null, null);
            d2dContext.EndDraw();

            //d2dContext.Target = intermediateAltBitmap;
            //d2dContext.BeginDraw();
            //d2dContext.DrawBitmap(intermediateBitmap, small, 1, d2.InterpolationMode.HighQualityCubic, small, null);
            //d2dContext.DrawBitmap(intermediateBitmap, new RawRectangleF(small.Right, 0, small.Right + 300, 300), 1, d2.InterpolationMode.NearestNeighbor, small, null);
            //d2dContext.EndDraw();

            d3dDevice.ImmediateContext.CopyResource(_intermediateTextureAlt2d, _readTexture2d);

            return false;
        }

        private void ProcessFrame(DesktopFrame frame)
        {
            // Get the desktop capture texture
            var mapSource = d3dDevice.ImmediateContext.MapSubresource(_readTexture2d, 0, d3d.MapMode.Read, d3d.MapFlags.None);

            var boundsRect = new sd.Rectangle(0, 0, gdiOutImage.Width, gdiOutImage.Height);
            // Copy pixels from screen capture Texture to GDI bitmap
            var mapDest = gdiOutImage.LockBits(boundsRect, sdi.ImageLockMode.WriteOnly, gdiOutImage.PixelFormat);
            var sourcePtr = mapSource.DataPointer;
            var destPtr = mapDest.Scan0;
            for (int y = 0; y < gdiOutImage.Height; y++)
            {
                // Copy a single line 
                Utilities.CopyMemory(destPtr, sourcePtr, gdiOutImage.Width * 4);

                // Advance pointers
                sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);
                destPtr = IntPtr.Add(destPtr, mapDest.Stride);
            }

            // Release source and dest locks
            gdiOutImage.UnlockBits(mapDest);
            d3dDevice.ImmediateContext.UnmapSubresource(_readTexture2d, 0);
            frame.DesktopImage = gdiOutImage;
        }

        private void ReleaseFrame()
        {
            try
            {
                mDeskDupl.ReleaseFrame();
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
