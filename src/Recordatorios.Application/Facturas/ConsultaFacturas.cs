using Recordatorios.Application.Abstractions;

namespace Recordatorios.Application.Facturas;

public record FacturaResumen(
    string Id, string Numero, string ClienteId, string Cliente, string Email,
    decimal Monto, DateTime FechaVencimiento, string Estado);

/// <summary>Consulta de solo lectura para la interfaz gráfica.</summary>
public class ConsultaFacturas(IClienteRepository clientes, IFacturaRepository facturas)
{
    public async Task<IReadOnlyList<FacturaResumen>> ObtenerResumenAsync(CancellationToken ct = default)
    {
        var porId = (await clientes.ObtenerTodosAsync(ct)).ToDictionary(c => c.Id);
        return (await facturas.ObtenerTodasAsync(ct))
            .Where(f => porId.ContainsKey(f.ClienteId))
            .Select(f =>
            {
                var c = porId[f.ClienteId];
                return new FacturaResumen(f.Id, f.Numero, c.Id, c.Nombre, c.Email, f.Monto, f.FechaVencimiento, f.Estado);
            })
            .OrderBy(r => r.Cliente).ThenBy(r => r.Numero)
            .ToList();
    }
}