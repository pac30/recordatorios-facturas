using MongoDB.Bson.Serialization.Attributes;
using Recordatorios.Domain;

namespace Recordatorios.Infrastructure.Mongo;

[BsonIgnoreExtraElements]
internal class ClienteDocument
{
    [BsonId] public string Id { get; set; } = "";
    [BsonElement("nombre")] public string Nombre { get; set; } = "";
    [BsonElement("email")] public string Email { get; set; } = "";

    public Cliente ToDomain() => new() { Id = Id, Nombre = Nombre, Email = Email };
    public static ClienteDocument From(Cliente c) => new() { Id = c.Id, Nombre = c.Nombre, Email = c.Email };
}

[BsonIgnoreExtraElements]
internal class FacturaDocument
{
    [BsonId] public string Id { get; set; } = "";
    [BsonElement("clienteId")] public string ClienteId { get; set; } = "";
    [BsonElement("numero")] public string Numero { get; set; } = "";
    [BsonElement("monto")] public decimal Monto { get; set; }
    [BsonElement("fechaVencimiento")] public DateTime FechaVencimiento { get; set; }
    [BsonElement("estado")] public string Estado { get; set; } = "";

    public Factura ToDomain() => new()
    {
        Id = Id, ClienteId = ClienteId, Numero = Numero, Monto = Monto,
        FechaVencimiento = FechaVencimiento, Estado = Estado
    };

    public static FacturaDocument From(Factura f) => new()
    {
        Id = f.Id, ClienteId = f.ClienteId, Numero = f.Numero, Monto = f.Monto,
        FechaVencimiento = f.FechaVencimiento, Estado = f.Estado
    };
}