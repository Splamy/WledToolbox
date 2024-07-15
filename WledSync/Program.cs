using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Serialization;

// open websocket and listen

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseWebSockets();

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            await HandleSocket(webSocket);
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
    else
    {
        await next(context);
    }

});

app.MapGet("/", () => "Hello World!");

app.Run();


static async Task HandleSocket(WebSocket webSocket)
{
    var buffer = new byte[1024 * 4];

    using var udp = new UdpClient();
    var ep = new IPEndPoint(IPAddress.Parse("192.168.178.57"), 21324);
    const int MaxLedPerPacket = 489;
    const int SendLedsPerPacket = 870 / 2;
    const int leds = 870;
    byte[] sendBuf = new byte[2 + 2 + MaxLedPerPacket * 3];


    while (true)
    {
        var receiveResult = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
        if (receiveResult.CloseStatus.HasValue)
        {
            await webSocket.CloseAsync(
                receiveResult.CloseStatus.Value,
                receiveResult.CloseStatusDescription,
                CancellationToken.None);
            break;
        }

        try
        {
            var message = JsonSerializer.Deserialize<CoreMessage>(buffer.AsSpan(0, receiveResult.Count));

            Console.WriteLine($"Topic: {string.Join(":", message.Payload.ColRgb)}");

            var r = (byte)message.Payload.ColRgb[0];
            var g = (byte)message.Payload.ColRgb[1];
            var b = (byte)message.Payload.ColRgb[2];

            sendBuf[0] = 0x04; // DNRGB [Timout(s)] [Startindex XLow, XHigh] [R,G,B] * n
            sendBuf[1] = 0x10;

            sendBuf[2] = 0x00;
            sendBuf[3] = 0x00;

            for (int i = 0; i < SendLedsPerPacket; i++)
            {
                sendBuf[4 + i * 3 + 0] = r;
                sendBuf[4 + i * 3 + 1] = g;
                sendBuf[4 + i * 3 + 2] = b;
            }

            await udp.SendAsync(sendBuf.AsMemory(0, 4 + SendLedsPerPacket * 3), ep);

            sendBuf[2] = (SendLedsPerPacket >> 8) & 0xFF;
            sendBuf[3] = (SendLedsPerPacket >> 0) & 0xFF;

            await udp.SendAsync(sendBuf.AsMemory(0, 4 + SendLedsPerPacket * 3), ep);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Invalid JSON: {0}", ex.Message);
            continue;
        }


        //await webSocket.SendAsync(
        //new ArraySegment<byte>(buffer, 0, receiveResult.Count),
        //receiveResult.MessageType,
        //receiveResult.EndOfMessage,
        //CancellationToken.None);

        //receiveResult = await webSocket.ReceiveAsync(
        //    new ArraySegment<byte>(buffer), CancellationToken.None);
    }
}

struct CoreMessage
{
    [JsonPropertyName("topic")]
    public string Topic { get; set; }

    [JsonPropertyName("payload")]
    public CorePayload Payload { get; set; }
}

struct CorePayload
{
    [JsonPropertyName("brightness")]
    public int Brightness { get; set; }

    [JsonPropertyName("color")]
    public JsonElement Color { get; set; }

    [JsonPropertyName("_col_rgb")]
    public int[] ColRgb { get; set; }
}
