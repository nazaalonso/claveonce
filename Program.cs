using Claveonce.Endpoints;
using MiniApi.Exceptions;
using Serilog;
using System.Reflection;
using Serilog.Formatting.Json;


var builder = WebApplication.CreateBuilder(args);


// Configuración de Serilog
builder.Host.UseSerilog((context, configuration) => configuration
    .WriteTo.Console() // Log en consola
    .WriteTo.File(new JsonFormatter(), "logs/log.json", rollingInterval: RollingInterval.Day) // Log en archivo JSON
    .Enrich.FromLogContext());


builder.Services.AddExceptionHandler<MiniApi.Exceptions.GlobalExceptionHandler>();   //addExceptionHandler le dice a .Net que delegue el manejo del error a mi clase personalizada

builder.Services.AddProblemDetails(); //es el estandar de .Net para manejar errores de forma estructurada


builder.Services.AddHealthChecks(); //Health Checks - Middleware


// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

app.UseExceptionHandler(); // es el middleware que intercepta errores antes de mandarselos al cliente

// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Endpoints
app.MapProductsEndpoints();
app.MapUsersEndpoints();
app.MapCartEndpoints();
app.MapOrdersEndpoints();
app.MapNotificationsEndpoints();

// Health Checks
app.MapGet("/health", () =>
{
    return Results.Ok(new
    {
        status = "Healthy",
        service = "ClaveOnce API",
        message = "La API de ClaveOnce est� funcionando correctamente",
        timestamp = DateTime.UtcNow
    });
})
.WithTags("Health")
.WithSummary("Verifica el estado general de la API")
.WithDescription("Devuelve el estado general de la API para comprobar que se encuentra operativa.");

app.MapGet("/health/ready", () =>
{
    return Results.Ok(new
    {
        status = "Healthy",
        service = "ClaveOnce API",
        message = "La API est� lista para recibir solicitudes",
        timestamp = DateTime.UtcNow
    });
})
.WithTags("Health")
.WithSummary("Verifica si la API est� lista")
.WithDescription("Devuelve el estado de preparaci�n de la API para indicar si puede recibir solicitudes.");

app.MapGet("/health/live", () =>
{
    return Results.Ok(new
    {
        status = "Healthy",
        service = "ClaveOnce API",
        message = "La API est� activa",
        timestamp = DateTime.UtcNow
    });
})
.WithTags("Health")
.WithSummary("Verifica si la API est� activa")
.WithDescription("Devuelve el estado de vida de la API para indicar si la aplicaci�n sigue ejecut�ndose.");

app.UseExceptionHandler(); //manejo de erores

app.MapHealthChecks("/health");         //definición de rutas del error
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");

app.UseMiddleware<MiniApi.Middleware.CorrelationIdMiddleware>(); //Correlación de ID - Middleware


app.Run();