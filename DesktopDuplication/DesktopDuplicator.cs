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

        private sd.Bitmap finalImage1, finalImage2;
        private bool isFinalImage1 = false;

        private sd.Bitmap FinalImage
        {
            get
            {
                return isFinalImage1 ? finalImage1 : finalImage2;
            }
            set
            {
                if (isFinalImage1)
                {
                    finalImage2 = value;
                    if (finalImage1 != null) finalImage1.Dispose();
                }
                else
                {
                    finalImage1 = value;
                    if (finalImage2 != null) finalImage2.Dispose();
                }
                isFinalImage1 = !isFinalImage1;
            }
        }

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
            _readTextureDescr = new d3d.Texture2DDescription()
            {
                CpuAccessFlags = d3d.CpuAccessFlags.Read,
                BindFlags = d3d.BindFlags.None,
                Format = dxgi.Format.B8G8R8A8_UNorm,
                Width = this.mOutputDesc.DesktopBounds.Width(),
                Height = this.mOutputDesc.DesktopBounds.Height(),
                OptionFlags = d3d.ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = d3d.ResourceUsage.Staging,
            };

            _intermediateTextureDescr = new d3d.Texture2DDescription()
            {
                CpuAccessFlags = d3d.CpuAccessFlags.None,
                BindFlags = d3d.BindFlags.RenderTarget | d3d.BindFlags.ShaderResource,
                Format = dxgi.Format.B8G8R8A8_UNorm,
                Width = this.mOutputDesc.DesktopBounds.Width(),
                Height = this.mOutputDesc.DesktopBounds.Height(),
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
                Width = this.mOutputDesc.DesktopBounds.Width(),
                Height = this.mOutputDesc.DesktopBounds.Height(),
                OptionFlags = d3d.ResourceOptionFlags.Shared,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = d3d.ResourceUsage.Default,
            };

            //var defaultDevice = new SharpDX.Direct3D11.Device(SharpDX.Direct3D.DriverType.Hardware,
            //                                      DeviceCreationFlags.VideoSupport
            //                                      | DeviceCreationFlags.BgraSupport
            //                                      | DeviceCreationFlags.None);
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

            using (var desktopTexture2d = desktopResource.QueryInterface<d3d.Texture2D>())
            {
                using var desktopSurface = desktopTexture2d.QueryInterface<dxgi.Surface>();
                using var desktopBitmap = new d2.Bitmap1(d2dContext, desktopSurface);

                using var intermediateSurface = _intermediateTexture2d.QueryInterface<dxgi.Surface>();
                using var intermediateBitmap = new d2.Bitmap1(d2dContext, intermediateSurface);

                var gaussianBlurEffect = new d2.Effects.GaussianBlur(d2dContext);
                gaussianBlurEffect.SetInput(0, desktopBitmap, true);
                gaussianBlurEffect.StandardDeviation = 5f;

                int size = 8;

                var small = new RawRectangleF(0, 0, 100, 100);

                d2dContext.Target = intermediateBitmap;
                d2dContext.BeginDraw();
                //d2dContext.DrawImage(desktopBitmap, d2.InterpolationMode.HighQualityCubic, d2.CompositeMode.SourceOver);
                d2dContext.DrawBitmap(desktopBitmap, small, 1, d2.InterpolationMode.HighQualityCubic, null, null);
                d2dContext.EndDraw();

                //d3dDevice.ImmediateContext.CopyResource(_intermediateTexture2d, _intermediateTextureAlt2d);

                using var intermediateAltSurface = _intermediateTextureAlt2d.QueryInterface<dxgi.Surface>();
                using var intermediateAltBitmap = new d2.Bitmap1(d2dContext, intermediateAltSurface);

                d2dContext.Target = intermediateAltBitmap;
                d2dContext.BeginDraw();
                d2dContext.DrawBitmap(intermediateBitmap, small, 1, d2.InterpolationMode.HighQualityCubic, small, null);
                d2dContext.DrawBitmap(intermediateBitmap, new RawRectangleF(small.Right, 0, small.Right + 300, 300), 1, d2.InterpolationMode.NearestNeighbor, small, null);
                d2dContext.EndDraw();


                d3dDevice.ImmediateContext.CopyResource(_intermediateTextureAlt2d, _readTexture2d);
            }


            desktopResource.Dispose();
            return false;
        }

        private void ProcessFrame(DesktopFrame frame)
        {
            // Get the desktop capture texture
            var mapSource = d3dDevice.ImmediateContext.MapSubresource(_readTexture2d, 0, d3d.MapMode.Read, d3d.MapFlags.None);

            FinalImage = new sd.Bitmap(mOutputDesc.DesktopBounds.Width(), mOutputDesc.DesktopBounds.Height(), sdi.PixelFormat.Format32bppRgb);
            var boundsRect = new sd.Rectangle(0, 0, mOutputDesc.DesktopBounds.Width(), mOutputDesc.DesktopBounds.Height());
            // Copy pixels from screen capture Texture to GDI bitmap
            var mapDest = FinalImage.LockBits(boundsRect, sdi.ImageLockMode.WriteOnly, FinalImage.PixelFormat);
            var sourcePtr = mapSource.DataPointer;
            var destPtr = mapDest.Scan0;
            for (int y = 0; y < mOutputDesc.DesktopBounds.Height(); y++)
            {
                // Copy a single line 
                Utilities.CopyMemory(destPtr, sourcePtr, mOutputDesc.DesktopBounds.Width() * 4);

                // Advance pointers
                sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);
                destPtr = IntPtr.Add(destPtr, mapDest.Stride);
            }

            // Release source and dest locks
            FinalImage.UnlockBits(mapDest);
            d3dDevice.ImmediateContext.UnmapSubresource(_readTexture2d, 0);
            frame.DesktopImage = FinalImage;
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