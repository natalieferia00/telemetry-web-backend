using System;
using System.Text.Json;
using System.Threading.Tasks;
using FleetTelemetry.API.Layers._02_Application.DTOs;
using FleetTelemetry.API.Layers._02_Application.Services;
using FleetTelemetry.API.Layers._04_Presentation.HubsHandlers.Common;
using Microsoft.AspNetCore.SignalR;

namespace FleetTelemetry.API.Layers._04_Presentation.HubsHandlers.VehicleStrategies
{
    public class IngestGpsStrategy : IChatActionStrategy
    {
        public string ActionType => "INGEST_GPS";
        private readonly IVehicleService _vehicleService;

        public IngestGpsStrategy(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        public async Task ExecuteAsync(Hub hub, JsonElement payload)
        {
           
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var telemetryDto = JsonSerializer.Deserialize<GpsIngestDto>(payload.GetRawText(), options);

            if (telemetryDto == null || string.IsNullOrEmpty(telemetryDto.VehicleId) || !telemetryDto.Lat.HasValue || !telemetryDto.Lng.HasValue)
            {
                await hub.Clients.Caller.SendAsync("ReceiveAction", new ActionResponse("ERROR", new { message = "Datos de telemetría inválidos." }));
                return;
            }
            
            _vehicleService.ProcessTelemetry(telemetryDto);
            
            var responseData = new VehicleStatusDto
            {
                VehicleId = telemetryDto.VehicleId,
                LastLat = telemetryDto.Lat.Value,
                LastLng = telemetryDto.Lng.Value,
                LastSeen = telemetryDto.Timestamp ?? DateTime.UtcNow,
                Status = "Active"
            };

            await hub.Clients.All.SendAsync("ReceiveAction", new ActionResponse("GPS_INGESTED", responseData));
            
            await hub.Clients.Caller.SendAsync("ReceiveAction", new ActionResponse("SUCCESS", new { message = "Coordenada procesada via WebSocket" }));
        }
    }
}