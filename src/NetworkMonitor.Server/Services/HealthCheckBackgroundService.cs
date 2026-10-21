using Microsoft.EntityFrameworkCore;
using NetworkMonitor.Core.Enums;
using NetworkMonitor.Data;
using NetworkMonitor.Data.Entities;
using NetworkMonitor.Server.Probers;

namespace NetworkMonitor.Server.Services;

public class HealthCheckBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INetworkProber _prober;
    private readonly ILogger<HealthCheckBackgroundService> _logger;

    public HealthCheckBackgroundService(
        IServiceProvider serviceProvider,
        INetworkProber prober,
        ILogger<HealthCheckBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _prober = prober;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🟢 Движок фонового мониторинга запущен.");
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(3));

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var services = await db.Services.Where(s => s.IsActive).ToListAsync(stoppingToken);

                foreach (var service in services)
                {
                    await CheckAndLogServiceAsync(service, db, stoppingToken);
                }

                await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка цикла сетевого мониторинга");
            }
        }
    }

    private async Task CheckAndLogServiceAsync(MonitoredService service, AppDbContext db, CancellationToken ct)
    {
        ProbeResult result = service.Type switch
        {
            ServiceType.Http => await _prober.ProbeHttpAsync(service.HostUrl, service.TimeoutMs, ct),
            ServiceType.Tcp => await _prober.ProbeTcpAsync(service.HostUrl, service.Port, service.TimeoutMs, ct),
            _ => new ProbeResult(false, 0, null, "Неизвестный протокол")
        };

        ServiceStatus status;
        if (!result.IsSuccess)
            status = ServiceStatus.Offline;
        else if (result.LatencyMs > service.LatencyThresholdMs)
            status = ServiceStatus.Warning;
        else
            status = ServiceStatus.Online;

        // 1. Запись истории метрики
        db.CheckHistories.Add(new CheckHistory
        {
            ServiceId = service.Id,
            Timestamp = DateTime.UtcNow,
            Status = status,
            ResponseTimeMs = Math.Round(result.LatencyMs, 2),
            HttpStatusCode = result.StatusCode,
            ErrorMessage = result.Error
        });

        // 2. Генерация алертов при инцидентах
        if (status == ServiceStatus.Offline)
        {
            db.Alerts.Add(new Alert
            {
                ServiceId = service.Id,
                Timestamp = DateTime.UtcNow,
                Type = AlertType.ServiceOffline,
                Message = $"Узел недоступен: {result.Error ?? "Сбой соединения"}"
            });
        }
        else if (status == ServiceStatus.Warning)
        {
            db.Alerts.Add(new Alert
            {
                ServiceId = service.Id,
                Timestamp = DateTime.UtcNow,
                Type = AlertType.HighLatency,
                Message = $"Задержка {result.LatencyMs:F0} мс превысила порог {service.LatencyThresholdMs} мс"
            });
        }
    }
}