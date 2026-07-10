using System.Collections.Concurrent;
using System.Collections.Generic;
using FleetTelemetry.API.Layers._01_Domain.Entities;

namespace FleetTelemetry.API.Layers._03_Infrastructure.Persistence
{
    public class InMemoryVehicleRepository : IVehicleRepository
    {
        private readonly ConcurrentDictionary<string, List<Vehicle>> _dataStore = new();

        public void SaveCoordinate(Vehicle vehicle)
        {
            _dataStore.AddOrUpdate(vehicle.Id,
                new List<Vehicle> { vehicle },
                (id, list) =>
                {
                    lock (list)
                    {
                        list.Add(vehicle);
                        if (list.Count > 30)
                        {
                            list.RemoveAt(0);
                        }
                    }
                    return list;
                });
        }

        public ConcurrentDictionary<string, List<Vehicle>> GetAllHistory()
        {
            return _dataStore;
        }

        public bool Delete(string vehicleId)
        {
            return _dataStore.TryRemove(vehicleId, out _);
        }
    }
}