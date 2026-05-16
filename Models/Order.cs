namespace Claveonce.Models
{
    public class Order
    {
        public Guid Id { get; set; }

        public Guid UsuarioId { get; set; }

        public List<OrderItem> Items { get; set; } = new List<OrderItem>();

        public decimal Total { get; set; }

        public string Estado { get; set; } = "Pendiente";

        public DateTime FechaCreacion { get; set; }

        public DateTime? FechaActualizacion { get; set; }
    }

    public class OrderItem
    {
        public Guid ProductoId { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }
    }

    public class CreateOrderRequest
    {
        public Guid UsuarioId { get; set; }

        public List<CreateOrderItemRequest> Items { get; set; } = new List<CreateOrderItemRequest>();
    }

    public class CreateOrderItemRequest
    {
        public Guid ProductoId { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }
    }

    public class UpdateOrderStatusRequest
    {
        public string Estado { get; set; } = string.Empty;
    }

    public class UpdateOrderStatusResponse
    {
        public Guid Id { get; set; }

        public string Estado { get; set; } = string.Empty;

        public DateTime FechaActualizacion { get; set; }
    }
}