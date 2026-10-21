using System.Globalization;
using Recordatorios.Application.Abstractions;
using Recordatorios.Domain;

namespace Recordatorios.Application.Recordatorios;

/// <summary>Construye el correo según el estado que tenía la factura antes de procesarla.</summary>
public static class MensajeRecordatorio
{
    private static readonly CultureInfo Co = CultureInfo.GetCultureInfo("es-CO");

    public static Correo Crear(Cliente cliente, Factura factura, ConfiguracionCorreo config)
    {
        var (asunto, titulo, aviso, color) = factura.Estado switch
        {
            EstadoFactura.PrimerRecordatorio => (
                $"Segundo recordatorio de pago - Factura {factura.Numero}",
                "Segundo recordatorio de pago",
                "Tu factura ha pasado a <b>segundo recordatorio</b>. Te pedimos realizar el pago lo antes posible " +
                "para evitar que sea desactivada.",
                "#f97316"),
            EstadoFactura.SegundoRecordatorio => (
                $"Aviso de desactivación - Factura {factura.Numero}",
                "Aviso de desactivación",
                "Tu factura <b>va a ser desactivada</b> por falta de pago. Si deseas conservarla activa, " +
                "comunícate con nosotros cuanto antes.",
                "#ef4444"),
            _ => throw new InvalidOperationException($"Estado '{factura.Estado}' sin mensaje definido.")
        };

        var monto = factura.Monto.ToString("C0", Co);
        var vence = factura.FechaVencimiento.ToString("dd/MM/yyyy", Co);
        var dias = (DateTime.UtcNow.Date - factura.FechaVencimiento.Date).Days;
        var estadoVencimiento = dias > 0 ? $"vencida hace {dias} día(s)" : $"vence en {-dias} día(s)";
        var avisoTexto = aviso.Replace("<b>", "").Replace("</b>", "");
        var contacto = config.Contacto.Trim();

        var texto = $"Hola {cliente.Nombre},\n\n{avisoTexto}\n\n" +
                    $"Factura: {factura.Numero}\nMonto: {monto}\nVencimiento: {vence} ({estadoVencimiento})\n\n" +
                    (contacto.Length > 0 ? $"Si ya realizaste el pago o tienes dudas: {contacto}\n\n" : "Si ya realizaste el pago, ignora este mensaje.\n\n") +
                    $"Cordialmente,\n{config.Empresa}\n";

        var html = Html(Esc(config.Empresa), Esc(titulo), color,
            Esc(cliente.Nombre), aviso, Esc(factura.Numero),
            Esc(monto), Esc($"{vence} · {estadoVencimiento}"),
            Esc(contacto));

        return new Correo(asunto, texto, html);
    }

    /// <summary>Escapa solo los caracteres que pueden romper o inyectar HTML; deja las tildes legibles.</summary>
    private static string Esc(string s) => s
        .Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&#39;");

    private static string Html(string empresa, string titulo, string color, string nombre, string aviso,
        string numero, string monto, string vencimiento, string contacto)
    {
        var pie = contacto.Length > 0
            ? $"Si ya realizaste el pago o tienes dudas, escríbenos: <b>{contacto}</b>"
            : "Si ya realizaste el pago, ignora este mensaje.";

        return $$"""
        <!doctype html>
        <html lang="es"><body style="margin:0;padding:24px;background:#f4f6fb;font-family:Segoe UI,Arial,sans-serif;color:#1b2333;">
          <table role="presentation" width="100%" cellpadding="0" cellspacing="0"><tr><td align="center">
            <table role="presentation" width="560" cellpadding="0" cellspacing="0" style="max-width:560px;background:#ffffff;border-radius:12px;overflow:hidden;border:1px solid #e6e9f0;">
              <tr><td style="background:{{color}};padding:22px 28px;color:#ffffff;">
                <div style="font-size:13px;letter-spacing:.06em;text-transform:uppercase;opacity:.9;">{{empresa}}</div>
                <div style="font-size:22px;font-weight:700;margin-top:4px;">{{titulo}}</div>
              </td></tr>
              <tr><td style="padding:28px;">
                <p style="margin:0 0 14px;font-size:16px;">Hola <b>{{nombre}}</b>,</p>
                <p style="margin:0 0 20px;font-size:15px;line-height:1.55;">{{aviso}}</p>
                <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background:#f8faff;border:1px solid #e6e9f0;border-radius:10px;">
                  <tr><td style="padding:12px 16px;color:#6b7488;font-size:13px;">Factura</td><td align="right" style="padding:12px 16px;font-weight:700;">{{numero}}</td></tr>
                  <tr><td style="padding:12px 16px;color:#6b7488;font-size:13px;border-top:1px solid #e6e9f0;">Monto</td><td align="right" style="padding:12px 16px;font-weight:700;border-top:1px solid #e6e9f0;">{{monto}}</td></tr>
                  <tr><td style="padding:12px 16px;color:#6b7488;font-size:13px;border-top:1px solid #e6e9f0;">Vencimiento</td><td align="right" style="padding:12px 16px;border-top:1px solid #e6e9f0;">{{vencimiento}}</td></tr>
                </table>
                <p style="margin:22px 0 0;font-size:13px;color:#6b7488;line-height:1.5;">{{pie}}</p>
              </td></tr>
              <tr><td style="padding:16px 28px;background:#fafbfd;border-top:1px solid #e6e9f0;font-size:12px;color:#8a93a6;">
                Cordialmente, {{empresa}}
              </td></tr>
            </table>
          </td></tr></table>
        </body></html>
        """;
    }
}