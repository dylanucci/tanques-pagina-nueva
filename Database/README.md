# Base de datos

MySQL debe estar disponible en `127.0.0.1:3306`.

La cadena se configura en `Backend/TankManagement.Api/appsettings.json` y `appsettings.Development.json`.

Desde `Backend/TankManagement.Api`:

```bash
dotnet restore
dotnet tool install --global dotnet-ef
dotnet ef database update
dotnet run
```

Las migraciones incluidas crean/actualizan:

- Roles
- Usuarios
- Tanques, incluyendo `AlturaCM` (`double`)
- UsuarioTanque
- Solicitudes
