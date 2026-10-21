using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Recordatorios.Infrastructure.Mongo;

internal class MongoContext
{
    static MongoContext()
    {
        BsonSerializer.TryRegisterSerializer(new DecimalSerializer(BsonType.Decimal128));
    }

    public MongoContext(IOptions<MongoSettings> options)
    {
        var s = options.Value;
        var db = new MongoClient(s.ConnectionString).GetDatabase(s.Database);
        Clientes = db.GetCollection<ClienteDocument>("clientes");
        Facturas = db.GetCollection<FacturaDocument>("facturas");
    }

    public IMongoCollection<ClienteDocument> Clientes { get; }
    public IMongoCollection<FacturaDocument> Facturas { get; }
}