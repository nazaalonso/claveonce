using Claveonce.Models;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Claveonce.Repositories
{
    public class NotificationRepository
    {
        private readonly IConfiguration _config;

        public NotificationRepository(IConfiguration config)
        {
            _config = config;
        }

        private SqliteConnection CreateConnection()
        {
            var connectionString = _config.GetConnectionString("DefaultConnection")
                ?? "Data Source=claveonce.db";

            return new SqliteConnection(connectionString);
        }

        public void Create(Notification notification)
        {
            using var connection = CreateConnection();

            connection.Execute(@"
                INSERT INTO notifications (
                    id,
                    usuario_id,
                    mensaje,
                    tipo,
                    estado,
                    fecha_envio
                )
                VALUES (
                    @Id,
                    @UsuarioId,
                    @Mensaje,
                    @Tipo,
                    @Estado,
                    @FechaEnvio
                );
            ", new
            {
                Id = notification.Id.ToString(),
                UsuarioId = notification.UsuarioId.ToString(),
                notification.Mensaje,
                notification.Tipo,
                notification.Estado,
                FechaEnvio = notification.FechaEnvio.ToString("o")
            });
        }

        public List<Notification> GetByUserId(Guid userId)
        {
            using var connection = CreateConnection();

            var rows = connection.Query(@"
                SELECT
                    id,
                    usuario_id,
                    mensaje,
                    tipo,
                    estado,
                    fecha_envio
                FROM notifications
                WHERE usuario_id = @UsuarioId;
            ", new { UsuarioId = userId.ToString() });

            var notifications = new List<Notification>();

            foreach (var row in rows)
            {
                var notification = new Notification();

                notification.Id = Guid.Parse((string)row.id);
                notification.UsuarioId = Guid.Parse((string)row.usuario_id);
                notification.Mensaje = row.mensaje;
                notification.Tipo = row.tipo;
                notification.Estado = row.estado;
                notification.FechaEnvio = DateTime.Parse((string)row.fecha_envio);

                notifications.Add(notification);
            }

            return notifications;
        }
    }
}