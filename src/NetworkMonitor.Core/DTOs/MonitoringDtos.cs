using NetworkMonitor.Core.Enums;

namespace NetworkMonitor.Core.DTOs;

// Карточка одного сервиса для Dashboard
public class ServiceCardDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ServiceType Type { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public int Port { get; set; }
    public ServiceStatus Status { get; set; }
    public double CurrentLatencyMs { get; set; }
    public double UptimePercentage { get; set; }
    public DateTime? LastChecked { get; set; }
}

// Верхняя плашка KPI + список сервисов
public class DashboardSummaryDto
{
    public int TotalServices { get; set; }
    public int OnlineCount { get; set; }
    public int WarningCount { get; set; }
    public int OfflineCount { get; set; }
    public double AvgNetworkLatencyMs { get; set; }
    public List<ServiceCardDto> Services { get; set; } = new();
}

// Запись для построения графика LiveCharts2
public class HistoryMetricDto
{
    public DateTime Timestamp { get; set; }
    public double ResponseTimeMs { get; set; }
    public ServiceStatus Status { get; set; }
}

// Строка таблицы инцидентов
public class AlertLogDto
{
    public int Id { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public DateTime Time { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

// Форма добавления нового узла
public class CreateServiceDto
{
    public string Name { get; set; } = string.Empty;
    public string HostUrl { get; set; } = string.Empty;
    public int Port { get; set; }
    public ServiceType Type { get; set; }
    public int CheckIntervalSec { get; set; } = 5;
    public int TimeoutMs { get; set; } = 2000;
    public int LatencyThresholdMs { get; set; } = 300;
}