using DesktopDuplication;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WledToolbox;

public partial class FormDemo : Form
{
    private PeriodicTimer updateTimer = new(TimeSpan.FromMilliseconds(1000 / 60));
    private DesktopDuplicator desktopDuplicator;
    private Bitmap inputBitmap;
    private Bitmap outputBitmap;
    private WledCore wled = new();

    public FormDemo()
    {
        InitializeComponent();

        desktopDuplicator = new DesktopDuplicator();

        inputBitmap = new Bitmap(
            desktopDuplicator.InputTexSize.Width,
            desktopDuplicator.InputTexSize.Height,
            PixelFormat.Format32bppRgb);
        inputDebugPicture.Image = inputBitmap;
        outputBitmap = new Bitmap(
            desktopDuplicator.OutputTexSize.Width,
            desktopDuplicator.OutputTexSize.Height,
            PixelFormat.Format32bppRgb);
        outputDebugPicture.Image = outputBitmap;

        selectMonitorDropDown.Format += (s, e) => e.Value = ScreenHelper.Format((Output)e.ListItem);
        selectMonitorDropDown.Items.AddRange(desktopDuplicator.Outputs.Cast<object>().ToArray());
        selectMonitorDropDown.SelectedItem = desktopDuplicator.SelectedOutput;

        brightnessPointPicker.PointTransform = Matrix3x2.CreateScale(1, -1) * Matrix3x2.CreateTranslation(0, 1);
        brightnessPointPicker.Points.AddRange([
            new PointPicker2D.PointData(Brushes.Black, new Vector2(0, 0)) {
                Constraint = (all, p) => Vector2.Clamp(p, Vector2.Zero, all[1].Point - new Vector2(0.001f, 0f))
            },
            new PointPicker2D.PointData(Brushes.White, new Vector2(1, 1)) {
                Constraint = (all, p) => Vector2.Clamp(p, all[0].Point + new Vector2(0.001f, 0f), Vector2.One)
            },
        ]);
        brightnessPointPicker.PointsChanged += () =>
        {
            desktopDuplicator.Brightness = (
                new RawVector2(brightnessPointPicker.Points[0].Point.X, brightnessPointPicker.Points[0].Point.Y),
                new RawVector2(brightnessPointPicker.Points[1].Point.X, brightnessPointPicker.Points[1].Point.Y));
        };
        brightnessPointPicker.PreDraw += (g) =>
        {
            var uiZero = brightnessPointPicker.ToUiCoordinates(Vector2.Zero);
            var uiOne = brightnessPointPicker.ToUiCoordinates(Vector2.One);
            var uiPtBlack = brightnessPointPicker.ToUiCoordinates(brightnessPointPicker.Points[0].Point);
            var uiPtWhite = brightnessPointPicker.ToUiCoordinates(brightnessPointPicker.Points[1].Point);

            g.DrawLine(Pens.Magenta, uiZero.X, uiPtBlack.Y, uiPtBlack.X, uiPtBlack.Y);
            g.DrawLine(Pens.Magenta, uiPtBlack.X, uiPtBlack.Y, uiPtWhite.X, uiPtWhite.Y);
            g.DrawLine(Pens.Magenta, uiPtWhite.X, uiPtWhite.Y, uiOne.X, uiPtWhite.Y);
        };
    }

    private void FormDemo_Shown(object sender, EventArgs e)
    {
        _ = Task.Run(WorkTask);
    }

    private async Task WorkTask()
    {
        while (await updateTimer.WaitForNextTickAsync())
        {
            framtimeView1.TrackWait();
            if (!desktopDuplicator.GetLatestFrame())
            {
                continue;
            }
            framtimeView1.TrackProcess();

            ShowDebugInputImage();
            ShowDebugOutputImage();

            if (sendWledDataCheckbox.Checked)
            {
                await wled.Send(desktopDuplicator.OutData);
                framtimeView1.TrackProcess();
                framtimeView1.AddFrame();
            }
        }
    }

    private unsafe void ShowDebugInputImage()
    {
        if (!checkDebugInputImage.Checked)
        {
            return;
        }

        if (!inputDebugPicture.PaintLock.TryEnter())
        {
            return;
        }

        try
        {
            using var g = Graphics.FromImage(inputBitmap);
            g.DrawImageUnscaled(desktopDuplicator.MiniImage, 0, 0);
        }
        finally
        {
            inputDebugPicture.PaintLock.Exit();
        }

        Invoke(() =>
        {
            inputDebugPicture.Refresh();
        });
    }

    private unsafe void ShowDebugOutputImage()
    {
        if (!checkDebugOutputImage.Checked)
        {
            return;
        }

        if (!outputDebugPicture.PaintLock.TryEnter())
        {
            return;
        }

        var boundsRect = new Rectangle(0, 0, outputBitmap.Width, outputBitmap.Height);
        var mapDest = outputBitmap.LockBits(boundsRect, ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);

        try
        {
            fixed (BGRAPixel* pixelData = desktopDuplicator.OutData)
            {
                nint srcPtr = (nint)pixelData;
                nint destPtr = mapDest.Scan0;

                for (int y = 0; y < outputBitmap.Height; y++)
                {
                    Utilities.CopyMemory(destPtr, srcPtr, outputBitmap.Width * 4);
                    srcPtr += outputBitmap.Width * 4;
                    destPtr += mapDest.Stride;
                }
            }
        }
        finally
        {
            outputBitmap.UnlockBits(mapDest);
            outputDebugPicture.PaintLock.Exit();
        }

        Invoke(() =>
        {
            outputDebugPicture.Refresh();
        });
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
    private void cropLeft_ValueChanged(object sender, EventArgs e) => crop_ValueChanged(sender, e);
    private void cropTop_ValueChanged(object sender, EventArgs e) => crop_ValueChanged(sender, e);
    private void cropRight_ValueChanged(object sender, EventArgs e) => crop_ValueChanged(sender, e);
    private void cropBottom_ValueChanged(object sender, EventArgs e) => crop_ValueChanged(sender, e);
    private void crop_ValueChanged(object sender, EventArgs e)
    {
        desktopDuplicator.PreCrop = new RawRectangle(
            (int)cropLeft.Value, (int)cropTop.Value,
            (int)cropRight.Value, (int)cropBottom.Value);
    }

    private void checkDebugInputImage_CheckedChanged(object sender, EventArgs e)
    {
        desktopDuplicator.CopyMiniImageOut = checkDebugInputImage.Checked;
    }
}
