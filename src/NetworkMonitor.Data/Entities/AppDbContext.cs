using Microsoft.EntityFrameworkCore;
using NetworkMonitor.Core.Enums;
using NetworkMonitor.Data.Entities;

namespace NetworkMonitor.Data;

public class AppDbContext : DbContext
{
    public DbSet<MonitoredService> Services => Set<MonitoredService>();
    public DbSet<CheckHistory> CheckHistories => Set<CheckHistory>();
    public DbSet<Alert> Alerts => Set<Alert>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Индекс для быстрых выборок аналитики за период
        modelBuilder.Entity<CheckHistory>()
            .HasIndex(h => new { h.ServiceId, h.Timestamp });

        // Автоматическое заполнение базы 4 тестовыми сервисами при первом запуске
        modelBuilder.Entity<MonitoredService>().HasData(
            new MonitoredService
            {
                Id = 1,
                Name = "Fast HTTP Service",
                HostUrl = "http://localhost:5001/fast",
                Port = 5001,
                Type = ServiceType.Http,
                CheckIntervalSec = 3,
                LatencyThresholdMs = 150,
                TimeoutMs = 1500
            },
            new MonitoredService
            {
                Id = 2,
                Name = "Slow Degraded Service",
                HostUrl = "http://localhost:5001/slow",
                Port = 5001,
                Type = ServiceType.Http,
                CheckIntervalSec = 4,
                LatencyThresholdMs = 200, // порог 200мс, а отдавать будет за 400мс -> статус WARNING
                TimeoutMs = 2000
            },
            new MonitoredService
            {
                Id = 3,
                Name = "Flaky Failing Service",
                HostUrl = "http://localhost:5001/flaky",
                Port = 5001,
                Type = ServiceType.Http,
                CheckIntervalSec = 5,
                LatencyThresholdMs = 200,
                TimeoutMs = 1500
            },
            new MonitoredService
            {
                Id = 4,
                Name = "TCP Socket Service",
                HostUrl = "127.0.0.1",
                Port = 9000,
                Type = ServiceType.Tcp,
                CheckIntervalSec = 3,
                LatencyThresholdMs = 100,
                TimeoutMs = 1500
            }
        );
    }
}