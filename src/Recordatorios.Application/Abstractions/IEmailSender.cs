namespace Recordatorios.Application.Abstractions;

public interface IEmailSender
{
    /// <exception cref="Exception">Si el envío falla, para que el estado no se actualice.</exception>
    Task EnviarAsync(string destinatario, Correo correo, CancellationToken ct = default);
}