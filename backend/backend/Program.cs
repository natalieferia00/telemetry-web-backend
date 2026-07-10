using FleetTelemetry.API.Layers._02_Application.Services;
using FleetTelemetry.API.Layers._03_Infrastructure.Persistence;
using FleetTelemetry.API.Layers._03_Infrastructure.Errors;
using FleetTelemetry.API.Layers._04_Presentation.Hubs;
using FleetTelemetry.API.Layers._04_Presentation.HubsHandlers.Common;
using FleetTelemetry.API.Layers._04_Presentation.HubsHandlers.Providers;
using FleetTelemetry.API.Layers._04_Presentation.HubsHandlers.VehicleStrategies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); 

builder.Services.AddSignalR();

builder.Services.AddScoped<ChatActionProvider>();
builder.Services.AddScoped<IChatActionStrategy, IngestGpsStrategy>();

builder.Services.AddSingleton<IVehicleRepository, InMemoryVehicleRepository>();
builder.Services.AddScoped<IVehicleService, VehicleService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200", 
                "http://localhost:5173", 
                "http://localhost:3000"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();  
    app.UseSwaggerUI(); 
}

app.UseMiddleware<GlobalExceptionHandler>();

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/chathub");

app.Run();