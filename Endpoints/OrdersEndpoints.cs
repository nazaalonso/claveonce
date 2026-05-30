using Claveonce.Models;
using Claveonce.Helpers;
using Claveonce.Repositories;

namespace Claveonce.Endpoints
{
    public static class OrdersEndpoints
    {
        public static void MapOrdersEndpoints(this WebApplication app)
        {
            // GET ALL
            app.MapGet("/api/orders", (Guid? usuarioId, OrderRepository orderRepository) =>
            {
                var ordersFiltered = orderRepository.GetAll(usuarioId);

                return Results.Ok(ordersFiltered);
            })
            .WithTags("Orders")
            .WithSummary("Lista órdenes")
            .WithDescription("Lista las órdenes generadas. Permite filtrar las órdenes por usuarioId.")
            .Produces<IEnumerable<Order>>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status500InternalServerError);

            // GET BY ID
            app.MapGet("/api/orders/{id}", (Guid id, OrderRepository orderRepository) =>
            {
                var order = orderRepository.GetById(id);

                if (order == null)
                {
                    return Results.NotFound(ErrorResponse.Create(
                        "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                        "Not Found",
                        404,
                        "No se encontró una orden con el identificador indicado.",
                        "/api/orders/" + id,
                        "ORD-001",
                        "Orden no encontrada."
                    ));
                }

                return Results.Ok(order);
            })
            .WithTags("Orders")
            .WithSummary("Obtiene detalle de una orden")
            .WithDescription("Obtiene una orden específica utilizando su identificador. Si la orden no existe, devuelve un error 404 con el código ORD-001.")
            .Produces<Order>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status404NotFound)
            .Produces<object>(StatusCodes.Status500InternalServerError);

            // POST
            app.MapPost("/api/orders", (CreateOrderRequest request, OrderRepository orderRepository) =>
            {
                if (request.UsuarioId == Guid.Empty || request.Items == null || request.Items.Count == 0)
                {
                    return Results.BadRequest(ErrorResponse.Create(
                        "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                        "Bad Request",
                        400,
                        "La orden debe tener un usuario válido y al menos un producto.",
                        "/api/orders",
                        "ORD-002",
                        "Los datos de la orden son inválidos."
                    ));
                }

                foreach (var itemRequest in request.Items)
                {
                    if (itemRequest.ProductoId == Guid.Empty ||
                        itemRequest.Cantidad <= 0 ||
                        itemRequest.PrecioUnitario <= 0)
                    {
                        return Results.BadRequest(ErrorResponse.Create(
                            "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                            "Bad Request",
                            400,
                            "Cada item de la orden debe tener productoId válido, cantidad mayor a cero y precio unitario mayor a cero.",
                            "/api/orders",
                            "ORD-002",
                            "Los datos de la orden son inválidos."
                        ));
                    }
                }

                var order = new Order();

                order.Id = Guid.NewGuid();
                order.UsuarioId = request.UsuarioId;
                order.Estado = "Pendiente";
                order.FechaCreacion = DateTime.UtcNow;

                foreach (var itemRequest in request.Items)
                {
                    var orderItem = new OrderItem();

                    orderItem.ProductoId = itemRequest.ProductoId;
                    orderItem.Cantidad = itemRequest.Cantidad;
                    orderItem.PrecioUnitario = itemRequest.PrecioUnitario;

                    order.Items.Add(orderItem);
                }

                order.Total = order.Items.Sum(i => i.Cantidad * i.PrecioUnitario);

                orderRepository.Create(order);

                return Results.Created("/api/orders/" + order.Id, order);
            })
            .WithTags("Orders")
            .WithSummary("Crea nueva orden")
            .WithDescription("Crea una nueva orden con usuarioId e items. Calcula el total según cantidad y precio unitario. Si los datos son inválidos, devuelve un error 400 con el código ORD-002.")
            .Accepts<CreateOrderRequest>("application/json")
            .Produces<Order>(StatusCodes.Status201Created)
            .Produces<object>(StatusCodes.Status400BadRequest)
            .Produces<object>(StatusCodes.Status404NotFound)
            .Produces<object>(StatusCodes.Status409Conflict)
            .Produces<object>(StatusCodes.Status422UnprocessableEntity)
            .Produces<object>(StatusCodes.Status500InternalServerError);

            // PUT STATUS
            app.MapPut("/api/orders/{id}/status", (Guid id, UpdateOrderStatusRequest request, OrderRepository orderRepository) =>
            {
                var order = orderRepository.GetById(id);

                if (order == null)
                {
                    return Results.NotFound(ErrorResponse.Create(
                        "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                        "Not Found",
                        404,
                        "No se encontró una orden con el identificador indicado.",
                        "/api/orders/" + id + "/status",
                        "ORD-001",
                        "Orden no encontrada."
                    ));
                }

                if (string.IsNullOrWhiteSpace(request.Estado))
                {
                    return Results.BadRequest(ErrorResponse.Create(
                        "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                        "Bad Request",
                        400,
                        "Debe informar el nuevo estado de la orden.",
                        "/api/orders/" + id + "/status",
                        "ORD-002",
                        "Los datos de la orden son inválidos."
                    ));
                }

                if (request.Estado != "Pendiente" &&
                    request.Estado != "Confirmada" &&
                    request.Estado != "Enviada" &&
                    request.Estado != "Entregada" &&
                    request.Estado != "Cancelada")
                {
                    return Results.Conflict(ErrorResponse.Create(
                        "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                        "Conflict",
                        409,
                        "El estado informado no es válido para una orden.",
                        "/api/orders/" + id + "/status",
                        "ORD-006",
                        "Los estados permitidos son Pendiente, Confirmada, Enviada, Entregada o Cancelada."
                    ));
                }

                if (order.Estado == "Entregada" && request.Estado == "Pendiente")
                {
                    return Results.Conflict(ErrorResponse.Create(
                        "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                        "Conflict",
                        409,
                        "La transición de estado solicitada no está permitida.",
                        "/api/orders/" + id + "/status",
                        "ORD-006",
                        "Una orden en estado 'Entregada' no puede volver a 'Pendiente'."
                    ));
                }

                order.Estado = request.Estado;
                order.FechaActualizacion = DateTime.UtcNow;

                orderRepository.UpdateStatus(order);

                var response = new UpdateOrderStatusResponse();

                response.Id = order.Id;
                response.Estado = order.Estado;
                response.FechaActualizacion = order.FechaActualizacion.Value;

                return Results.Ok(response);
            })
            .WithTags("Orders")
            .WithSummary("Actualiza estado de la orden")
            .WithDescription("Actualiza el estado de una orden. Los estados permitidos son Pendiente, Confirmada, Enviada, Entregada y Cancelada. Si la orden no existe, devuelve ORD-001. Si el estado no es válido, devuelve ORD-006.")
            .Accepts<UpdateOrderStatusRequest>("application/json")
            .Produces<UpdateOrderStatusResponse>(StatusCodes.Status200OK)
            .Produces<object>(StatusCodes.Status400BadRequest)
            .Produces<object>(StatusCodes.Status404NotFound)
            .Produces<object>(StatusCodes.Status409Conflict)
            .Produces<object>(StatusCodes.Status500InternalServerError);
        }
    }
}