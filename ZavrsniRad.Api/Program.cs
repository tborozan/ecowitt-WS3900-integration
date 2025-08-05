using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ZavrsniRad.Api.Data;
using ZavrsniRad.Api.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add PostgreSQL database
string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<WeatherDbContext>(options =>
    options.UseNpgsql(connectionString)
);

WebApplication app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Ensure database is created
using (IServiceScope scope = app.Services.CreateScope())
{
    WeatherDbContext context = scope.ServiceProvider.GetRequiredService<WeatherDbContext>();
    try
    {
        await context.Database.EnsureCreatedAsync();
        Log.Information("Database initialized successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error initializing database");
    }
}

// Minimal API Endpoints

// Webhook endpoint for receiving Ecowitt data
app.MapPost(
        "/api/webhook",
        async (HttpContext context, WeatherDbContext db, ILogger<Program> logger) =>
        {
            try
            {
                IFormCollection form = await context.Request.ReadFormAsync();

                logger.LogInformation("Received webhook data with {Count} fields", form.Count);

                // Parse the Ecowitt data
                WeatherReading weatherReading = ParseEcowittData(form);

                // Save to the database
                db.WeatherReadings.Add(weatherReading);
                await db.SaveChangesAsync();

                logger.LogInformation("Saved weather reading for {Timestamp}", weatherReading.Timestamp);

                return Results.Ok(new { status = "success", timestamp = weatherReading.Timestamp });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing webhook data");
                return Results.Problem(detail: ex.Message, statusCode: 500);
            }
        }
    )
    .WithName("ReceiveWebhook")
    .WithOpenApi();

// Status endpoint
app.MapGet(
        "/api/status",
        async (WeatherDbContext db) =>
        {
            try
            {
                WeatherReading? lastReading = await db.WeatherReadings
                    .OrderByDescending(w => w.Timestamp)
                    .FirstOrDefaultAsync();

                int totalReadings = await db.WeatherReadings.CountAsync();

                return Results.Ok(
                    new
                    {
                        status = "healthy",
                        totalReadings,
                        lastReading = lastReading?.Timestamp,
                        uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()
                    }
                );
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 500);
            }
        }
    )
    .WithName("GetStatus")
    .WithOpenApi();

// Latest reading endpoint
app.MapGet(
        "/api/latest",
        async (WeatherDbContext db) =>
        {
            try
            {
                WeatherReading? latest = await db.WeatherReadings
                    .OrderByDescending(w => w.Timestamp)
                    .FirstOrDefaultAsync();

                if (latest == null)
                    return Results.NotFound(new { message = "No weather readings found" });

                return Results.Ok(latest);
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 500);
            }
        }
    )
    .WithName("GetLatestReading")
    .WithOpenApi();

Log.Information("Weather API starting...");

app.Run();
return;

// Helper methods
static WeatherReading ParseEcowittData(IFormCollection form)
{
    WeatherReading reading = new()
    {
        Timestamp = ParseDateTime(form["dateutc"]) ?? DateTime.UtcNow,
        StationType = form["stationtype"].ToString(),
        Model = form["model"].ToString(),
        Frequency = form["freq"].ToString(),
        // Convert temperatures from Fahrenheit to Celsius
        OutdoorTemperature = FahrenheitToCelsius(ParseDouble(form["tempf"])),
        IndoorTemperature = FahrenheitToCelsius(ParseDouble(form["tempinf"])),
        Sensor1Temperature = FahrenheitToCelsius(ParseDouble(form["temp1f"])),
        Sensor2Temperature = FahrenheitToCelsius(ParseDouble(form["temp2f"])),
        // Humidity (already in percentage)
        OutdoorHumidity = ParseDouble(form["humidity"]) ?? 0,
        IndoorHumidity = ParseDouble(form["humidityin"]) ?? 0,
        Sensor1Humidity = ParseDouble(form["humidity1"]),
        Sensor2Humidity = ParseDouble(form["humidity2"]),
        // Convert pressure from inHg to hPa
        BarometricPressure = InHgToHPa(ParseDouble(form["baromrelin"])),
        AbsolutePressure = InHgToHPa(ParseDouble(form["baromabsin"])),
        // Wind data - convert mph to m/s
        WindSpeed = MphToMs(ParseDouble(form["windspeedmph"])) ?? 0,
        WindGust = MphToMs(ParseDouble(form["windgustmph"])) ?? 0,
        MaxDailyGust = MphToMs(ParseDouble(form["maxdailygust"])) ?? 0,
        WindDirection = ParseDouble(form["winddir"]) ?? 0,
        // Rain data - convert inches to mm
        RainRate = InchesToMm(ParseDouble(form["rainratein"])) ?? 0,
        EventRain = InchesToMm(ParseDouble(form["eventrainin"])) ?? 0,
        HourlyRain = InchesToMm(ParseDouble(form["hourlyrainin"])) ?? 0,
        DailyRain = InchesToMm(ParseDouble(form["dailyrainin"])) ?? 0,
        WeeklyRain = InchesToMm(ParseDouble(form["weeklyrainin"])) ?? 0,
        MonthlyRain = InchesToMm(ParseDouble(form["monthlyrainin"])) ?? 0,
        YearlyRain = InchesToMm(ParseDouble(form["yearlyrainin"])) ?? 0,
        TotalRain = InchesToMm(ParseDouble(form["totalrainin"])) ?? 0,
        // Solar data
        SolarRadiation = ParseDouble(form["solarradiation"]) ?? 0,
        UvIndex = ParseDouble(form["uv"]) ?? 0,
        // Battery status
        Wh65Battery = ParseInt(form["wh65batt"]) ?? 0,
        Wh25Battery = ParseInt(form["wh25batt"]) ?? 0,
        Battery1 = ParseInt(form["batt1"]) ?? 0,
        Battery2 = ParseInt(form["batt2"]) ?? 0
    };

    return reading;
}

// Unit conversion methods
static double FahrenheitToCelsius(double? fahrenheit)
{
    return fahrenheit.HasValue ? (fahrenheit.Value - 32) * 5 / 9 : 0;
}

static double InHgToHPa(double? inHg)
{
    return inHg.HasValue ? inHg.Value * 33.8639 : 0;
}

static double? MphToMs(double? mph)
{
    return mph * 0.44704;
}

static double? InchesToMm(double? inches)
{
    return inches * 25.4;
}

// Parsing helper methods
static double? ParseDouble(string? value)
{
    return double.TryParse(value, out double result) ? result : null;
}

static int? ParseInt(string? value)
{
    return int.TryParse(value, out int result) ? result : null;
}

static DateTime? ParseDateTime(string? value)
{
    if (string.IsNullOrEmpty(value))
        return null;

    // Ecowitt sends date as "YYYY-MM-DD HH:mm:ss" in UTC
    string cleanValue = value.Replace("+", " ");
    return DateTime.TryParse(cleanValue, out DateTime result) ? DateTime.SpecifyKind(result, DateTimeKind.Utc) : null;
}