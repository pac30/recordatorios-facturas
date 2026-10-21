# Resultado, progreso y pasos a seguir

Prueba técnica: proceso de recordatorios de facturas (.NET + MongoDB + Angular).
Cómo ejecutarlo: [README.md](README.md) · Planteamiento y arquitectura: [docs/ARQUITECTURA.md](docs/ARQUITECTURA.md)

## Resultado
- Proceso de recordatorios implementado: lectura desde MongoDB, filtro por estado, envío de email y actualización del estado **solo tras el envío exitoso** (`primerrecordatorio` → `segundorecordatorio`, `segundorecordatorio` → `desactivado`).
- Funciona para varios clientes: procesa todos o uno solo. Los datos demo incluyen 3 clientes con 7 facturas.
- Interfaz Angular con facturas agrupadas por cliente, tarjetas de resumen, búsqueda, filtro por estado y botones para ejecutar el proceso (global o por cliente).
- Correos HTML personalizados (tabla con el detalle de la factura, color según el aviso, datos de la empresa) y versión en texto plano.
- Se puede ejecutar sin cuentas externas (MongoDB local y bandeja de pruebas Mailpit, con o sin Docker) o con MongoDB Atlas y Gmail.

## Progreso (puntos de evaluación)
| Punto | Estado |
|---|---|
| Clean Code y SOLID | Hecho: capas Domain / Application / Infrastructure / Api; ver sección SOLID en `docs/ARQUITECTURA.md` |
| Unit Test | Hecho: 20 en .NET (xUnit) y 7 en Angular, todas en verde |
| Documentación, planteamiento y arquitectura | Hecho: `README.md` y `docs/ARQUITECTURA.md` |
| Inyección de dependencias | Hecho: constructores + `AddInfrastructure` + patrón Options |
| FrontEnd en Angular | Hecho: Angular 20 |
| Desarrollo en Visual Studio con .NET y C# | Hecho: solución `.sln` (.NET 9), compilada y probada en Visual Studio 2022 (20/20 en el Explorador de pruebas) |

## Verificaciones realizadas
- Flujo completo con **MongoDB Atlas y Gmail reales**: 5 correos enviados y 5 estados actualizados; las facturas `pagada` y `desactivado` quedaron intactas.
- Flujo completo **sin cuentas externas**: MongoDB local y Mailpit.
- Flujo completo con los contenedores de **`docker-compose.yml`** (MongoDB y Mailpit).
- **Copia limpia** del repositorio: compila, 20/20 pruebas .NET, `npm install` + build + 7/7 pruebas Angular.
- Ejecución con F5 y pruebas visuales en **Visual Studio 2022**.

## Pasos a seguir
- Ejecutar el proceso de forma automática (diaria) con un `BackgroundService` o un job programado.
- Registro de envíos (*outbox*) para evitar correos duplicados si falla la actualización tras enviar.
- Reintentos y cola de envío ante fallos temporales del SMTP.
- Pruebas de integración de los repositorios con MongoDB real (Testcontainers) y autenticación en la API.