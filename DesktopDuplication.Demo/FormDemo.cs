using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Buffers.Binary;

namespace DesktopDuplication.Demo
{
    public partial class FormDemo : Form
    {
        private PeriodicTimer updateTimer = new(TimeSpan.FromMilliseconds(1000 / 30));
        private DesktopDuplicator desktopDuplicator;
        private Bitmap bitmap = new Bitmap(239, 2, PixelFormat.Format32bppRgb);

        public FormDemo()
        {
            InitializeComponent();

            try
            {
                desktopDuplicator = new DesktopDuplicator(2);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }


        }

        private void FormDemo_Shown(object sender, EventArgs e)
        {
            _ = Task.Run(WorkTask);
        }

        private async Task WorkTask()
        {
            using var udp = new UdpClient();
            var ep = new IPEndPoint(IPAddress.Parse("192.168.178.57"), 21324);
            const int MaxLedPerPacket = 489;
            const int SendLedsPerPacket = 239 + 196;
            const int leds = 239 * 2 + 196 * 2; // = 870
            byte[] sendBuf = new byte[2 + 2 + leds * 3];

            // Syncs are:
            // 1)
            // Front: 239 LEDs
            // Left: 196 LEDs
            // 2)
            // Back: 239 LEDs
            // Right: 196 LEDs


            while (await updateTimer.WaitForNextTickAsync())
            {
                DesktopFrame frame = desktopDuplicator.GetLatestFrame();

                if (frame == null)
                {
                    continue;
                }

                var image = frame.DesktopImage;
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.DrawImage(image, 0, 0, 239, 2);
                }

                //Invoke(() =>
                //{
                //    this.BackgroundImage = bitmap;
                //    this.Invalidate();
                //});

                var bitMapData = image.LockBits(
                    new Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.ReadOnly,
                    image.PixelFormat);

                try
                {


                    void Set(int offset, Span<BGRAPixel> pixels)
                    {
                        for (int i = 0; i < pixels.Length; i++)
                        {
                            var pixel = pixels[i];
                            sendBuf[offset + i * 3 + 0] = pixel.R;
                            sendBuf[offset + i * 3 + 1] = pixel.G;
                            sendBuf[offset + i * 3 + 2] = pixel.B;
                        }
                    }
                    void Fill(int offset, int length, BGRAPixel color)
                    {
                        for (int i = 0; i < length; i++)
                        {
                            sendBuf[offset + i * 3 + 0] = color.R;
                            sendBuf[offset + i * 3 + 1] = color.G;
                            sendBuf[offset + i * 3 + 2] = color.B;
                        }
                    }

                    sendBuf[0] = 0x04; // DNRGB [Timout(s)] [Startindex XLow, XHigh] [R,G,B] * n
                    sendBuf[1] = 0x10;


                    unsafe void Map1()
                    {
                        var pixelsR0 = new Span<BGRAPixel>((void*)bitMapData.Scan0, bitMapData.Width * 2);
                        Span<BGRAPixel> tmpBuf = stackalloc BGRAPixel[239];
                        // write first 239 LEDs into tmpBuf reversed
                        for (int i = 0; i < 239; i++)
                        {
                            tmpBuf[i] = pixelsR0[239 - i];
                        }

                        Set(4 + 0, tmpBuf);
                        Fill(4 + (239 * 3), 196, pixelsR0[0]); 
                        
                        sendBuf[2] = 0x00;
                        sendBuf[3] = 0x00;
                    }

                    Map1();

                    await udp.SendAsync(sendBuf.AsMemory(0, 4 + SendLedsPerPacket * 3), ep);

                    unsafe void Map2()
                    {
                        var pixelsR0 = new Span<BGRAPixel>((void*)bitMapData.Scan0, bitMapData.Width * 2);

                        Set(4 + 0, pixelsR0[239..478]);
                        Fill(4 + (239 * 3), 196, pixelsR0[238]);

                        Span<byte> writeOffBytes = stackalloc byte[2];
                        BinaryPrimitives.WriteUInt16BigEndian(writeOffBytes, 239 + 196);

                        sendBuf[2] = writeOffBytes[0];
                        sendBuf[3] = writeOffBytes[1];
                    }

                    Map2();

                    await udp.SendAsync(sendBuf.AsMemory(0, 4 + SendLedsPerPacket * 3), ep);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Invalid JSON: {0}", ex.Message);
                    continue;
                }
                finally
                {
                    image.UnlockBits(bitMapData);
                }
            }
        }
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
struct BGRAPixel
{
    public byte B;
    public byte G;
    public byte R;
    public byte A;
}