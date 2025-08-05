namespace ZavrsniRad.Api.Models;

using System.ComponentModel.DataAnnotations;

public class WeatherReading
{
    [Key]
    public int Id { get; set; }

    public DateTime Timestamp { get; set; }

    // Temperature (in Celsius)
    public double OutdoorTemperature { get; set; }

    public double IndoorTemperature { get; set; }

    public double? Sensor1Temperature { get; set; }

    public double? Sensor2Temperature { get; set; }

    // Humidity (percentage)
    public double OutdoorHumidity { get; set; }

    public double IndoorHumidity { get; set; }

    public double? Sensor1Humidity { get; set; }

    public double? Sensor2Humidity { get; set; }

    // Pressure (hPa)
    public double BarometricPressure { get; set; }

    public double AbsolutePressure { get; set; }

    // Wind (speed in m/s, direction in degrees)
    public double WindSpeed { get; set; }

    public double WindGust { get; set; }

    public double WindDirection { get; set; }

    public double MaxDailyGust { get; set; }

    // Rain (in mm)
    public double RainRate { get; set; }

    public double EventRain { get; set; }

    public double HourlyRain { get; set; }

    public double DailyRain { get; set; }

    public double WeeklyRain { get; set; }

    public double MonthlyRain { get; set; }

    public double YearlyRain { get; set; }

    public double TotalRain { get; set; }

    // Solar
    public double SolarRadiation { get; set; }

    public double UvIndex { get; set; }

    // Battery status
    public int Wh65Battery { get; set; }

    public int Wh25Battery { get; set; }

    public int Battery1 { get; set; }

    public int Battery2 { get; set; }

    // Station info
    public string StationType { get; set; } = string.Empty;

    public string Frequency { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;
}