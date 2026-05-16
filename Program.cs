using Claveonce.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
        message = "La API de ClaveOnce está funcionando correctamente",
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
        message = "La API está lista para recibir solicitudes",
        timestamp = DateTime.UtcNow
    });
})
.WithTags("Health")
.WithSummary("Verifica si la API está lista")
.WithDescription("Devuelve el estado de preparación de la API para indicar si puede recibir solicitudes.");

app.MapGet("/health/live", () =>
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

app.Run();