using MongoDB.Driver;
using Recordatorios.Application.Abstractions;
using Recordatorios.Domain;

namespace Recordatorios.Infrastructure.Mongo;

internal class FacturaRepository(MongoContext ctx) : IFacturaRepository
{
    public async Task<IReadOnlyList<Factura>> ObtenerTodasAsync(CancellationToken ct = default) =>
        (await ctx.Facturas.Find(FilterDefinition<FacturaDocument>.Empty).ToListAsync(ct))
            .Select(d => d.ToDomain()).ToList();

    public async Task<IReadOnlyList<Factura>> ObtenerPorClienteAsync(string clienteId, CancellationToken ct = default) =>
        (await ctx.Facturas.Find(f => f.ClienteId == clienteId).ToListAsync(ct))
            .Select(d => d.ToDomain()).ToList();

    public async Task ActualizarEstadoAsync(string facturaId, string nuevoEstado, CancellationToken ct = default)
    {
        var res = await ctx.Facturas.UpdateOneAsync(
            f => f.Id == facturaId, Builders<FacturaDocument>.Update.Set(f => f.Estado, nuevoEstado), cancellationToken: ct);
        if (res.MatchedCount == 0) throw new KeyNotFoundException($"Factura '{facturaId}' no existe.");
    }
}