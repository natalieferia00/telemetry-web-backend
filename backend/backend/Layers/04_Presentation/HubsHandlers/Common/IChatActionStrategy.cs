using System.Text.Json;
using Microsoft.AspNetCore.SignalR;

namespace FleetTelemetry.API.Layers._04_Presentation.HubsHandlers.Common
{
    public interface IChatActionStrategy
    {
        string ActionType { get; }
        Task ExecuteAsync(Hub hub, JsonElement payload);
    }
}