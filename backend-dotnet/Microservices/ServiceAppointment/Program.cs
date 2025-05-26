using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using ServiceAppointment.API.Exceptions;
using ServiceAppointment.API.Interfaces;
using ServiceAppointment.API.Services;
using ServiceAppointment.Data;
using ServiceAppointment.Interfaces;
using ServiceAppointment.Services;
using Serilog;
using StackExchange.Redis;
using System.Text;
using HealthChecks.NpgSql;

// ✅ Serilog Logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .MinimumLevel.Information()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// ✅ Use Serilog
builder.Host.UseSerilog();

// ✅ Add Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ EF Core – PostgreSQL
builder.Services.AddDbContext<AppointmentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// ✅ Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("redis:6379"));
builder.Services.AddScoped<ICacheService, RedisCacheService>();

// ✅ Application Services
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IRabbitMqPublisher, RabbitMqPublisher>();

// ✅ CORS – Allow All
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ✅ JWT Authentication (HS256)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Domain"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// ✅ Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "ready" })
    .AddNpgSql(
        builder.Configuration.GetConnectionString("Default"),
        name: "postgresql",
        timeout: TimeSpan.FromSeconds(5),
        tags: new[] { "db", "sql", "postgres" });

var app = builder.Build();

// ✅ Global Exception Middleware
app.UseMiddleware<ExceptionMiddleware>();

// ✅ Swagger
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ Request Pipeline
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// ✅ Apply Migrations on Startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppointmentDbContext>();
    dbContext.Database.Migrate();
}

app.Run();
