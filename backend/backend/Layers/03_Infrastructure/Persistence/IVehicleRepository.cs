using System.Collections.Concurrent;
using System.Collections.Generic;
using FleetTelemetry.API.Layers._01_Domain.Entities;

namespace FleetTelemetry.API.Layers._03_Infrastructure.Persistence
{
    public interface IVehicleRepository
    {
        void SaveCoordinate(Vehicle vehicle);
        ConcurrentDictionary<string, List<Vehicle>> GetAllHistory();
        bool Delete(string vehicleId);
    }
}