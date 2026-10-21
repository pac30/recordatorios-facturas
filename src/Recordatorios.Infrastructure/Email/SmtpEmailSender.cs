using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Recordatorios.Application.Abstractions;

namespace Recordatorios.Infrastructure.Email;

internal class SmtpEmailSender(IOptions<SmtpSettings> options) : IEmailSender
{
    public async Task EnviarAsync(string destinatario, Correo correo, CancellationToken ct = default)
    {
        var s = options.Value;
        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(s.FromName, string.IsNullOrWhiteSpace(s.From) ? s.User : s.From));
        msg.To.Add(MailboxAddress.Parse(destinatario));
        msg.Subject = correo.Asunto;
        msg.Body = new BodyBuilder { TextBody = correo.Texto, HtmlBody = correo.Html }.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(s.Host, s.Port, s.Security, ct);
        if (!string.IsNullOrEmpty(s.User))
            await client.AuthenticateAsync(s.User, s.Password, ct);
        await client.SendAsync(msg, ct);
        await client.DisconnectAsync(true, ct);
    }
}