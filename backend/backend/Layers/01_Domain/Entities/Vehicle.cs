using System;
using FleetTelemetry.API.Layers._01_Domain.Enums;

namespace FleetTelemetry.API.Layers._01_Domain.Entities
{
    public class Vehicle
    {
        public string Id { get; private set; }
        public double Lat { get; private set; }
        public double Lng { get; private set; }
        public DateTime Timestamp { get; private set; }

        public Vehicle(string id, double lat, double lng, DateTime timestamp)
        {
            if (string.IsNullOrWhiteSpace(id)) 
                throw new ArgumentException("El identificador del vehículo no puede estar vacío.");

            Id = id;
            UpdateLocation(lat, lng, timestamp);
        }

        public void UpdateLocation(double lat, double lng, DateTime timestamp)
        {
            Lat = lat;
            Lng = lng;
            Timestamp = timestamp;
        }
    }
}