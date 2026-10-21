using FluentAssertions;
using Recordatorios.Domain;

namespace Recordatorios.Tests;

public class EstadoFacturaTests
{
    [Theory]
    [InlineData(EstadoFactura.PrimerRecordatorio, EstadoFactura.SegundoRecordatorio)]
    [InlineData(EstadoFactura.SegundoRecordatorio, EstadoFactura.Desactivado)]
    public void Siguiente_avanza_un_paso(string actual, string esperado) =>
        EstadoFactura.Siguiente(actual).Should().Be(esperado);

    [Theory]
    [InlineData(EstadoFactura.Desactivado)]
    [InlineData("pagada")]
    public void Siguiente_lanza_si_el_estado_no_admite_transicion(string estado)
    {
        var act = () => EstadoFactura.Siguiente(estado);
        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(EstadoFactura.PrimerRecordatorio, true)]
    [InlineData(EstadoFactura.SegundoRecordatorio, true)]
    [InlineData(EstadoFactura.Desactivado, false)]
    [InlineData("pagada", false)]
    [InlineData(null, false)]
    public void RequiereRecordatorio_solo_en_estados_de_recordatorio(string? estado, bool esperado) =>
        EstadoFactura.RequiereRecordatorio(estado).Should().Be(esperado);
}