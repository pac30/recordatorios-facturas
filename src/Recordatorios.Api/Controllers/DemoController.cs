using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Recordatorios.Application.Abstractions;

namespace Recordatorios.Api.Controllers;

[ApiController]
[Route("api/demo")]
public class DemoController(IDatosDemo datos, IOptions<DemoSettings> demo, IWebHostEnvironment env) : ControllerBase
{
    /// <summary>Borra y vuelve a cargar 3 clientes con facturas en distintos estados (solo Development).</summary>
    [HttpPost("reiniciar")]
    public async Task<IActionResult> Reiniciar(CancellationToken ct)
    {
        if (!env.IsDevelopment()) return NotFound();
        var (clientes, facturas) = await datos.ReiniciarAsync(demo.Value.EmailBase, ct);
        return Ok(new { clientes, facturas });
    }
}