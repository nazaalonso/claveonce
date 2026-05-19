using Claveonce.Models;
using Claveonce.Helpers;

namespace Claveonce.Endpoints
{
    public static class NotificationsEndpoints
    {
        public static void MapNotificationsEndpoints(this WebApplication app)
        {
            var notifications = new List<Notification>();

            // POST SEND
            app.MapPost("/api/notifications/send", (SendNotificationRequest request) =>
            {
                if (request.UsuarioId == Guid.Empty ||
                    string.IsNullOrWhiteSpace(request.Mensaje) ||
                    string.IsNullOrWhiteSpace(request.Tipo))
                {
                    return Results.BadRequest(ErrorResponse.Create(
                        "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                        "Bad Request",
                        400,
                        "La notificación debe tener un usuario válido, un mensaje y un tipo de envío.",
                        "/api/notifications/send",
                        "NTF-002",
                        "Los datos de la notificación son inválidos."
                    ));
                }

                if (request.Tipo != "Email" &&
                    request.Tipo != "Push" &&
                    request.Tipo != "SMS")
                {
                    return Results.BadRequest(ErrorResponse.Create(
                        "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                        "Bad Request",
                        400,
                        "El tipo de notificación informado no es válido.",
                        "/api/notifications/send",
                        "NTF-002",
                        "El tipo de notificación debe ser Email, Push o SMS."
                    ));
                }

                var notification = new Notification();

                notification.Id = Guid.NewGuid();
                notification.UsuarioId = request.UsuarioId;
                notification.Mensaje = request.Mensaje;
                notification.Tipo = request.Tipo;
                notification.Estado = "Enviada";
                notification.FechaEnvio = DateTime.UtcNow;

                notifications.Add(notification);

                return Results.Created("/api/notifications/" + notification.UsuarioId, notification);
            })
            .WithTags("Notifications")
            .WithSummary("Registra y simula envío de notificación")
            .WithDescription("Registra una notificación para un usuario y simula su envío. Si los datos son inválidos, devuelve un error 400 con el código NTF-002.")
            .Accepts<SendNotificationRequest>("application/json")
            .Produces<Notification>(StatusCodes.Status201Created)
            .Produces<object>(StatusCodes.Status400BadRequest)
            .Produces<object>(StatusCodes.Status404NotFound)
            .Produces<object>(StatusCodes.Status500InternalServerError);

            // GET BY USER
            app.MapGet("/api/notifications/{userId}", (Guid userId) =>
            {
                var userNotifications = notifications
                    .Where(n => n.UsuarioId == userId)
                    .ToList();

                if (userNotifications.Count == 0)
                {
                    return Results.NotFound(ErrorResponse.Create(
                        "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                        "Not Found",
                        404,
                        "No se encontraron notificaciones registradas para el usuario indicado.",
                        "/api/notifications/" + userId,
                        "NTF-003",
                        "El usuario no tiene notificaciones registradas."
                    ));
                }

                return Results.Ok(userNotifications);
            })
            .WithTags("Notifications")
            .WithSummary("Lista notificaciones de un usuario")
            .WithDescription("Devuelve todas las notificaciones registradas para un usuario. Si el usuario no tiene notificaciones, devuelve un error 404 con el código NTF-003.")
            .Produces<List<Notification>>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status404NotFound)
            .Produces<object>(StatusCodes.Status500InternalServerError);
        }
    }
}