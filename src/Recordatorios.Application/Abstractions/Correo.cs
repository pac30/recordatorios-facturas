namespace Recordatorios.Application.Abstractions;

/// <summary>Contenido de un correo: asunto y cuerpo en texto plano y en HTML.</summary>
public record Correo(string Asunto, string Texto, string Html);