using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetworkMonitor.Core.Enums;

namespace NetworkMonitor.Data.Entities;

[Table("Services")]
public class MonitoredService
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string HostUrl { get; set; } = string.Empty;

    public int Port { get; set; }

    public ServiceType Type { get; set; }

    public int CheckIntervalSec { get; set; } = 5;

    public int TimeoutMs { get; set; } = 2000;

    public int LatencyThresholdMs { get; set; } = 300;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CheckHistory> CheckHistories { get; set; } = new List<CheckHistory>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}