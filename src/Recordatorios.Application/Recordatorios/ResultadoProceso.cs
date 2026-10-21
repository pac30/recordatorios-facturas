namespace Recordatorios.Application.Recordatorios;

public record FacturaProcesada(
    string FacturaId, string Numero, string EstadoAnterior, string? EstadoNuevo, bool Exito, string? Error);

public record ResultadoCliente(string ClienteId, string Cliente, string Email, IReadOnlyList<FacturaProcesada> Facturas);