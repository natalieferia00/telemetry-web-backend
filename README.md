# Sistema de Monitoreo de Flotas GPS

## 1. Resumen ejecutivo
Este repositorio implementa un flujo completo de telemetría GPS para flotas:

1. un simulador genera coordenadas,
2. la API REST las recibe y procesa,
3. el backend calcula el estado del vehículo,
4. el frontend presenta el dashboard con la información actualizada.

## 2. Qué resuelve este proyecto
- Recibe lecturas GPS por HTTP.
- Registra y mantiene el historial reciente por vehículo.
- Calcula si un vehículo está `Moving`, `Stopped` o `NoSignal`.
- Expone el estado de la flota para uso en una interfaz Angular.
- Permite validar el flujo completo en local con un simulador realista.

## 3. Estructura del proyecto
- `backend-finanzauto-fleet/backend`: API REST en .NET 10 con capas de dominio, aplicación,
  infraestructura y presentación.
- `telemetry-web`: frontend Angular para visualizar el estado de la flota.
- `telemetry-simulator`: script Node.js que genera datos GPS para alimentar la API.

## 4. Flujo de operación
1. El simulador envía un `POST` a `/api/gps` con `VehicleId`, `Lat`, `Lng` y `Timestamp`.
2. El backend valida el payload y lo transforma en una lectura de vehículo.
3. El servicio calcula el estado reciente usando el historial acumulado.
4. El frontend consulta `/api/vehicles` y renderiza el dashboard.
5. En la versión con broadcasting en tiempo real, además se emiten eventos por WebSocket/SignalR.

## 5. Requisitos previos
- .NET SDK 10 o superior
- Node.js 18+ y npm
- Angular CLI opcional para invocar `ng` directamente

## 6. Arranque rápido

### Cómo probarlo en 2 minutos
1. Abre el backend:

```bash
cd backend-finanzauto-fleet/backend
dotnet run
```

2. Abre el frontend en otra terminal:

```bash
cd telemetry-web
npx ng serve --host 0.0.0.0 --port 4200
```

3. Inicia el simulador en otra terminal:

```bash
node telemetry-simulator/simulator.js
```

Con eso, la API queda en `http://localhost:5136`, el frontend en `http://localhost:4200` y el dashboard comienza a consumir `GET /api/vehicles`.

### 1) Backend

```bash
cd backend-finanzauto-fleet/backend
dotnet restore
dotnet run
```

El backend queda disponible en la URL configurada por `dotnet run` y por el perfil de lanzamiento
(definido en `Properties/launchSettings.json`). En la versión actual el puerto esperado es el de
`http://localhost:5136`, pero conviene verificar la salida real.

### 2) Frontend Angular

```bash
cd telemetry-web
npm install
npx ng serve --host 0.0.0.0 --port 4200
```

No es necesario ejecutar con `angular`; lo correcto es usar `npx ng serve` desde `telemetry-web`.
La UI queda disponible por defecto en `http://localhost:4200`.

Endpoint que consume el frontend:
- `GET http://localhost:5136/api/vehicles`

Este endpoint es el que consulta el estado actual de la flota y alimenta el dashboard.
El flujo real del frontend no escribe datos hacia `/api/gps`; esa ingestión la hace el simulador o cualquier cliente externo.

### 3) Simulador

```bash
node telemetry-simulator/simulator.js
```

El simulador apunta por defecto a `http://localhost:5136/api/gps`. Si cambias el puerto del backend,
ajusta la constante `API_URL` en el archivo `telemetry-simulator/simulator.js`.

## 7. Endpoints expuestos

### POST `/api/gps`
Ingesta una coordenada GPS para un vehículo.

Payload de ejemplo:

```json
{
  "VehicleId": "VH-001",
  "Lat": 4.609710,
  "Lng": -74.081700,
  "Timestamp": "2026-07-10T12:34:56.000Z"
}
```

Respuesta esperada:

```json
{
  "message": "Coordenada almacenada"
}
```

### GET `/api/vehicles`
Devuelve el estado actual de la flota.
Este es el endpoint que usa el frontend Angular para pintar el dashboard.

Respuesta esperada:

```json
[
  {
    "vehicleId": "VH-001",
    "lastLat": 4.60971,
    "lastLng": -74.0817,
    "lastSeen": "2026-07-10T12:34:56Z",
    "status": "Moving"
  }
]
```

### DELETE `/api/vehicles/{id}`
Elimina el historial asociado a un vehículo.

## 8. Lógica de negocio interna
- El DTO `GpsIngestDto` valida que `VehicleId`, `Lat`, `Lng` y `Timestamp` lleguen correctamente.
- El repositorio en memoria mantiene el historial de las últimas lecturas por vehículo.
- El servicio `VehicleService` determina el estado como:
  - `Moving`: si hubo desplazamiento en la última ventana de observación.
  - `Stopped`: cuando el vehículo no se movió recientemente.
  - `NoSignal`: si la última lectura supera el umbral temporal permitido.

## 9. Frontend y consumo de datos
- El servicio `TelemetryService` del frontend consume `http://localhost:5136/api/vehicles` para consultar el estado de la flota.
- El endpoint mostrado en la UI es `GET /api/vehicles`, que es la lectura principal del dashboard.
- El backend expone la información de vehículos en la ruta `/api/vehicles`.
- El dashboard actual usa polling cada 10 segundos para refrescar la vista.
- Si se habilita SignalR/WebSocket, el frontend puede escuchar el evento `ReceiveAction` y reaccionar
  a eventos como `GPS_INGESTED` y `VEHICLE_DELETED` sin depender solo del polling.
- El patrón de componentes está orientado a rendimiento con `ChangeDetectionStrategy.OnPush`.

## 10. WebSocket / streaming en tiempo real
En la versión del controlador que integra `IHubContext<ChatHub>`, el backend no solo responde con REST,
sino que también emite eventos en tiempo real a todos los clientes conectados por SignalR/WebSocket.

Comportamiento real:
- `POST /api/gps`: después de validar y procesar la lectura, el backend envía un evento `ReceiveAction`
  con `ActionResponse("GPS_INGESTED", responseData)`.
- `DELETE /api/vehicles/{id}`: después de eliminar el vehículo, el backend envía un evento `ReceiveAction`
  con `ActionResponse("VEHICLE_DELETED", new { vehicleId = id })`.

Esto permite que el frontend escuche el broadcast y actualice la UI de forma inmediata, sin esperar
solamente al próximo polling.

Regla práctica:
- REST: consultas, creación del estado inicial y operaciones síncronas.
- WebSocket/SignalR: difundir cambios de flota en vivo a todos los clientes conectados.

## 11. Ejecución de pruebas

```bash
cd backend-finanzauto-fleet/backend
dotnet test
```

## 12. Solución de problemas rápida
- Si el frontend no muestra datos, revisa que el backend está corriendo y que CORS permite el origen del navegador.
- Si el simulador falla, valida la URL del backend en `telemetry-simulator/simulator.js`.
- Si recibes `400 Bad Request`, revisa el payload: `VehicleId`, `Lat`, `Lng` y `Timestamp` deben estar presentes y con tipos válidos.

## 13. Pregunta técnica
Si en un sistema real existiera tanto un caché (Redis) como una base de datos persistente,
¿qué deberías garantizar al eliminar un vehículo para evitar inconsistencias entre ambos?

La respuesta correcta es garantizar que la eliminación sea coherente en ambos niveles como una sola decisión de negocio. No basta con borrar el vehículo del caché y asumir que la base de datos quedó limpia, ni al revés. Lo que necesitas asegurar es que el sistema no termine en un estado parcialmente sincronizado:

- si la base de datos confirma el borrado, el caché también debe quedar invalidado o eliminado;
- si el caché se limpia pero la base de datos no, el vehículo puede seguir existiendo para la capa persistente;
- si el borrado en una capa falla, la operación debe ser reintentada o revertida de forma controlada.

En palabras simples: el caché no debe ser la fuente de verdad; debe ser una vista efímera.
La fuente de verdad es la base de datos y la eliminación debe propagarse de forma consistente,
idealmente con una operación transaccional o una estrategia de outbox/saga para evitar que
Redis y la BD queden en estados distintos.

## 14. Buenas prácticas y recomendaciones
- Sustituir el repositorio en memoria por una base de datos persistente en producción.
- Reservar Redis para caché de lectura o para acelerar consultas frecuentes, no como fuente de verdad.
- Externalizar puertos, URLs y configuraciones por `appsettings.json` o variables de entorno.
- Añadir autenticación y autorización si la ingestión se expone en entornos compartidos.
- Medir la retención del historial y el tamaño de la ventana de estado por vehículo.

## 15. Contribuciones
Para cambios funcionales, abre un PR con:
- descripción del cambio,
- pasos de prueba,
- ejemplo de payloads.

## 16. Contacto y licencia
- Abre un issue para dudas o solicitudes de demo.
- Añade una licencia si deseas publicar el proyecto externamente.

---

README generado a partir del código del repositorio para facilitar arranque, pruebas y evaluación técnica.
