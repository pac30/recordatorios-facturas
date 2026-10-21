using MongoDB.Driver;
using Recordatorios.Application.Abstractions;
using Recordatorios.Domain;

namespace Recordatorios.Infrastructure.Mongo;

/// <summary>Carga 3 clientes con facturas en distintos estados. Se puede repetir para reiniciar la demo.</summary>
internal class DatosDemo : IDatosDemo
{
    private readonly MongoContext _ctx;
    public DatosDemo(MongoContext ctx) => _ctx = ctx;

    public async Task<(int Clientes, int Facturas)> ReiniciarAsync(string emailBase, CancellationToken ct = default)
    {
        var clientes = new[]
        {
            new Cliente { Id = "cli-001", Nombre = "Ana Torres",   Email = ConAlias(emailBase, "ana") },
            new Cliente { Id = "cli-002", Nombre = "Luis Gómez",   Email = ConAlias(emailBase, "luis") },
            new Cliente { Id = "cli-003", Nombre = "Marta Ruiz",   Email = ConAlias(emailBase, "marta") },
        };
        var hoy = DateTime.UtcNow.Date;
        var facturas = new[]
        {
            F("fac-001", "cli-001", "F-1001", 1200000m, hoy.AddDays(-20), EstadoFactura.PrimerRecordatorio),
            F("fac-002", "cli-001", "F-1002",  450000m, hoy.AddDays(-45), EstadoFactura.SegundoRecordatorio),
            F("fac-003", "cli-001", "F-1003",  300000m, hoy.AddDays(10),  "pagada"),
            F("fac-004", "cli-002", "F-2001",  980000m, hoy.AddDays(-30), EstadoFactura.PrimerRecordatorio),
            F("fac-005", "cli-002", "F-2002",  150000m, hoy.AddDays(-60), EstadoFactura.Desactivado),
            F("fac-006", "cli-003", "F-3001", 2300000m, hoy.AddDays(-50), EstadoFactura.SegundoRecordatorio),
            F("fac-007", "cli-003", "F-3002",  620000m, hoy.AddDays(-15), EstadoFactura.PrimerRecordatorio),
        };

        await _ctx.Clientes.DeleteManyAsync(FilterDefinition<ClienteDocument>.Empty, ct);
        await _ctx.Facturas.DeleteManyAsync(FilterDefinition<FacturaDocument>.Empty, ct);
        await _ctx.Clientes.InsertManyAsync(clientes.Select(ClienteDocument.From), cancellationToken: ct);
        await _ctx.Facturas.InsertManyAsync(facturas.Select(FacturaDocument.From), cancellationToken: ct);
        return (clientes.Length, facturas.Length);
    }

    /// <summary>"yo@gmail.com" + "ana" → "yo+ana@gmail.com": todos los correos llegan a la misma bandeja.</summary>
    private static string ConAlias(string email, string alias)
    {
        var at = email.IndexOf('@');
        return at < 0 ? email : $"{email[..at]}+{alias}{email[at..]}";
    }

    private static Factura F(string id, string cli, string num, decimal monto, DateTime venc, string estado) =>
        new() { Id = id, ClienteId = cli, Numero = num, Monto = monto, FechaVencimiento = venc, Estado = estado };
}