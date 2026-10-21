# Recordatorios de facturas

Lee facturas de MongoDB, envía por email un recordatorio según su estado y actualiza el estado. Incluye una interfaz Angular con el resumen de facturas por cliente.

| Estado actual | Email que recibe el cliente | Estado nuevo |
|---|---|---|
| `primerrecordatorio` | Pasó a segundo recordatorio | `segundorecordatorio` |
| `segundorecordatorio` | Va a ser desactivada | `desactivado` |

El estado solo se actualiza **después** de que el correo se envió bien. Si el envío falla, la factura conserva su estado y el proceso continúa con las demás. Los correos son HTML (con tabla de detalle de la factura y color según el aviso) y llevan también versión en texto plano.

> Documentación detallada: [planteamiento y arquitectura](docs/ARQUITECTURA.md) · [resultado, progreso y pasos a seguir](RESULTADO.md)

## Arquitectura (Clean Architecture)

```
src/
  Recordatorios.Domain          Entidades y regla de transición (sin dependencias)
  Recordatorios.Application     Casos de uso + interfaces (IClienteRepository, IFacturaRepository, IEmailSender)
  Recordatorios.Infrastructure  MongoDB.Driver y MailKit (implementan las interfaces)
  Recordatorios.Api             ASP.NET Core: controllers, DI, CORS
tests/Recordatorios.Tests       xUnit + NSubstitute + FluentAssertions
frontend/                       Angular 20 (standalone, signals)
```

La dependencia apunta siempre hacia adentro: Domain no conoce a nadie y Application solo conoce Domain. MongoDB y SMTP se pueden cambiar sin tocar la lógica, y las dependencias se inyectan por constructor (DI nativa de .NET).

## Ejecutar en local (sin cuentas externas)

Requisitos: .NET 9 SDK, Node 20+ y Angular CLI. La configuración por defecto de `Development` ya apunta a un MongoDB local (`localhost:27017`) y a un SMTP local de pruebas (`localhost:1025`).

**1. Levantar MongoDB y la bandeja de correo de pruebas**

Con Docker:
```
docker compose up -d
```
Esto levanta MongoDB en `27017` y [Mailpit](https://mailpit.axllent.org/) (SMTP en `1025`, bandeja web en http://localhost:8025).

> Si ya tienes un MongoDB corriendo en el puerto `27017` (por ejemplo un servicio local), detén ese servicio antes de `docker compose up`, o el contenedor no podrá publicar el puerto.

Sin Docker: instala MongoDB Community (servicio en `27017`) y descarga el ejecutable de Mailpit.

**2. API y frontend**
```
dotnet run --project src/Recordatorios.Api        # http://localhost:5236
cd frontend && npm install && npx ng serve        # http://localhost:4200
```

**3. Probar**
1. Abre http://localhost:4200 y pulsa **Reiniciar datos demo** (crea 3 clientes con 7 facturas en distintos estados).
2. Pulsa **Enviar recordatorios** (o el botón **Enviar** de un cliente).
3. Mira los correos en http://localhost:8025 y cómo cambian los estados en la interfaz.

## Ejecutar desde Visual Studio 2022
1. Abre `Recordatorios.sln` (Archivo → Abrir → Proyecto o solución). Espera a que termine la restauración de paquetes NuGet.
2. Clic derecho en **Recordatorios.Api → Establecer como proyecto de inicio**, y elige el perfil **http** (no `https`: el frontend usa el puerto 5236).
3. Pulsa **F5**. Con MongoDB y Mailpit levantados (ver arriba) la API queda en http://localhost:5236.
4. Pruebas: **Prueba → Explorador de pruebas → Ejecutar todas** (20 pruebas).
5. Interfaz: `cd frontend && npm install && npx ng serve` → http://localhost:4200.
## Ejecutar con MongoDB Atlas y Gmail (opcional)
Los secretos van en `user-secrets` (fuera del repositorio) y reemplazan la configuración local:
```
cd src/Recordatorios.Api
dotnet user-secrets set "Mongo:ConnectionString" "mongodb+srv://USUARIO:CLAVE@CLUSTER/"
dotnet user-secrets set "Smtp:Host" "smtp.gmail.com"
dotnet user-secrets set "Smtp:Port" "587"
dotnet user-secrets set "Smtp:Security" "StartTls"
dotnet user-secrets set "Smtp:User" "tu_cuenta@gmail.com"
dotnet user-secrets set "Smtp:From" "tu_cuenta@gmail.com"
dotnet user-secrets set "Smtp:Password" "contraseña de aplicación de 16 caracteres"
dotnet user-secrets set "Demo:EmailBase" "tu_cuenta@gmail.com"
```
`Demo:EmailBase` genera los correos de los clientes demo como alias (`tu_cuenta+ana@gmail.com`), que llegan a la misma bandeja. En producción se usan variables de entorno equivalentes (`Mongo__ConnectionString`, `Smtp__Password`...).

## Personalizar los correos
- Empresa y contacto: sección `Correo` de `appsettings.json` (`Empresa`, `Contacto`).
- Textos y diseño: `src/Recordatorios.Application/Recordatorios/MensajeRecordatorio.cs`.

## API
| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/facturas` | Resumen de todas las facturas con su cliente |
| POST | `/api/recordatorios/procesar` | Procesa los 3 clientes |
| POST | `/api/recordatorios/procesar/{clienteId}` | Procesa un cliente |
| POST | `/api/demo/reiniciar` | Recarga los datos demo (solo Development) |

## Pruebas
```
dotnet test                                   # 20 pruebas (dominio, proceso y correos)
cd frontend && npx ng test --watch=false      # 7 pruebas de la interfaz
```