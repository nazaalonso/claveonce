using Claveonce.Endpoints;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<ClaveOnce.Exceptions.GlobalExceptionHandler>();
// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

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

app.UseExceptionHandler();
app.Run();