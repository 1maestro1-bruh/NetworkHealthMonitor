using System.Net;
using System.Net.Sockets;

Console.Title = "Target Mock Services (Порты: HTTP 5001, TCP 9000)";
Console.WriteLine("Запуск целевых тестовых сервисов...");

// 1. Запуск TCP Socket сервера (порт 9000)
_ = Task.Run(async () =>
{
    var listener = new TcpListener(IPAddress.Any, 9000);
    listener.Start();
    Console.WriteLine("🟢 [TCP Server] Слушает на порту 9000");

    while (true)
    {
        var client = await listener.AcceptTcpClientAsync();
        _ = Task.Run(() =>
        {
            using (client)
            using (var stream = client.GetStream())
            {
                Thread.Sleep(15); // Имитируем быстрый сетевой handshake
            }
        });
    }
});

// 2. Запуск HTTP-заглушек на порту 5001
var builder = WebApplication.CreateBuilder();
var app = builder.Build();

int flakyHits = 0;

// Узел 1: Быстрый (Online)
app.MapGet("/fast", () => Results.Ok(new { status = "OK", node = "FastNode" }));

// Узел 2: Медленный (задержка 400мс -> превысит порог 200мс -> статус Warning)
app.MapGet("/slow", async () =>
{
    await Task.Delay(400);
    return Results.Ok(new { status = "Degraded", node = "SlowNode" });
});

// Узел 3: Падающий (каждый 2-й запрос дает HTTP 500 -> статус Offline)
app.MapGet("/flaky", () =>
{
    flakyHits++;
    if (flakyHits % 2 == 0)
    {
        return Results.Problem("Сервис временно недоступен", statusCode: 500);
    }
    return Results.Ok(new { status = "OK", node = "FlakyNode" });
});

Console.WriteLine("🟢 [HTTP Server] Эндпоинты запущены на http://localhost:5001 (/fast, /slow, /flaky)");
app.Run("http://localhost:5001");