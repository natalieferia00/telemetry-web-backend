using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using FleetTelemetry.API.Layers._02_Application.DTOs;
using FleetTelemetry.API.Layers._02_Application.Services;
using FleetTelemetry.API.Layers._04_Presentation.Hubs;
using FleetTelemetry.API.Layers._04_Presentation.HubsHandlers.Common;

namespace FleetTelemetry.API.Layers._04_Presentation.Controllers
{
    [ApiController]
    [Route("api")]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;
        private readonly IHubContext<ChatHub> _hubContext;
        
        public VehiclesController(IVehicleService vehicleService, IHubContext<ChatHub> hubContext)
        {
            _vehicleService = vehicleService;
            _hubContext = hubContext;
        }

        [HttpPost("gps")]
        public async Task<IActionResult> IngestGps([FromBody] GpsIngestDto telemetryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _vehicleService.ProcessTelemetry(telemetryDto);
            
            var responseData = new VehicleStatusDto
            {
                VehicleId = telemetryDto.VehicleId,
                LastLat = telemetryDto.Lat ?? 0,
                LastLng = telemetryDto.Lng ?? 0,
                LastSeen = telemetryDto.Timestamp ?? DateTime.UtcNow,
                Status = "Active"
            };
            
            await _hubContext.Clients.All.SendAsync("ReceiveAction", new ActionResponse("GPS_INGESTED", responseData));

            return StatusCode(201, new { message = "Coordenada almacenada" });
        }

        [HttpGet("vehicles")]
        public IActionResult GetVehicles()
        {
            var statuses = _vehicleService.GetCurrentFleetStatus();
            return Ok(statuses);
        }

        [HttpDelete("vehicles/{id}")]
        public async Task<IActionResult> DeleteVehicle(string id)
        {
            var isRemoved = _vehicleService.RemoveVehicle(id);
            
            if (!isRemoved)
            {
                return NotFound(new { message = "Vehículo no encontrado" });
            }
            
            await _hubContext.Clients.All.SendAsync("ReceiveAction", new ActionResponse("VEHICLE_DELETED", new { vehicleId = id }));

            return Ok(new { message = $"Vehículo {id} eliminado exitosamente." });
        }
    }
}