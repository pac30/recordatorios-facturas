namespace Recordatorios.Api;

public class DemoSettings
{
    public const string Section = "Demo";

    /// <summary>Correo base de los clientes demo: "yo@x.com" genera "yo+ana@x.com", "yo+luis@x.com"...</summary>
    public string EmailBase { get; set; } = "demo@ejemplo.com";
}