using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetworkMonitor.Core.Enums;

namespace NetworkMonitor.Data.Entities;

[Table("CheckHistory")]
public class CheckHistory
{
    [Key]
    public long Id { get; set; }

    public int ServiceId { get; set; }
    [ForeignKey(nameof(ServiceId))]
    public MonitoredService Service { get; set; } = null!;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public ServiceStatus Status { get; set; }

    public double ResponseTimeMs { get; set; }

    public int? HttpStatusCode { get; set; }

    [MaxLength(500)]
    public string? ErrorMessage { get; set; }
}