using System;

namespace FleetTelemetry.API.Layers._02_Application.DTOs
{
    public class VehicleStatusDto
    {
        public string VehicleId { get; set; } = string.Empty;
        public double LastLat { get; set; }
        public double LastLng { get; set; }
        public DateTime LastSeen { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}