using Claveonce.Endpoints;
using Claveonce.Data;
using Claveonce.Repositories;
using MiniApi.Exceptions;
using Serilog;
using System.Reflection;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);

// Configuración de Serilog
builder.Host.UseSerilog((context, configuration) => configuration
    .WriteTo.Console()
    .WriteTo.File(new JsonFormatter(), "logs/log.json", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext());

builder.Services.AddExceptionHandler<MiniApi.Exceptions.GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks();

// Base de datos
builder.Services.AddSingleton<DatabaseInitializer>();

// Repositories
builder.Services.AddScoped<UserRepository>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// Inicialización de la base de datos
using (var scope = app.Services.CreateScope())
{
    var databaseInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    databaseInitializer.Initialize();
}

app.UseExceptionHandler();

// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Middleware de correlación
app.UseMiddleware<MiniApi.Middleware.CorrelationIdMiddleware>();

// Endpoints
app.MapProductsEndpoints();
app.MapUsersEndpoints();
app.MapCartEndpoints();
app.MapOrdersEndpoints();
app.MapNotificationsEndpoints();

// Health Checks personalizados
app.MapGet("/health/status", () =>
{
    return Results.Ok(new
    {
        status = "Healthy",
        service = "ClaveOnce API",
        message = "La API de ClaveOnce está funcionando correctamente",
        timestamp = DateTime.UtcNow
    });
})
.WithTags("Health")
.WithSummary("Verifica el estado general de la API")
.WithDescription("Devuelve el estado general de la API para comprobar que se encuentra operativa.");

app.MapGet("/health/ready-info", () =>
{
    return Results.Ok(new
    {
        status = "Healthy",
        service = "ClaveOnce API",
        message = "La API está lista para recibir solicitudes",
        timestamp = DateTime.UtcNow
    });
})
.WithTags("Health")
.WithSummary("Verifica si la API está lista")
.WithDescription("Devuelve el estado de preparación de la API para indicar si puede recibir solicitudes.");

app.MapGet("/health/live-info", () =>
{
    return Results.Ok(new
    {
        status = "Healthy",
        service = "ClaveOnce API",
        message = "La API está activa",
        timestamp = DateTime.UtcNow
    });
})
.WithTags("Health")
.WithSummary("Verifica si la API está activa")
.WithDescription("Devuelve el estado de vida de la API para indicar si la aplicación sigue ejecutándose.");

// Health Checks reales de .NET
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");

app.Run();