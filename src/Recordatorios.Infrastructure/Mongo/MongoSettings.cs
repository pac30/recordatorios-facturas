namespace Recordatorios.Infrastructure.Mongo;

public class MongoSettings
{
    public const string Section = "Mongo";
    public string ConnectionString { get; set; } = "";
    public string Database { get; set; } = "recordatorios";
}