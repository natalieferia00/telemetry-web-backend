namespace FleetTelemetry.API.Layers._01_Domain.Enums
{
    public enum VehicleStatus
    {
        Moving,
        Stopped,
        NoSignal
    }

    public static class VehicleStatusExtensions
    {
        public static string ToFriendlyString(this VehicleStatus status)
        {
            return status switch
            {
                VehicleStatus.Moving => "En movimiento",
                VehicleStatus.Stopped => "Detenido",
                VehicleStatus.NoSignal => "Sin señal",
                _ => "Desconocido"
            };
        }
    }
}