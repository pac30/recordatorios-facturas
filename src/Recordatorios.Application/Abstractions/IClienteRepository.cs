using Recordatorios.Domain;

namespace Recordatorios.Application.Abstractions;

public interface IClienteRepository
{
    Task<IReadOnlyList<Cliente>> ObtenerTodosAsync(CancellationToken ct = default);
    Task<Cliente?> ObtenerPorIdAsync(string id, CancellationToken ct = default);
}