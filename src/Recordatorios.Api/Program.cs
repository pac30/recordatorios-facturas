using Recordatorios.Api;
using Recordatorios.Application.Facturas;
using Recordatorios.Application.Recordatorios;
using Recordatorios.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.Configure<ConfiguracionCorreo>(builder.Configuration.GetSection(ConfiguracionCorreo.Section));
builder.Services.Configure<DemoSettings>(builder.Configuration.GetSection(DemoSettings.Section));
builder.Services.AddScoped<ProcesadorRecordatorios>();
builder.Services.AddScoped<ConsultaFacturas>();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:4200"])
    .AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.UseCors();
app.MapControllers();
app.Run();

public partial class Program;