using Microsoft.EntityFrameworkCore;
using NetworkMonitor.Data;
using NetworkMonitor.Server.Probers;
using NetworkMonitor.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Подключение контроллеров и Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. Регистрация SQLite базы данных (файл monitoring.db)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=monitoring.db"));

// 3. Регистрация сетевого пробера и HTTP-клиента
builder.Services.AddHttpClient();
builder.Services.AddSingleton<INetworkProber, NetworkProber>();

// 4. Регистрация фонового сервиса мониторинга
builder.Services.AddHostedService<HealthCheckBackgroundService>();

var app = builder.Build();

// Автоматическое создание БД и сидирование тестовых данных при старте
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

// Запускаем API на порту 5000
app.Run("http://localhost:5000");