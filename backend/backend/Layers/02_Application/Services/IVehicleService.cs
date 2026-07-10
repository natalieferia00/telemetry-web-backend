using System.Collections.Generic;
using FleetTelemetry.API.Layers._02_Application.DTOs;

namespace FleetTelemetry.API.Layers._02_Application.Services
{
    public interface IVehicleService
    {
        void ProcessTelemetry(GpsIngestDto telemetryDto);
        IEnumerable<VehicleStatusDto> GetCurrentFleetStatus();
        bool RemoveVehicle(string vehicleId);
    }
}