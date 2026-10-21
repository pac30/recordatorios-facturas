using Recordatorios.Domain;

namespace Recordatorios.Application.Abstractions;

public interface IFacturaRepository
{
    Task<IReadOnlyList<Factura>> ObtenerTodasAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Factura>> ObtenerPorClienteAsync(string clienteId, CancellationToken ct = default);
    Task ActualizarEstadoAsync(string facturaId, string nuevoEstado, CancellationToken ct = default);
}