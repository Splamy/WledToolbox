using SharpDX.DXGI;
using System;
using System.Buffers.Binary;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DesktopDuplication.Demo
{
    public partial class FormDemo : Form
    {
        private PeriodicTimer updateTimer = new(TimeSpan.FromMilliseconds(1000 / 60));
        private DesktopDuplicator desktopDuplicator;
        private Bitmap bitmap;

        private readonly UdpClient udp = new();
        private readonly IPEndPoint ep = new IPEndPoint(IPAddress.Parse("192.168.178.57"), 21324);
        private const int MaxLedPerPacket = 489;
        private const int SendLedsPerPacket = 239 + 196;
        private const int leds = 239 * 2 + 196 * 2; // = 870
        private readonly byte[] sendBuf = new byte[2 + 2 + leds * 3];

        public FormDemo()
        {
            InitializeComponent();

            try
            {
                desktopDuplicator = new DesktopDuplicator();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            bitmap = new Bitmap(desktopDuplicator.GdiOutImage.Width, desktopDuplicator.GdiOutImage.Height, PixelFormat.Format32bppRgb);
            pictureBox2.Image = bitmap;

            selectMonitorDropDown.Format += (s, e) => e.Value = ((Output)e.ListItem).Description.DeviceName;
            selectMonitorDropDown.Items.AddRange(desktopDuplicator.Outputs.Cast<object>().ToArray());
            selectMonitorDropDown.SelectedItem = desktopDuplicator.SelectedOutput;
        }

        private void FormDemo_Shown(object sender, EventArgs e)
        {
            _ = Task.Run(WorkTask);
        }

        private async Task WorkTask()
        {
            while (await updateTimer.WaitForNextTickAsync())
            {
                if (!desktopDuplicator.GetLatestFrame())
                {
                    continue;
                }

                if (checkDebugOutputImage.Checked)
                {
                    if (Monitor.TryEnter(pictureBox2.PaintLock))
                    {
                        try
                        {
                            using var g = Graphics.FromImage(bitmap);
                            g.DrawImage(desktopDuplicator.GdiOutImage, 0, 0);
                        }
                        finally
                        {
                            Monitor.Exit(pictureBox2.PaintLock);
                        }
                    }

                    Invoke(() =>
                    {
                        pictureBox2.Refresh();
                    });
                }

                if (sendWledDataCheckbox.Checked)
                {
                    await SendData();
                }
            }
        }

        private async Task SendData()
        {
            // Syncs are:
            // 1)
            // Front: 239 LEDs
            // Left: 196 LEDs
            // 2)
            // Back: 239 LEDs
            // Right: 196 LEDs

            var image = desktopDuplicator.GdiOutImage;

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

                void SetInv(int offset, Span<BGRAPixel> pixels)
                {
                    for (int i = 1; i < pixels.Length; i++)
                    {
                        var pixel = pixels[^i];
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
                    var pixelsR0 = new Span<BGRAPixel>((void*)bitMapData.Scan0, bitMapData.Width * 3);
                    var frontSpan = pixelsR0[(239 * 0)..(239 * 0 + 239)];
                    var leftSpan = pixelsR0[(239 * 1)..(239 * 1 + 196)];

                    // Front
                    SetInv(4 + 0, frontSpan);
                    // Left
                    SetInv(4 + (239 * 3), leftSpan);

                    sendBuf[2] = 0x00;
                    sendBuf[3] = 0x00;
                }

                Map1();

                await udp.SendAsync(sendBuf.AsMemory(0, 4 + SendLedsPerPacket * 3), ep);

                unsafe void Map2()
                {
                    var pixelsR0 = new Span<BGRAPixel>((void*)bitMapData.Scan0, bitMapData.Width * 3);
                    var rightSpan = pixelsR0[(239 * 2)..(239 * 2 + 196)];

                    // Back
                    Fill(4 + 0, 239, default);
                    // Right
                    SetInv(4 + (239 * 3), rightSpan);

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
            }
            finally
            {
                image.UnlockBits(bitMapData);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            desktopDuplicator.SelectedOutput = (Output)selectMonitorDropDown.SelectedItem;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            desktopDuplicator.BlurAmount = trackBar1.Value;
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