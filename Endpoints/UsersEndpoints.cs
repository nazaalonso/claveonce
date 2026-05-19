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
                    return Results.BadRequest(ErrorResponse.Create(
                        "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                        "Bad Request",
                        400,
                        "La solicitud contiene datos inválidos.",
                        "/api/users/register",
                        "USR-002",
                        "Los datos del usuario son inválidos."
                    ));
                }

                var existingUser = users.FirstOrDefault(u => u.Email.ToLower() == request.Email.ToLower());

                if (existingUser != null)
                {
                    return Results.Conflict(ErrorResponse.Create(
                        "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                        "Conflict",
                        409,
                        "Ya existe un usuario con esos datos.",
                        "/api/users/register",
                        "USR-001",
                        "El email '" + request.Email + "' ya está registrado."
                    ));
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
            .WithDescription("Registra un nuevo usuario en ClaveOnce. Si los datos enviados son inválidos, devuelve un error 400 con el código USR-002. Si el email ya está registrado, devuelve un error 409 con el código USR-001.")
            .Accepts<RegisterUserRequest>("application/json")
            .Produces<UserResponse>(StatusCodes.Status201Created)
            .Produces<object>(StatusCodes.Status400BadRequest)
            .Produces<object>(StatusCodes.Status409Conflict)
            .Produces<object>(StatusCodes.Status500InternalServerError);

            app.MapPost("/api/users/login", (LoginUserRequest request) =>
            {
                if (string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.Password))
                {
                    return Results.BadRequest(ErrorResponse.Create(
                        "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                        "Bad Request",
                        400,
                        "La solicitud contiene datos inválidos.",
                        "/api/users/login",
                        "USR-002",
                        "Los datos del usuario son inválidos."
                    ));
                }

                var user = users.FirstOrDefault(u => u.Email.ToLower() == request.Email.ToLower());

                if (user == null)
                {
                    return Results.Json(ErrorResponse.Create(
                        "https://tools.ietf.org/html/rfc7235#section-3.1",
                        "Unauthorized",
                        401,
                        "Las credenciales no son válidas.",
                        "/api/users/login",
                        "USR-003",
                        "Credenciales incorrectas."
                    ), statusCode: 401);
                }

                if (user.Activo == false)
                {
                    return Results.Json(ErrorResponse.Create(
                        "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                        "Forbidden",
                        403,
                        "El acceso está prohibido.",
                        "/api/users/login",
                        "USR-004",
                        "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte."
                    ), statusCode: 403);
                }

                if (user.PasswordHash != request.Password)
                {
                    user.IntentosFallidos++;

                    if (user.IntentosFallidos >= 3)
                    {
                        user.Activo = false;

                        return Results.Json(ErrorResponse.Create(
                            "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                            "Forbidden",
                            403,
                            "El acceso está prohibido.",
                            "/api/users/login",
                            "USR-004",
                            "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte."
                        ), statusCode: 403);
                    }

                    return Results.Json(ErrorResponse.Create(
                        "https://tools.ietf.org/html/rfc7235#section-3.1",
                        "Unauthorized",
                        401,
                        "Las credenciales no son válidas.",
                        "/api/users/login",
                        "USR-003",
                        "Credenciales incorrectas."
                    ), statusCode: 401);
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
            .WithDescription("Autentica un usuario mediante email y contraseña. Si las credenciales son incorrectas, devuelve un error 401 con el código USR-003. Si el usuario está bloqueado por intentos fallidos, devuelve un error 403 con el código USR-004.")
            .Accepts<LoginUserRequest>("application/json")
            .Produces<LoginUserResponse>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest)
            .Produces<object>(StatusCodes.Status401Unauthorized)
            .Produces<object>(StatusCodes.Status403Forbidden)
            .Produces<object>(StatusCodes.Status500InternalServerError);
        }
    }
}