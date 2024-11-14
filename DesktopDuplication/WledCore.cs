using System;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DesktopDuplication;

public class WledCore
{
    private readonly UdpClient udp = new();
    private readonly IPEndPoint ep = new IPEndPoint(IPAddress.Parse("192.168.178.57"), 21324);
    private const int MaxLedPerPacket = 489;
    private const int SendLedsPerPacket = 239 + 196;
    private const int leds = 239 * 2 + 196 * 2; // = 870
    private readonly byte[] sendBuf = new byte[2 + 2 + leds * 3];

    public async Task Send(Memory<BGRAPixel> image)
    {
        // Syncs are:
        // 1)
        // Front: 239 LEDs
        // Left: 196 LEDs
        // 2)
        // Back: 239 LEDs
        // Right: 196 LEDs

        //var image = desktopDuplicator.GdiOutImage;

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


            var pixelsR0 = image.Span;
            var frontSpan = pixelsR0[(239 * 0)..(239 * 0 + 239)];
            var leftSpan = pixelsR0[(239 * 1)..(239 * 1 + 196)];

            // Front
            SetInv(4 + 0, frontSpan);
            // Left
            SetInv(4 + (239 * 3), leftSpan);

            sendBuf[2] = 0x00;
            sendBuf[3] = 0x00;

            await udp.SendAsync(sendBuf.AsMemory(0, 4 + SendLedsPerPacket * 3), ep);


            pixelsR0 = image.Span;
            var rightSpan = pixelsR0[(239 * 2)..(239 * 2 + 196)];

            // Back
            Fill(4 + 0, 239, default);
            // Right
            SetInv(4 + (239 * 3), rightSpan);

            BinaryPrimitives.WriteUInt16BigEndian(sendBuf.AsSpan(2, 2), 239 + 196);

            await udp.SendAsync(sendBuf.AsMemory(0, 4 + SendLedsPerPacket * 3), ep);
        }
        catch (Exception ex)
        {
        }
    }
}


[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
public struct BGRAPixel
{
    public byte B;
    public byte G;
    public byte R;
    public byte A;
}