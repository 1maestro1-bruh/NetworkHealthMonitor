using System.Diagnostics;
using System.Net.Sockets;

namespace NetworkMonitor.Server.Probers;

public interface INetworkProber
{
    Task<ProbeResult> ProbeHttpAsync(string url, int timeoutMs, CancellationToken ct);
    Task<ProbeResult> ProbeTcpAsync(string host, int port, int timeoutMs, CancellationToken ct);
}

public class NetworkProber : INetworkProber
{
    private readonly IHttpClientFactory _httpClientFactory;

    public NetworkProber(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ProbeResult> ProbeHttpAsync(string url, int timeoutMs, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromMilliseconds(timeoutMs);

        var sw = Stopwatch.StartNew();
        try
        {
            using var response = await client.GetAsync(url, ct);
            sw.Stop();

            bool isHealthy = (int)response.StatusCode >= 200 && (int)response.StatusCode < 400;
            return new ProbeResult(
                IsSuccess: isHealthy,
                LatencyMs: sw.Elapsed.TotalMilliseconds,
                StatusCode: (int)response.StatusCode,
                Error: isHealthy ? null : $"HTTP {(int)response.StatusCode}"
            );
        }
        catch (TaskCanceledException)
        {
            sw.Stop();
            return new ProbeResult(false, sw.Elapsed.TotalMilliseconds, null, "Превышен таймаут (Timeout)");
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new ProbeResult(false, sw.Elapsed.TotalMilliseconds, null, ex.Message);
        }
    }

    public async Task<ProbeResult> ProbeTcpAsync(string host, int port, int timeoutMs, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        using var client = new TcpClient();
        try
        {
            using var timeoutCts = new CancellationTokenSource(timeoutMs);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

            await client.ConnectAsync(host, port, linkedCts.Token);
            sw.Stop();
            return new ProbeResult(true, sw.Elapsed.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new ProbeResult(false, sw.Elapsed.TotalMilliseconds, null, ex.Message);
        }
    }
}