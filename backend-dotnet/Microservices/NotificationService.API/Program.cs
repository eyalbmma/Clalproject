using NotificationService.API.Interfaces;
using NotificationService.API.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using NotificationService.API.Exceptions;  // הוספת ה־using עבור ExceptionMiddleware

// הגדרת Serilog כ־Logger
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .MinimumLevel.Information()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// ✅ שימוש ב־Serilog כ־Logging Provider
builder.Host.UseSerilog();

// הוספת שירותים
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<INotificationSenderService, NotificationSenderService>();
builder.Services.AddHostedService<RabbitMqConsumer>();

// ✅ CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ✅ Health Checks – רק בדיקת Self
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "ready" });

var app = builder.Build();

// ✅ הוספת Middleware לטיפול בשגיאות
app.UseMiddleware<ExceptionMiddleware>();

// Middleware
app.UseCors("AllowAll");

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

// ✅ Map /health
app.MapHealthChecks("/health");

app.Run();
