using MongoDB.Driver;
using Recordatorios.Application.Abstractions;
using Recordatorios.Domain;

namespace Recordatorios.Infrastructure.Mongo;

internal class ClienteRepository(MongoContext ctx) : IClienteRepository
{
    public async Task<IReadOnlyList<Cliente>> ObtenerTodosAsync(CancellationToken ct = default) =>
        (await ctx.Clientes.Find(FilterDefinition<ClienteDocument>.Empty).SortBy(c => c.Nombre).ToListAsync(ct))
            .Select(d => d.ToDomain()).ToList();

    public async Task<Cliente?> ObtenerPorIdAsync(string id, CancellationToken ct = default) =>
        (await ctx.Clientes.Find(c => c.Id == id).FirstOrDefaultAsync(ct))?.ToDomain();
}