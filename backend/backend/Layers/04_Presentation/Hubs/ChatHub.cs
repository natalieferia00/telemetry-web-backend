using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using FleetTelemetry.API.Layers._04_Presentation.HubsHandlers.Common;
using FleetTelemetry.API.Layers._04_Presentation.HubsHandlers.Providers;

namespace FleetTelemetry.API.Layers._04_Presentation.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ChatActionProvider _chatActionProvider;

        public ChatHub(ChatActionProvider chatActionProvider)
        {
            _chatActionProvider = chatActionProvider;
        }

        public async Task HandleAction(ActionRequest request)
        {
            try
            {
                var strategy = _chatActionProvider.GetStrategy(request.Type);
                await strategy.ExecuteAsync(this, request.Payload);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ReceiveAction", new ActionResponse("ERROR", new { message = ex.Message }));
            }
        }
    }
}