namespace Claveonce.Models
{
    public class Cart
    {
        public Guid UsuarioId { get; set; }

        public List<CartItem> Items { get; set; } = new List<CartItem>();

        public DateTime FechaActualizacion { get; set; }
    }

    public class CartItem
    {
        public Guid ProductoId { get; set; }

        public int Cantidad { get; set; }
    }

    public class AddCartItemRequest
    {
        public Guid ProductoId { get; set; }

        public int Cantidad { get; set; }
    }

    public class UpdateCartItemRequest
    {
        public int Cantidad { get; set; }
    }
}