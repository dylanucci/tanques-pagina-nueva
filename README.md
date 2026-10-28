# TankManagement

Sistema de gestión de tanques con API ASP.NET Core .NET 8, MySQL, interfaz HTML/CSS/JavaScript y Arduino/ESP32 conectado por USB.

## Componentes y puertos

- API: `http://localhost:3000`; Swagger: `http://localhost:3000/swagger`.
- MySQL: `localhost:3306`, base `tanques`.
- Arduino/ESP32: puerto serie USB `COM3`, 115200 baudios, gestionado **directamente por la API**. No se requiere ni existe un proceso BridgeApp independiente.

## Configurar y ejecutar

Edita `Backend/TankManagement.Api/appsettings.json` y coloca la contraseña real en `ConnectionStrings:DefaultConnection`. Requiere MySQL ejecutándose y una base a la que el usuario configurado pueda acceder.

Desde `Backend/TankManagement.Api` ejecuta:

```bash
dotnet restore
dotnet tool install --global dotnet-ef   # solo la primera vez, si no está instalado
dotnet ef database update
dotnet run
```

La API crea los roles/usuario inicial según el seeder del proyecto. Para desarrollo, abre `Frontend/login.html` mediante Live Server o un servidor estático. La interfaz consume `http://localhost:3000/api`.

## Comunicación serial integrada en la API

El servicio serial de la API abre `ArduinoSerial:PortName` (por defecto `COM3`) y reintenta la conexión si el Arduino se desconecta. Las lecturas `tanqueId;distanciaCm` se validan y actualizan directamente usando el servicio de tanques; ya no se hace un POST HTTP de vuelta a la propia API. También se acepta una línea que solo contenga la distancia, usando `ArduinoSerial:DefaultTankId`.

Configurable en `Backend/TankManagement.Api/appsettings.json`:

- `PortName`: `COM3`.
- `BaudRate`: `115200`.
- `ReconnectDelaySeconds`: intervalo de reintento.
- `CommandTimeoutSeconds`: espera máxima por la confirmación del Arduino.
- `NoDataTimeoutSeconds`: fuerza reconexión si el puerto no entrega líneas durante ese tiempo.
- `DefaultTankId` y `MaxDistanceCm`: validación/compatibilidad de lecturas.

**Importante:** la API debe ser el único proceso que abra COM3. Cierra el monitor serie del IDE de Arduino y cualquier terminal o servicio que use ese puerto antes de iniciar la API.

## Auto llenado

La interfaz Dueño muestra el botón **Auto Llenado al 80 %** para cada tanque. Envía `POST /api/tanques/{id}/auto-llenado`. La API envía por serie `CMD;{id};FILL80` y espera un `ACK` del microcontrolador. Si el puerto no está conectado o el Arduino no confirma el comando, se informa el error en la interfaz.

El llenado automático de respaldo también se ejecuta en el Arduino: inicia al alcanzar el 10 % o menos y cierra la electroválvula al llegar al 80 %. Para el tanque configurado con altura de 17 cm, con el sensor montado arriba y sin desfase, los umbrales calculados son:

- Inicio al 10 %: distancia al agua de `17 × (1 − 0,10) = 15,3 cm`.
- Final al 80 %: distancia al agua de `17 × (1 − 0,80) = 3,4 cm`.

Una distancia de 16,2 cm representa aproximadamente 4,7 % de llenado, no 10 %. El diámetro de 10 cm se utiliza para calcular el volumen cilíndrico aproximado en el sketch; el porcentaje se calcula a partir de la altura del líquido. Ante una lectura ultrasónica inválida durante el llenado, el Arduino cierra la válvula.

## Protocolo serial del sketch

- Telemetría periódica: `1;25.50`.
- Solicitud de la API: `CMD;1;FILL80`.
- Confirmación: `ACK;1;FILL80;STARTED` (o `ALREADY_AT_TARGET` / `ERROR`).

Asegúrate de que `TANQUE_ID` en `Arduino/TankController/TankController.ino` coincide con el ID del tanque que representa ese dispositivo físico. Los parámetros físicos están definidos al inicio del sketch (`ALTURA_TANQUE_CM = 17`, `DIAMETRO_TANQUE_CM = 10`).
