namespace NetworkMonitor.Server.Probers;

public record ProbeResult(
    bool IsSuccess,
    double LatencyMs,
    int? StatusCode = null,
    string? Error = null);