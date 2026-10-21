namespace Recordatorios.Domain;

/// <summary>Estados posibles de una factura y la regla de transición entre recordatorios.</summary>
public static class EstadoFactura
{
    public const string PrimerRecordatorio = "primerrecordatorio";
    public const string SegundoRecordatorio = "segundorecordatorio";
    public const string Desactivado = "desactivado";

    /// <summary>Indica si la factura debe procesarse (está en un estado de recordatorio).</summary>
    public static bool RequiereRecordatorio(string? estado) =>
        estado is PrimerRecordatorio or SegundoRecordatorio;

    /// <summary>Devuelve el estado al que pasa la factura tras enviar el recordatorio.</summary>
    public static string Siguiente(string estado) => estado switch
    {
        PrimerRecordatorio => SegundoRecordatorio,
        SegundoRecordatorio => Desactivado,
        _ => throw new InvalidOperationException($"El estado '{estado}' no admite transición.")
    };
}