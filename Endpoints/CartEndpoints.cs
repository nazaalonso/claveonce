using Claveonce.Models;
using Claveonce.Repositories;

namespace Claveonce.Endpoints
{
    public static class CartEndpoints
    {
        public static void MapCartEndpoints(this WebApplication app)
        {
            app.MapGet("/api/cart/{userId}", (Guid userId, CartRepository cartRepository) =>
            {
                var cart = cartRepository.GetByUserId(userId);

                if (cart == null)
                {
                    return Results.NotFound(new
                    {
                        type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                        title = "Not Found",
                        status = 404,
                        detail = "El recurso solicitado no fue encontrado.",
                        instance = "/api/cart/" + userId,
                        errorCode = "CRT-001",
                        errorMessage = "Carrito no encontrado."
                    });
                }

                return Results.Ok(cart);
            })
            .WithTags("Cart")
            .WithSummary("Obtiene carrito del usuario")
            .WithDescription("Obtiene el carrito activo de un usuario. Si el usuario no tiene carrito activo, devuelve un error 404 con el código CRT-001.")
            .Produces<Cart>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status404NotFound)
            .Produces<object>(StatusCodes.Status500InternalServerError);

            app.MapPost("/api/cart/{userId}/items", (Guid userId, AddCartItemRequest request, CartRepository cartRepository) =>
            {
                if (request.Cantidad <= 0)
                {
                    return Results.BadRequest(new
                    {
                        type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                        title = "Bad Request",
                        status = 400,
                        detail = "La solicitud contiene datos inválidos.",
                        instance = "/api/cart/" + userId + "/items",
                        errorCode = "CRT-004",
                        errorMessage = "Cantidad inválida."
                    });
                }

                cartRepository.AddItem(userId, request.ProductoId, request.Cantidad);

                var cart = cartRepository.GetByUserId(userId);

                return Results.Ok(cart);
            })
            .WithTags("Cart")
            .WithSummary("Agrega producto al carrito")
            .WithDescription("Agrega un producto al carrito del usuario. Si la cantidad es inválida, devuelve un error 400 con el código CRT-004.")
            .Accepts<AddCartItemRequest>("application/json")
            .Produces<Cart>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest)
            .Produces<object>(StatusCodes.Status404NotFound)
            .Produces<object>(StatusCodes.Status422UnprocessableEntity)
            .Produces<object>(StatusCodes.Status500InternalServerError);

            app.MapPut("/api/cart/{userId}/items/{productId}", (Guid userId, Guid productId, UpdateCartItemRequest request, CartRepository cartRepository) =>
            {
                if (request.Cantidad <= 0)
                {
                    return Results.BadRequest(new
                    {
                        type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                        title = "Bad Request",
                        status = 400,
                        detail = "La cantidad debe ser mayor a cero.",
                        instance = "/api/cart/" + userId + "/items/" + productId,
                        errorCode = "CRT-004",
                        errorMessage = "La cantidad debe ser mayor a cero."
                    });
                }

                var cart = cartRepository.GetByUserId(userId);

                if (cart == null)
                {
                    return Results.NotFound(new
                    {
                        type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                        title = "Not Found",
                        status = 404,
                        detail = "No se encontró un carrito activo para el usuario indicado.",
                        instance = "/api/cart/" + userId,
                        errorCode = "CRT-001",
                        errorMessage = "Carrito no encontrado."
                    });
                }

                if (!cartRepository.ItemExists(userId, productId))
                {
                    return Results.NotFound(new
                    {
                        type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                        title = "Not Found",
                        status = 404,
                        detail = "El carrito existe, pero no contiene el producto indicado.",
                        instance = "/api/cart/" + userId + "/items/" + productId,
                        errorCode = "CRT-002",
                        errorMessage = "Producto no encontrado."
                    });
                }

                cartRepository.UpdateItem(userId, productId, request.Cantidad);

                cart = cartRepository.GetByUserId(userId);

                return Results.Ok(cart);
            })
            .WithTags("Cart")
            .WithSummary("Actualiza cantidad de un producto")
            .WithDescription("Actualiza la cantidad de un producto dentro del carrito. Si la cantidad es inválida, devuelve un error 400 con el código CRT-004. Si el carrito o el producto no existen, devuelve un error 404.")
            .Accepts<UpdateCartItemRequest>("application/json")
            .Produces<Cart>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest)
            .Produces<object>(StatusCodes.Status404NotFound)
            .Produces<object>(StatusCodes.Status422UnprocessableEntity)
            .Produces<object>(StatusCodes.Status500InternalServerError);

            app.MapDelete("/api/cart/{userId}/items/{productId}", (Guid userId, Guid productId, CartRepository cartRepository) =>
            {
                var cart = cartRepository.GetByUserId(userId);

                if (cart == null)
                {
                    return Results.NotFound(new
                    {
                        type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                        title = "Not Found",
                        status = 404,
                        detail = "No se encontró un carrito activo para el usuario indicado.",
                        instance = "/api/cart/" + userId,
                        errorCode = "CRT-001",
                        errorMessage = "Carrito no encontrado."
                    });
                }

                if (!cartRepository.ItemExists(userId, productId))
                {
                    return Results.NotFound(new
                    {
                        type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                        title = "Not Found",
                        status = 404,
                        detail = "El carrito existe, pero no contiene el producto indicado.",
                        instance = "/api/cart/" + userId + "/items/" + productId,
                        errorCode = "CRT-002",
                        errorMessage = "Producto no encontrado."
                    });
                }

                cartRepository.DeleteItem(userId, productId);

                return Results.NoContent();
            })
            .WithTags("Cart")
            .WithSummary("Quita producto del carrito")
            .WithDescription("Quita un producto específico del carrito del usuario. Si el carrito o el producto no existen, devuelve un error 404.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<object>(StatusCodes.Status404NotFound)
            .Produces<object>(StatusCodes.Status500InternalServerError);

            app.MapDelete("/api/cart/{userId}", (Guid userId, CartRepository cartRepository) =>
            {
                var cart = cartRepository.GetByUserId(userId);

                if (cart == null)
                {
                    return Results.NotFound(new
                    {
                        type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                        title = "Not Found",
                        status = 404,
                        detail = "No se encontró un carrito activo para el usuario indicado.",
                        instance = "/api/cart/" + userId,
                        errorCode = "CRT-001",
                        errorMessage = "Carrito no encontrado."
                    });
                }

                cartRepository.DeleteCart(userId);

                return Results.NoContent();
            })
            .WithTags("Cart")
            .WithSummary("Vacía carrito completo")
            .WithDescription("Elimina el carrito completo de un usuario. Si el carrito no existe, devuelve un error 404 con el código CRT-001.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<object>(StatusCodes.Status404NotFound)
            .Produces<object>(StatusCodes.Status500InternalServerError);
        }
    }
}