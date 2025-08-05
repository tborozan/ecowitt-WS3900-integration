-- Create weather database
CREATE DATABASE weatherdb;

-- Connect to weather database
\c weatherdb;

-- Create weather user
CREATE USER weather WITH PASSWORD 'weatherpass123';

-- Grant privileges
GRANT ALL PRIVILEGES ON DATABASE weatherdb TO weather;
GRANT ALL PRIVILEGES ON SCHEMA public TO weather;

-- Create weather_readings table (will be created by EF Core, but this ensures user has permissions)
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO weather;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO weather;