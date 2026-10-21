namespace Recordatorios.Application.Abstractions;

public interface IDatosDemo
{
    Task<(int Clientes, int Facturas)> ReiniciarAsync(string emailBase, CancellationToken ct = default);
}