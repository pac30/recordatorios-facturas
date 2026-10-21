namespace Recordatorios.Domain;

public class Cliente
{
    public required string Id { get; init; }
    public required string Nombre { get; init; }
    public required string Email { get; init; }
}