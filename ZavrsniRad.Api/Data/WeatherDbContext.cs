namespace ZavrsniRad.Api.Data;

using Microsoft.EntityFrameworkCore;
using Models;

public class WeatherDbContext : DbContext
{
    public WeatherDbContext(DbContextOptions<WeatherDbContext> options) : base(options) { }

    public DbSet<WeatherReading> WeatherReadings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WeatherReading>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Timestamp)
                    .HasColumnType("timestamp with time zone");

                // Create index on timestamp for better query performance
                entity.HasIndex(e => e.Timestamp);

                // Set decimal precision for weather data
                entity.Property(e => e.OutdoorTemperature).HasPrecision(5, 2);
                entity.Property(e => e.IndoorTemperature).HasPrecision(5, 2);
                entity.Property(e => e.OutdoorHumidity).HasPrecision(5, 2);
                entity.Property(e => e.IndoorHumidity).HasPrecision(5, 2);
                entity.Property(e => e.BarometricPressure).HasPrecision(7, 2);
                entity.Property(e => e.WindSpeed).HasPrecision(5, 2);
                entity.Property(e => e.WindGust).HasPrecision(5, 2);
                entity.Property(e => e.RainRate).HasPrecision(6, 2);
                entity.Property(e => e.SolarRadiation).HasPrecision(7, 2);
            }
        );
    }
}