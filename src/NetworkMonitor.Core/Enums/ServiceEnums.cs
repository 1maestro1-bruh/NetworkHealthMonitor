namespace NetworkMonitor.Core.Enums;

public enum ServiceType
{
    Http = 0,
    Tcp = 1
}

public enum ServiceStatus
{
    Online = 0,
    Warning = 1,
    Offline = 2
}

public enum AlertType
{
    ServiceOffline = 0,
    HighLatency = 1,
    HttpError = 2
}