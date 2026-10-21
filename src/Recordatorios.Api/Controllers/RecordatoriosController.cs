using Microsoft.AspNetCore.Mvc;
using Recordatorios.Application.Recordatorios;

namespace Recordatorios.Api.Controllers;

[ApiController]
[Route("api/recordatorios")]
public class RecordatoriosController(ProcesadorRecordatorios procesador) : ControllerBase
{
    /// <summary>Procesa las facturas en recordatorio de todos los clientes.</summary>
    [HttpPost("procesar")]
    public async Task<IReadOnlyList<ResultadoCliente>> ProcesarTodos(CancellationToken ct) =>
        await procesador.ProcesarTodosAsync(ct);

    /// <summary>Procesa las facturas en recordatorio de un solo cliente.</summary>
    [HttpPost("procesar/{clienteId}")]
    public async Task<ActionResult<ResultadoCliente>> ProcesarCliente(string clienteId, CancellationToken ct)
    {
        try { return await procesador.ProcesarClienteAsync(clienteId, ct); }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }
}