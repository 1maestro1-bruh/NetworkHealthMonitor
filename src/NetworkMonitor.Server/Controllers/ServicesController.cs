using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetworkMonitor.Core.DTOs;
using NetworkMonitor.Core.Enums;
using NetworkMonitor.Data;
using NetworkMonitor.Data.Entities;

namespace NetworkMonitor.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ServicesController(AppDbContext db) => _db = db;

    // Главный эндпоинт для клиента: KPI + Карточки
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardSummaryDto>> GetDashboard()
    {
        var services = await _db.Services.Where(s => s.IsActive).ToListAsync();
        var cardDtos = new List<ServiceCardDto>();

        foreach (var s in services)
        {
            var lastCheck = await _db.CheckHistories
                .Where(h => h.ServiceId == s.Id)
                .OrderByDescending(h => h.Timestamp)
                .FirstOrDefaultAsync();

            var totalChecks = await _db.CheckHistories.CountAsync(h => h.ServiceId == s.Id);
            var successChecks = await _db.CheckHistories
                .CountAsync(h => h.ServiceId == s.Id && h.Status != ServiceStatus.Offline);

            double uptime = totalChecks > 0 ? (double)successChecks / totalChecks * 100 : 100.0;

            cardDtos.Add(new ServiceCardDto
            {
                Id = s.Id,
                Name = s.Name,
                Type = s.Type,
                Endpoint = s.HostUrl,
                Port = s.Port,
                Status = lastCheck?.Status ?? ServiceStatus.Offline,
                CurrentLatencyMs = lastCheck?.ResponseTimeMs ?? 0,
                UptimePercentage = Math.Round(uptime, 1),
                LastChecked = lastCheck?.Timestamp
            });
        }

        var result = new DashboardSummaryDto
        {
            TotalServices = cardDtos.Count,
            OnlineCount = cardDtos.Count(c => c.Status == ServiceStatus.Online),
            WarningCount = cardDtos.Count(c => c.Status == ServiceStatus.Warning),
            OfflineCount = cardDtos.Count(c => c.Status == ServiceStatus.Offline),
            AvgNetworkLatencyMs = cardDtos.Count > 0 ? Math.Round(cardDtos.Average(c => c.CurrentLatencyMs), 1) : 0,
            Services = cardDtos
        };

        return Ok(result);
    }

    // История для построения графиков LiveCharts2
    [HttpGet("{id}/history")]
    public async Task<ActionResult<List<HistoryMetricDto>>> GetHistory(int id, [FromQuery] int minutes = 60)
    {
        var fromDate = DateTime.UtcNow.AddMinutes(-minutes);
        var data = await _db.CheckHistories
            .Where(h => h.ServiceId == id && h.Timestamp >= fromDate)
            .OrderBy(h => h.Timestamp)
            .Select(h => new HistoryMetricDto
            {
                Timestamp = h.Timestamp,
                ResponseTimeMs = h.ResponseTimeMs,
                Status = h.Status
            })
            .ToListAsync();

        return Ok(data);
    }

    // Список последних инцидентов для таблицы
    [HttpGet("alerts")]
    public async Task<ActionResult<List<AlertLogDto>>> GetAlerts([FromQuery] int count = 50)
    {
        var alerts = await _db.Alerts
            .Include(a => a.Service)
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .Select(a => new AlertLogDto
            {
                Id = a.Id,
                ServiceName = a.Service.Name,
                Time = a.Timestamp,
                Type = a.Type.ToString(),
                Message = a.Message
            })
            .ToListAsync();

        return Ok(alerts);
    }

    // Добавление нового сервиса через форму настроек
    [HttpPost]
    public async Task<IActionResult> CreateService([FromBody] CreateServiceDto dto)
    {
        var service = new MonitoredService
        {
            Name = dto.Name,
            HostUrl = dto.HostUrl,
            Port = dto.Port,
            Type = dto.Type,
            CheckIntervalSec = dto.CheckIntervalSec,
            TimeoutMs = dto.TimeoutMs,
            LatencyThresholdMs = dto.LatencyThresholdMs
        };

        _db.Services.Add(service);
        await _db.SaveChangesAsync();

        return Ok(service);
    }
}