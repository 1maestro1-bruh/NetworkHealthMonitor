using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetworkMonitor.Core.Enums;

namespace NetworkMonitor.Data.Entities;

[Table("Alerts")]
public class Alert
{
    [Key]
    public int Id { get; set; }

    public int ServiceId { get; set; }
    [ForeignKey(nameof(ServiceId))]
    public MonitoredService Service { get; set; } = null!;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public AlertType Type { get; set; }

    [MaxLength(255)]
    public string Message { get; set; } = string.Empty;

    public bool IsResolved { get; set; } = false;
}