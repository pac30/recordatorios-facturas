using FluentAssertions;
using Recordatorios.Application.Recordatorios;
using Recordatorios.Domain;

namespace Recordatorios.Tests;

public class MensajeRecordatorioTests
{
    private static readonly ConfiguracionCorreo Config = new() { Empresa = "Acme SAS", Contacto = "cobros@acme.com" };

    private static Cliente Cliente(string nombre = "Ana") => new() { Id = "c1", Nombre = nombre, Email = "ana@test.com" };

    private static Factura Factura(string estado) => new()
    {
        Id = "1", ClienteId = "c1", Numero = "F-1001", Monto = 1200000m,
        FechaVencimiento = DateTime.UtcNow.Date.AddDays(-20), Estado = estado
    };

    [Fact]
    public void Primer_recordatorio_informa_el_paso_a_segundo()
    {
        var correo = MensajeRecordatorio.Crear(Cliente(), Factura(EstadoFactura.PrimerRecordatorio), Config);

        correo.Asunto.Should().Be("Segundo recordatorio de pago - Factura F-1001");
        correo.Texto.Should().Contain("segundo recordatorio").And.Contain("Acme SAS").And.Contain("cobros@acme.com");
        correo.Html.Should().Contain("Acme SAS").And.Contain("F-1001").And.Contain("1.200.000").And.Contain("hace 20 día(s)");
    }

    [Fact]
    public void Segundo_recordatorio_avisa_la_desactivacion()
    {
        var correo = MensajeRecordatorio.Crear(Cliente(), Factura(EstadoFactura.SegundoRecordatorio), Config);

        correo.Asunto.Should().Be("Aviso de desactivación - Factura F-1001");
        correo.Texto.Should().Contain("desactivada");
        correo.Html.Should().Contain("#ef4444");
    }

    [Fact]
    public void El_html_escapa_el_contenido_del_cliente()
    {
        var correo = MensajeRecordatorio.Crear(Cliente("<script>x</script>"), Factura(EstadoFactura.PrimerRecordatorio), Config);

        correo.Html.Should().NotContain("<script>").And.Contain("&lt;script&gt;");
    }

    [Fact]
    public void Sin_contacto_no_muestra_correo_de_contacto()
    {
        var correo = MensajeRecordatorio.Crear(Cliente(), Factura(EstadoFactura.PrimerRecordatorio),
            new ConfiguracionCorreo { Empresa = "Acme SAS" });

        correo.Texto.Should().Contain("ignora este mensaje").And.NotContain("cobros@");
    }

    [Fact]
    public void Estado_sin_mensaje_lanza()
    {
        var act = () => MensajeRecordatorio.Crear(Cliente(), Factura(EstadoFactura.Desactivado), Config);
        act.Should().Throw<InvalidOperationException>();
    }
}