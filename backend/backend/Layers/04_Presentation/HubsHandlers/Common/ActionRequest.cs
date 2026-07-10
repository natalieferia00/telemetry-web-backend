using System.Text.Json;

namespace FleetTelemetry.API.Layers._04_Presentation.HubsHandlers.Common
{
    public record ActionRequest(string Type, JsonElement Payload);
}