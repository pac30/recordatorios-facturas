using MailKit.Security;

namespace Recordatorios.Infrastructure.Email;

public class SmtpSettings
{
    public const string Section = "Smtp";
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public string User { get; set; } = "";
    public string Password { get; set; } = "";
    public string From { get; set; } = "";
    public string FromName { get; set; } = "Cobranzas";

    /// <summary>"StartTls" para Gmail; "None" para un SMTP local de pruebas como Mailpit.</summary>
    public SecureSocketOptions Security { get; set; } = SecureSocketOptions.StartTls;
}