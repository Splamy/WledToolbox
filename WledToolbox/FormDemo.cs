using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
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
        private WledCore wled = new();

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
                    await wled.Send(desktopDuplicator.GdiOutImage);
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            desktopDuplicator.SelectedOutput = (Output)selectMonitorDropDown.SelectedItem;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            desktopDuplicator.BlurAmount = trackBar1.Value / 5f;
        }

        private void gammaR_Scroll(object sender, EventArgs e) => gamma_Scroll(sender, e);
        private void gammeG_Scroll(object sender, EventArgs e) => gamma_Scroll(sender, e);
        private void gammaB_Scroll(object sender, EventArgs e) => gamma_Scroll(sender, e);
        private void gamma_Scroll(object sender, EventArgs e)
        {
            if (gammaLock.Checked)
            {
                gammaR.Value = ((TrackBar)sender).Value;
                gammeG.Value = ((TrackBar)sender).Value;
                gammaB.Value = ((TrackBar)sender).Value;
            }
            SetGamma();
        }

        private void gammaLock_CheckedChanged(object sender, EventArgs e)
        {
            if (gammaLock.Checked)
            {
                gammeG.Value = gammaR.Value;
                gammaB.Value = gammaR.Value;
            }
            SetGamma();
        }

        private void SetGamma() => desktopDuplicator.Gamma = new RawVector3(gammaR.Value / 30f, gammeG.Value / 30f, gammaB.Value / 30f);

        private void blackLow_Scroll(object sender, EventArgs e) => brightness_Scroll(sender, e);
        private void blackHigh_Scroll(object sender, EventArgs e) => brightness_Scroll(sender, e);
        private void whiteLow_Scroll(object sender, EventArgs e) => brightness_Scroll(sender, e);
        private void whiteHigh_Scroll(object sender, EventArgs e) => brightness_Scroll(sender, e);
        private void brightness_Scroll(object sender, EventArgs e)
        {
            desktopDuplicator.Brightness = (
                new RawVector2(Math.Clamp(blackLow.Value / 100f, 0, 1), Math.Clamp(blackHigh.Value / 100f, 0, 1)),
                new RawVector2(Math.Clamp(whiteLow.Value / 100f, 0, 1), Math.Clamp(whiteHigh.Value / 100f, 0, 1)));
        }
    }
}
