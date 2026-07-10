using System;
using System.Collections.Generic;
using System.Linq;
using FleetTelemetry.API.Layers._04_Presentation.HubsHandlers.Common;

namespace FleetTelemetry.API.Layers._04_Presentation.HubsHandlers.Providers
{
    public class ChatActionProvider
    {
        private readonly IEnumerable<IChatActionStrategy> _strategies;

        public ChatActionProvider(IEnumerable<IChatActionStrategy> strategies)
        {
            _strategies = strategies;
        }

        public IChatActionStrategy GetStrategy(string actionType)
        {
            var strategy = _strategies.FirstOrDefault(s => s.ActionType.Equals(actionType, StringComparison.OrdinalIgnoreCase));

            if (strategy == null)
                throw new Exception($"El tipo de acción '{actionType}' no está soportado en el sistema.");

            return strategy;
        }
    }
}