using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Recordatorios.Application.Abstractions;
using Recordatorios.Application.Recordatorios;
using Recordatorios.Domain;

namespace Recordatorios.Tests;

public class ProcesadorRecordatoriosTests
{
    private readonly IClienteRepository _clientes = Substitute.For<IClienteRepository>();
    private readonly IFacturaRepository _facturas = Substitute.For<IFacturaRepository>();
    private readonly IEmailSender _email = Substitute.For<IEmailSender>();
    private readonly Cliente _cliente = new() { Id = "c1", Nombre = "Ana", Email = "ana@test.com" };

    private ProcesadorRecordatorios Crear() =>
        new(_clientes, _facturas, _email, Options.Create(new ConfiguracionCorreo { Empresa = "Acme" }), NullLogger<ProcesadorRecordatorios>.Instance);

    private static Factura Factura(string id, string estado) => new()
    {
        Id = id, ClienteId = "c1", Numero = $"F-{id}", Monto = 100m,
        FechaVencimiento = new DateTime(2026, 11, 1), Estado = estado
    };

    private void DadoFacturas(params Factura[] facturas)
    {
        _clientes.ObtenerPorIdAsync("c1", Arg.Any<CancellationToken>()).Returns(_cliente);
        _facturas.ObtenerPorClienteAsync("c1", Arg.Any<CancellationToken>()).Returns(facturas);
    }

    [Fact]
    public async Task Primer_recordatorio_envia_correo_y_pasa_a_segundo()
    {
        DadoFacturas(Factura("1", EstadoFactura.PrimerRecordatorio));

        var resultado = await Crear().ProcesarClienteAsync("c1");

        await _email.Received(1).EnviarAsync("ana@test.com", Arg.Is<Correo>(c => c.Asunto.Contains("Segundo recordatorio")),
            Arg.Any<CancellationToken>());
        await _facturas.Received(1).ActualizarEstadoAsync("1", EstadoFactura.SegundoRecordatorio, Arg.Any<CancellationToken>());
        resultado.Facturas.Single().Exito.Should().BeTrue();
    }

    [Fact]
    public async Task Segundo_recordatorio_avisa_desactivacion_y_pasa_a_desactivado()
    {
        DadoFacturas(Factura("1", EstadoFactura.SegundoRecordatorio));

        await Crear().ProcesarClienteAsync("c1");

        await _email.Received(1).EnviarAsync("ana@test.com", Arg.Is<Correo>(c => c.Asunto.Contains("desactivación")),
            Arg.Any<CancellationToken>());
        await _facturas.Received(1).ActualizarEstadoAsync("1", EstadoFactura.Desactivado, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Ignora_facturas_que_no_estan_en_recordatorio()
    {
        DadoFacturas(Factura("1", EstadoFactura.Desactivado), Factura("2", "pagada"));

        var resultado = await Crear().ProcesarClienteAsync("c1");

        resultado.Facturas.Should().BeEmpty();
        await _email.DidNotReceiveWithAnyArgs().EnviarAsync(default!, default!);
        await _facturas.DidNotReceiveWithAnyArgs().ActualizarEstadoAsync(default!, default!);
    }

    [Fact]
    public async Task Si_el_correo_falla_no_actualiza_el_estado_y_sigue_con_las_demas()
    {
        DadoFacturas(Factura("1", EstadoFactura.PrimerRecordatorio), Factura("2", EstadoFactura.PrimerRecordatorio));
        _email.EnviarAsync(Arg.Any<string>(), Arg.Is<Correo>(c => c.Asunto.Contains("F-1")), Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("SMTP caído"));

        var resultado = await Crear().ProcesarClienteAsync("c1");

        await _facturas.DidNotReceive().ActualizarEstadoAsync("1", Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _facturas.Received(1).ActualizarEstadoAsync("2", EstadoFactura.SegundoRecordatorio, Arg.Any<CancellationToken>());
        resultado.Facturas.Select(f => f.Exito).Should().Equal(false, true);
        resultado.Facturas[0].Error.Should().Be("SMTP caído");
    }

    [Fact]
    public async Task Cliente_inexistente_lanza_KeyNotFound()
    {
        _clientes.ObtenerPorIdAsync("x", Arg.Any<CancellationToken>()).Returns((Cliente?)null);

        var act = () => Crear().ProcesarClienteAsync("x");

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ProcesarTodos_procesa_cada_cliente()
    {
        var otro = new Cliente { Id = "c2", Nombre = "Luis", Email = "luis@test.com" };
        _clientes.ObtenerTodosAsync(Arg.Any<CancellationToken>()).Returns(new[] { _cliente, otro });
        _clientes.ObtenerPorIdAsync("c1", Arg.Any<CancellationToken>()).Returns(_cliente);
        _clientes.ObtenerPorIdAsync("c2", Arg.Any<CancellationToken>()).Returns(otro);
        _facturas.ObtenerPorClienteAsync("c1", Arg.Any<CancellationToken>()).Returns(new[] { Factura("1", EstadoFactura.PrimerRecordatorio) });
        _facturas.ObtenerPorClienteAsync("c2", Arg.Any<CancellationToken>()).Returns(new[] { Factura("2", EstadoFactura.SegundoRecordatorio) });

        var resultados = await Crear().ProcesarTodosAsync();

        resultados.Should().HaveCount(2);
        await _email.Received(1).EnviarAsync("luis@test.com", Arg.Any<Correo>(), Arg.Any<CancellationToken>());
    }
}