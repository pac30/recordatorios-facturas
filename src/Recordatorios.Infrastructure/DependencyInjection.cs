using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Recordatorios.Application.Abstractions;
using Recordatorios.Infrastructure.Email;
using Recordatorios.Infrastructure.Mongo;

namespace Recordatorios.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<MongoSettings>(config.GetSection(MongoSettings.Section));
        services.Configure<SmtpSettings>(config.GetSection(SmtpSettings.Section));

        services.AddSingleton<MongoContext>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IFacturaRepository, FacturaRepository>();
        services.AddScoped<IDatosDemo, DatosDemo>();
        services.AddTransient<IEmailSender, SmtpEmailSender>();
        return services;
    }
}