namespace Recordatorios.Application.Recordatorios;

/// <summary>Datos de la empresa que se muestran en los correos (sección "Correo" de appsettings).</summary>
public class ConfiguracionCorreo
{
    public const string Section = "Correo";

    public string Empresa { get; set; } = "Cobranzas";

    /// <summary>Texto libre de contacto (correo, teléfono, WhatsApp...). Si está vacío no se muestra.</summary>
    public string Contacto { get; set; } = "";
}