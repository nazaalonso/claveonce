using Claveonce.Models;
using Claveonce.Helpers;

namespace Claveonce.Endpoints
{
    public static class UsersEndpoints
    {
        public static void MapUsersEndpoints(this WebApplication app)
        {
            var users = new List<User>();

            app.MapPost("/api/users/register", (RegisterUserRequest request) =>
            {
                if (string.IsNullOrWhiteSpace(request.Nombre) ||
                    string.IsNullOrWhiteSpace(request.Apellido) ||
                    string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.Password))
                {
                    return Results.BadRequest(new
                    {
                        type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                        title = "Bad Request",
                        status = 400,
                        detail = "La solicitud contiene datos inválidos.",
                        instance = "/api/users/register",
                        errorCode = "USR-002",
                        errorMessage = "Los datos del usuario son inválidos."
                    });
                }

                var existingUser = users.FirstOrDefault(u => u.Email.ToLower() == request.Email.ToLower());

                if (existingUser != null)
                {
                    return Results.Conflict(new
                    {
                        type = "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                        title = "Conflict",
                        status = 409,
                        detail = "Ya existe un recurso con esos datos.",
                        instance = "/api/users/register",
                        errorCode = "USR-001",
                        errorMessage = "El email '" + request.Email + "' ya está registrado."
                    });
                }

                var user = new User();

                user.Id = Guid.NewGuid();
                user.Nombre = request.Nombre;
                user.Apellido = request.Apellido;
                user.Email = request.Email;
                user.PasswordHash = request.Password;
                user.FechaRegistro = DateTime.UtcNow;
                user.Activo = true;
                user.IntentosFallidos = 0;

                users.Add(user);

                var response = new UserResponse();

                response.Id = user.Id;
                response.Nombre = user.Nombre;
                response.Apellido = user.Apellido;
                response.Email = user.Email;
                response.FechaRegistro = user.FechaRegistro;
                response.Activo = user.Activo;

                return Results.Created("/api/users/register", response);
            })
            .WithTags("Users")
            .WithSummary("Registra nuevo usuario")
            .WithDescription("Registra un nuevo usuario con nombre, apellido, email y password.");

            app.MapPost("/api/users/login", (LoginUserRequest request) =>
            {
                if (string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.Password))
                {
                    return Results.BadRequest(new
                    {
                        type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                        title = "Bad Request",
                        status = 400,
                        detail = "La solicitud contiene datos inválidos.",
                        instance = "/api/users/login",
                        errorCode = "USR-002",
                        errorMessage = "Los datos del usuario son inválidos."
                    });
                }

                var user = users.FirstOrDefault(u => u.Email.ToLower() == request.Email.ToLower());

                if (user == null)
                {
                    return Results.Unauthorized();
                }

                if (user.Activo == false)
                {
                    return Results.Json(new
                    {
                        type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                        title = "Forbidden",
                        status = 403,
                        detail = "El acceso está prohibido.",
                        instance = "/api/users/login",
                        errorCode = "USR-004",
                        errorMessage = "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte."
                    }, statusCode: 403);
                }

                if (user.PasswordHash != request.Password)
                {
                    user.IntentosFallidos++;

                    if (user.IntentosFallidos >= 3)
                    {
                        user.Activo = false;

                        return Results.Json(new
                        {
                            type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                            title = "Forbidden",
                            status = 403,
                            detail = "El acceso está prohibido.",
                            instance = "/api/users/login",
                            errorCode = "USR-004",
                            errorMessage = "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte."
                        }, statusCode: 403);
                    }

                    return Results.Json(new
                    {
                        type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                        title = "Unauthorized",
                        status = 401,
                        detail = "Las credenciales no son válidas.",
                        instance = "/api/users/login",
                        errorCode = "USR-003",
                        errorMessage = "Credenciales incorrectas."
                    }, statusCode: 401);
                }

                user.IntentosFallidos = 0;

                var response = new LoginUserResponse();

                response.Id = user.Id;
                response.Nombre = user.Nombre;
                response.Apellido = user.Apellido;
                response.Email = user.Email;

                return Results.Ok(response);
            })
            .WithTags("Users")
            .WithSummary("Autentica usuario")
            .WithDescription("Autentica un usuario mediante email y password.");
        }
    }
}