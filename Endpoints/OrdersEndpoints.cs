using Claveonce.Models;
using Claveonce.Helpers;

namespace Claveonce.Endpoints
{
    public static class OrdersEndpoints
    {
        public static void MapOrdersEndpoints(this WebApplication app)
        {
            var orders = new List<Order>();

            // GET ALL
            app.MapGet("/api/orders", (Guid? usuarioId) =>
            {
                var ordersFiltered = orders.AsEnumerable();

                if (usuarioId != null)
                {
                    ordersFiltered = ordersFiltered.Where(o => o.UsuarioId == usuarioId);
                }

                return Results.Ok(ordersFiltered);
            })
            .WithTags("Orders")
            .WithSummary("Lista órdenes")
            .WithDescription("Lista las órdenes generadas. Permite filtrar por usuarioId.");

            // GET BY ID
            app.MapGet("/api/orders/{id}", (Guid id) =>
            {
                var order = orders.FirstOrDefault(o => o.Id == id);

                if (order == null)
                {
                    return Results.NotFound(new
                    {
                        type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                        title = "Not Found",
                        status = 404,
                        detail = "El recurso solicitado no fue encontrado.",
                        instance = "/api/orders/" + id,
                        errorCode = "ORD-001",
                        errorMessage = "Orden no encontrada."
                    });
                }

                return Results.Ok(order);
            })
            .WithTags("Orders")
            .WithSummary("Obtiene detalle de una orden")
            .WithDescription("Obtiene una orden específica utilizando su identificador.");

            // POST
            app.MapPost("/api/orders", (CreateOrderRequest request) =>
            {
                if (request.UsuarioId == Guid.Empty || request.Items == null || request.Items.Count == 0)
                {
                    return Results.BadRequest(new
                    {
                        type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                        title = "Bad Request",
                        status = 400,
                        detail = "La solicitud contiene datos inválidos.",
                        instance = "/api/orders",
                        errorCode = "ORD-002",
                        errorMessage = "Los datos de la orden son inválidos."
                    });
                }

                foreach (var itemRequest in request.Items)
                {
                    if (itemRequest.ProductoId == Guid.Empty ||
                        itemRequest.Cantidad <= 0 ||
                        itemRequest.PrecioUnitario <= 0)
                    {
                        return Results.BadRequest(new
                        {
                            type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                            title = "Bad Request",
                            status = 400,
                            detail = "La solicitud contiene datos inválidos.",
                            instance = "/api/orders",
                            errorCode = "ORD-002",
                            errorMessage = "Los datos de la orden son inválidos."
                        });
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

                orders.Add(order);

                return Results.Created("/api/orders/" + order.Id, order);
            })
            .WithTags("Orders")
            .WithSummary("Crea nueva orden")
            .WithDescription("Crea una nueva orden con usuarioId e items. Calcula el total según cantidad y precio unitario.");

            // PUT STATUS
            app.MapPut("/api/orders/{id}/status", (Guid id, UpdateOrderStatusRequest request) =>
            {
                var order = orders.FirstOrDefault(o => o.Id == id);

                if (order == null)
                {
                    return Results.NotFound(new
                    {
                        type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                        title = "Not Found",
                        status = 404,
                        detail = "El recurso solicitado no fue encontrado.",
                        instance = "/api/orders/" + id + "/status",
                        errorCode = "ORD-001",
                        errorMessage = "Orden no encontrada."
                    });
                }

                if (string.IsNullOrWhiteSpace(request.Estado))
                {
                    return Results.BadRequest(new
                    {
                        type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                        title = "Bad Request",
                        status = 400,
                        detail = "La solicitud contiene datos inválidos.",
                        instance = "/api/orders/" + id + "/status",
                        errorCode = "ORD-002",
                        errorMessage = "Los datos de la orden son inválidos."
                    });
                }

                if (request.Estado != "Pendiente" &&
                    request.Estado != "Confirmada" &&
                    request.Estado != "Enviada" &&
                    request.Estado != "Entregada" &&
                    request.Estado != "Cancelada")
                {
                    return Results.Conflict(new
                    {
                        type = "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                        title = "Conflict",
                        status = 409,
                        detail = "No se puede modificar el estado.",
                        instance = "/api/orders/" + id + "/status",
                        errorCode = "ORD-006",
                        errorMessage = "El estado de la orden no puede ser modificado."
                    });
                }

                if (order.Estado == "Entregada" && request.Estado == "Pendiente")
                {
                    return Results.Conflict(new
                    {
                        type = "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                        title = "Conflict",
                        status = 409,
                        detail = "No se puede modificar el estado.",
                        instance = "/api/orders/" + id + "/status",
                        errorCode = "ORD-006",
                        errorMessage = "Una orden en estado 'Entregada' no puede volver a 'Pendiente'."
                    });
                }

                order.Estado = request.Estado;
                order.FechaActualizacion = DateTime.UtcNow;

                var response = new UpdateOrderStatusResponse();

                response.Id = order.Id;
                response.Estado = order.Estado;
                response.FechaActualizacion = order.FechaActualizacion.Value;

                return Results.Ok(response);
            })
            .WithTags("Orders")
            .WithSummary("Actualiza estado de la orden")
            .WithDescription("Actualiza el estado de una orden. Los estados permitidos son Pendiente, Confirmada, Enviada, Entregada y Cancelada.");
        }
    }
}