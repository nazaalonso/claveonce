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
                    return Results.BadRequest(new
                    {
                        type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                        title = "Bad Request",
                        status = 400,
                        detail = "La solicitud contiene datos inválidos.",
                        instance = "/api/notifications/send",
                        errorCode = "NTF-002",
                        errorMessage = "Los datos de la notificación son inválidos."
                    });
                }

                if (request.Tipo != "Email" &&
                    request.Tipo != "Push" &&
                    request.Tipo != "SMS")
                {
                    return Results.BadRequest(new
                    {
                        type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                        title = "Bad Request",
                        status = 400,
                        detail = "La solicitud contiene datos inválidos.",
                        instance = "/api/notifications/send",
                        errorCode = "NTF-002",
                        errorMessage = "Los datos de la notificación son inválidos. El tipo debe ser Email, Push o SMS."
                    });
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
            .WithDescription("Registra una notificación para un usuario y simula su envío.");

            // GET BY USER
            app.MapGet("/api/notifications/{userId}", (Guid userId) =>
            {
                var userNotifications = notifications
                    .Where(n => n.UsuarioId == userId)
                    .ToList();

                if (userNotifications.Count == 0)
                {
                    return Results.NotFound(new
                    {
                        type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                        title = "Not Found",
                        status = 404,
                        detail = "El recurso solicitado no fue encontrado.",
                        instance = "/api/notifications/" + userId,
                        errorCode = "NTF-003",
                        errorMessage = "No se encontraron notificaciones para el usuario."
                    });
                }

                return Results.Ok(userNotifications);
            })
            .WithTags("Notifications")
            .WithSummary("Lista notificaciones de un usuario")
            .WithDescription("Devuelve todas las notificaciones registradas para un usuario.");
        }
    }
}