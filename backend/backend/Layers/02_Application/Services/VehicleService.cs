using System;
using System.Collections.Generic;
using System.Linq;
using FleetTelemetry.API.Layers._01_Domain.Entities;
using FleetTelemetry.API.Layers._01_Domain.Enums;
using FleetTelemetry.API.Layers._02_Application.DTOs;
using FleetTelemetry.API.Layers._03_Infrastructure.Persistence;

namespace FleetTelemetry.API.Layers._02_Application.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IVehicleRepository _repository;

        public VehicleService(IVehicleRepository repository)
        {
            _repository = repository;
        }

        public void ProcessTelemetry(GpsIngestDto telemetryDto)
        {
            var vehicle = new Vehicle(
                telemetryDto.VehicleId, 
                telemetryDto.Lat!.Value, 
                telemetryDto.Lng!.Value, 
                telemetryDto.Timestamp!.Value
            );

            _repository.SaveCoordinate(vehicle);
        }

        public IEnumerable<VehicleStatusDto> GetCurrentFleetStatus()
        {
            var now = DateTime.UtcNow;
            var fleetHistory = _repository.GetAllHistory();
            var statusList = new List<VehicleStatusDto>();

            foreach (var kvp in fleetHistory)
            {
                var vehicleId = kvp.Key;
                var historicalCoords = kvp.Value.OrderByDescending(c => c.Timestamp).ToList();
                
                if (!historicalCoords.Any()) continue;

                var currentInfo = historicalCoords.First();
                var secondsSinceLastSeen = (now - currentInfo.Timestamp).TotalSeconds;
                
                VehicleStatus computedStatus = VehicleStatus.Stopped;

                if (secondsSinceLastSeen > 120)
                {
                    computedStatus = VehicleStatus.NoSignal;
                }
                else
                {
                    var lastMinuteReads = historicalCoords
                        .Where(c => (now - c.Timestamp).TotalSeconds <= 60)
                        .ToList();

                    if (lastMinuteReads.Count > 1)
                    {
                        var oldestInWindow = lastMinuteReads.Last();
                        
                        bool hasMoved = Math.Abs(currentInfo.Lat - oldestInWindow.Lat) > 0.00001 || 
                                        Math.Abs(currentInfo.Lng - oldestInWindow.Lng) > 0.00001;

                        if (hasMoved)
                        {
                            computedStatus = VehicleStatus.Moving;
                        }
                    }
                }

                statusList.Add(new VehicleStatusDto
                {
                    VehicleId = vehicleId,
                    LastLat = currentInfo.Lat,
                    LastLng = currentInfo.Lng,
                    LastSeen = currentInfo.Timestamp,
                    Status = computedStatus.ToFriendlyString()
                });
            }

            return statusList;
        }

        public bool RemoveVehicle(string vehicleId)
        {
            return _repository.Delete(vehicleId);
        }
    }
}