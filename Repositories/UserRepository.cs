using Claveonce.Models;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Claveonce.Repositories
{
    public class UserRepository
    {
        private readonly IConfiguration _config;

        public UserRepository(IConfiguration config)
        {
            _config = config;
        }

        private SqliteConnection CreateConnection()
        {
            var connectionString = _config.GetConnectionString("DefaultConnection")
                ?? "Data Source=claveonce.db";

            return new SqliteConnection(connectionString);
        }

        public User? GetByEmail(string email)
        {
            using var connection = CreateConnection();

            var row = connection.QueryFirstOrDefault(@"
                SELECT
                    id,
                    nombre,
                    apellido,
                    email,
                    password_hash,
                    fecha_registro,
                    activo,
                    intentos_fallidos
                FROM users
                WHERE LOWER(email) = LOWER(@Email);
            ", new { Email = email });

            if (row == null)
            {
                return null;
            }

            var user = new User();

            user.Id = Guid.Parse((string)row.id);
            user.Nombre = row.nombre;
            user.Apellido = row.apellido;
            user.Email = row.email;
            user.PasswordHash = row.password_hash;
            user.FechaRegistro = DateTime.Parse((string)row.fecha_registro);
            user.Activo = Convert.ToInt32(row.activo) == 1;
            user.IntentosFallidos = Convert.ToInt32(row.intentos_fallidos);

            return user;
        }

        public void Create(User user)
        {
            using var connection = CreateConnection();

            connection.Execute(@"
                INSERT INTO users (
                    id,
                    nombre,
                    apellido,
                    email,
                    password_hash,
                    fecha_registro,
                    activo,
                    intentos_fallidos
                )
                VALUES (
                    @Id,
                    @Nombre,
                    @Apellido,
                    @Email,
                    @PasswordHash,
                    @FechaRegistro,
                    @Activo,
                    @IntentosFallidos
                );
            ", new
            {
                Id = user.Id.ToString(),
                user.Nombre,
                user.Apellido,
                user.Email,
                user.PasswordHash,
                FechaRegistro = user.FechaRegistro.ToString("o"),
                Activo = user.Activo ? 1 : 0,
                user.IntentosFallidos
            });
        }

        public void UpdateLoginState(User user)
        {
            using var connection = CreateConnection();

            connection.Execute(@"
                UPDATE users
                SET
                    activo = @Activo,
                    intentos_fallidos = @IntentosFallidos
                WHERE id = @Id;
            ", new
            {
                Id = user.Id.ToString(),
                Activo = user.Activo ? 1 : 0,
                user.IntentosFallidos
            });
        }
    }
}