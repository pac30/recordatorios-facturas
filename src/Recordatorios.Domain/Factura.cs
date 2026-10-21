namespace Recordatorios.Domain;

public class Factura
{
    public required string Id { get; init; }
    public required string ClienteId { get; init; }
    public required string Numero { get; init; }
    public required decimal Monto { get; init; }
    public required DateTime FechaVencimiento { get; init; }
    public required string Estado { get; set; }

    public bool RequiereRecordatorio => EstadoFactura.RequiereRecordatorio(Estado);

    public string EstadoSiguiente => EstadoFactura.Siguiente(Estado);
}