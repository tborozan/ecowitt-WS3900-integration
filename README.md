# Ecowitt WS3900 Weather Station Integration

This project provides a complete solution for collecting data from an Ecowitt WS3900 weather station via webhook, storing it in PostgreSQL, and visualizing it with Grafana dashboards.

## Features

- **C# ASP.NET Core Web API** - Receives webhook data from Ecowitt WS3900
- **PostgreSQL Database** - Stores weather data with proper indexing
- **Grafana Dashboard** - Beautiful visualizations of weather data
- **Docker Compose** - Easy deployment and management
- **Unit Conversion** - Converts imperial units to metric (Celsius, hPa, m/s, mm)

## Quick Start

### Prerequisites

- Docker and Docker Compose installed
- Ecowitt WS3900 weather station with internet connectivity

### 1. Clone and Start Services

```bash
# Start all services
docker-compose up -d

# Check service status
docker-compose ps
```

### 2. Configure Ecowitt WS3900

1. Access your weather station's web interface
2. Navigate to **Services** → **Customized**
3. Configure webhook settings:
    - **Server IP/Hostname**: Your server's IP address
    - **Path**: `/api/webhook`
    - **Port**: `8080`
    - **Upload Interval**: `60` seconds (recommended)

Full webhook URL: `http://YOUR_SERVER_IP:8080/api/webhook`

### 3. Access Grafana Dashboard

1. Open browser to `http://localhost:3000`
2. Login with:
    - Username: `admin`
    - Password: `admin`
3. The weather dashboard will be automatically provisioned

## API Endpoints

- `POST /api/webhook` - Receives Ecowitt webhook data
- `GET /api/status` - API health status and statistics
- `GET /api/latest` - Latest weather reading

## Database Schema

The `WeatherReadings` table stores:

- Temperature data in Celsius
- Battery status for sensors
- Timestamp in UTC

## Data Conversion

Imperial to metric conversions:
- Temperature: °F → °C using (°F - 32) × 5/9

## Grafana Dashboard

The dashboard includes:
- Current temperature
- Temperature trends

## Service Management

```bash
# Start services
docker-compose up -d

# View logs
docker-compose logs -f weather-api
docker-compose logs -f postgres
docker-compose logs -f grafana

# Stop services
docker-compose down

# Stop and remove data volumes
docker-compose down -v
```

## Troubleshooting

### API not receiving data
1. Check webhook URL configuration in weather station
2. Verify network connectivity: `curl -X POST http://localhost:8080/api/status`
3. Check API logs: `docker-compose logs weather-api`

### Database connection issues
1. Check PostgreSQL status: `docker-compose ps postgres`
2. Verify database initialization: `docker-compose logs postgres`

### Grafana dashboard not showing data
1. Verify PostgreSQL datasource connection in Grafana
2. Check if data exists: Connect to database and query `WeatherReadings` table
3. Ensure time range in dashboard covers available data

## Development

### Project Structure

```
├── ZavrsniRad.Api/                 # C# Web API
│   ├── Data/                  # Entity Framework DbContext
│   ├── Models/                # Data models
│   ├── ZavrsniRad.Api.csproj  # Project file
│   ├── Program.cs             # Application entry point
│   ├── appsettings.json       # Configuration
│   └── Dockerfile             # API container
├── grafana/                   # Grafana configuration
│   ├── provisioning/          # Auto-provisioning
│   └── dashboards/            # Dashboard definitions
├── postgres/                  # PostgreSQL setup
│   └── init/                  # Database initialization
└── docker-compose.yml         # Service orchestration
```

### Running Locally

```bash
docker-compose up -d
```

## License

MIT License - see LICENSE file for details.
