using System.Collections.Concurrent;
using System.Linq;
using FleetTelemetry.API.Layers._01_Domain.Entities;
using FleetTelemetry.API.Layers._02_Application.Services;
using FleetTelemetry.API.Layers._03_Infrastructure.Persistence;
using Moq;
using Xunit;

namespace backend.Layers.Tests
{
    public class VehicleServiceTests
    {
        [Fact]
        public void GetCurrentFleetStatus_ShouldReturnNoSignal_WhenTimeExceeds120Seconds()
        {
            var mockRepository = new Mock<IVehicleRepository>();
            var fakeStore = new ConcurrentDictionary<string, List<Vehicle>>();
            
            var oldTime = DateTime.UtcNow.AddSeconds(-130); 
            var vehicle = new Vehicle("VH-TEST", 4.60, -74.00, oldTime);
            
            fakeStore.TryAdd("VH-TEST", [vehicle]);

            mockRepository.Setup(r => r.GetAllHistory()).Returns(fakeStore);
            var service = new VehicleService(mockRepository.Object);
            
            var result = service.GetCurrentFleetStatus().ToList();
            
            Assert.Single(result);
            Assert.Equal("Sin señal", result.First().Status);
        }
    }
}