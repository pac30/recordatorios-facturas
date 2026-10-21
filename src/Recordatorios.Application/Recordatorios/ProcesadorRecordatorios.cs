using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Recordatorios.Application.Abstractions;

namespace Recordatorios.Application.Recordatorios;

/// <summary>
/// Caso de uso: por cada factura en recordatorio envía el correo y, solo si se envió,
/// actualiza el estado en la base de datos.
/// </summary>
public class ProcesadorRecordatorios(
    IClienteRepository clientes,
    IFacturaRepository facturas,
    IEmailSender email,
    IOptions<ConfiguracionCorreo> config,
    ILogger<ProcesadorRecordatorios> logger)
{
    public async Task<IReadOnlyList<ResultadoCliente>> ProcesarTodosAsync(CancellationToken ct = default)
    {
        var resultados = new List<ResultadoCliente>();
        foreach (var cliente in await clientes.ObtenerTodosAsync(ct))
            resultados.Add(await ProcesarClienteAsync(cliente.Id, ct));
        return resultados;
    }

    public async Task<ResultadoCliente> ProcesarClienteAsync(string clienteId, CancellationToken ct = default)
    {
        var cliente = await clientes.ObtenerPorIdAsync(clienteId, ct)
            ?? throw new KeyNotFoundException($"Cliente '{clienteId}' no existe.");

        var procesadas = new List<FacturaProcesada>();
        foreach (var factura in await facturas.ObtenerPorClienteAsync(clienteId, ct))
        {
            if (!factura.RequiereRecordatorio) continue;

            var anterior = factura.Estado;
            try
            {
                var correo = MensajeRecordatorio.Crear(cliente, factura, config.Value);
                await email.EnviarAsync(cliente.Email, correo, ct);

                var nuevo = factura.EstadoSiguiente;
                await facturas.ActualizarEstadoAsync(factura.Id, nuevo, ct);
                procesadas.Add(new FacturaProcesada(factura.Id, factura.Numero, anterior, nuevo, true, null));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Falló el recordatorio de la factura {Numero}", factura.Numero);
                procesadas.Add(new FacturaProcesada(factura.Id, factura.Numero, anterior, null, false, ex.Message));
            }
        }
        return new ResultadoCliente(cliente.Id, cliente.Nombre, cliente.Email, procesadas);
    }
}