using Microsoft.AspNetCore.Mvc;
using Recordatorios.Application.Facturas;

namespace Recordatorios.Api.Controllers;

[ApiController]
[Route("api/facturas")]
public class FacturasController(ConsultaFacturas consulta) : ControllerBase
{
    /// <summary>Resumen de todas las facturas con los datos de su cliente.</summary>
    [HttpGet]
    public async Task<IReadOnlyList<FacturaResumen>> Get(CancellationToken ct) =>
        await consulta.ObtenerResumenAsync(ct);
}