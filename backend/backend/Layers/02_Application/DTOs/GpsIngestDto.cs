using System;
using System.ComponentModel.DataAnnotations;

namespace FleetTelemetry.API.Layers._02_Application.DTOs
{
    public class GpsIngestDto
    {
        [Required(ErrorMessage = "El campo 'vehicle_id' es obligatorio.")]
        public string VehicleId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El campo 'lat' es obligatorio.")]
        [Range(-90.0, 90.0, ErrorMessage = "La latitud 'lat' debe estar en el rango de -90 a 90.")]
        public double? Lat { get; set; }

        [Required(ErrorMessage = "El campo 'lng' es obligatorio.")]
        [Range(-180.0, 180.0, ErrorMessage = "La longitud 'lng' debe estar en el rango de -180 a 180.")]
        public double? Lng { get; set; }

        [Required(ErrorMessage = "El campo 'timestamp' es obligatorio.")]
        public DateTime? Timestamp { get; set; }
    }
}