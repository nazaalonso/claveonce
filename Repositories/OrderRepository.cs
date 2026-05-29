using Claveonce.Models;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Claveonce.Repositories
{
    public class OrderRepository
    {
        private readonly IConfiguration _config;

        public OrderRepository(IConfiguration config)
        {
            _config = config;
        }

        private SqliteConnection CreateConnection()
        {
            var connectionString = _config.GetConnectionString("DefaultConnection")
                ?? "Data Source=claveonce.db";

            return new SqliteConnection(connectionString);
        }

        public List<Order> GetAll(Guid? usuarioId)
        {
            using var connection = CreateConnection();

            var rows = connection.Query(@"
                SELECT
                    id,
                    usuario_id,
                    total,
                    estado,
                    fecha_creacion,
                    fecha_actualizacion
                FROM orders;
            ");

            var orders = new List<Order>();

            foreach (var row in rows)
            {
                var order = MapOrder(row);
                order.Items = GetItemsByOrderId(order.Id);

                orders.Add(order);
            }

            if (usuarioId != null)
            {
                orders = orders
                    .Where(o => o.UsuarioId == usuarioId)
                    .ToList();
            }

            return orders;
        }

        public Order? GetById(Guid id)
        {
            using var connection = CreateConnection();

            var row = connection.QueryFirstOrDefault(@"
                SELECT
                    id,
                    usuario_id,
                    total,
                    estado,
                    fecha_creacion,
                    fecha_actualizacion
                FROM orders
                WHERE id = @Id;
            ", new { Id = id.ToString() });

            if (row == null)
            {
                return null;
            }

            var order = MapOrder(row);
            order.Items = GetItemsByOrderId(order.Id);

            return order;
        }

        public void Create(Order order)
        {
            using var connection = CreateConnection();

            connection.Execute(@"
                INSERT INTO orders (
                    id,
                    usuario_id,
                    total,
                    estado,
                    fecha_creacion,
                    fecha_actualizacion
                )
                VALUES (
                    @Id,
                    @UsuarioId,
                    @Total,
                    @Estado,
                    @FechaCreacion,
                    @FechaActualizacion
                );
            ", new
            {
                Id = order.Id.ToString(),
                UsuarioId = order.UsuarioId.ToString(),
                Total = order.Total,
                order.Estado,
                FechaCreacion = order.FechaCreacion.ToString("o"),
                FechaActualizacion = order.FechaActualizacion?.ToString("o")
            });

            foreach (var item in order.Items)
            {
                connection.Execute(@"
                    INSERT INTO order_items (
                        order_id,
                        product_id,
                        cantidad,
                        precio_unitario
                    )
                    VALUES (
                        @OrderId,
                        @ProductId,
                        @Cantidad,
                        @PrecioUnitario
                    );
                ", new
                {
                    OrderId = order.Id.ToString(),
                    ProductId = item.ProductoId.ToString(),
                    item.Cantidad,
                    PrecioUnitario = item.PrecioUnitario
                });
            }
        }

        public void UpdateStatus(Order order)
        {
            using var connection = CreateConnection();

            connection.Execute(@"
                UPDATE orders
                SET
                    estado = @Estado,
                    fecha_actualizacion = @FechaActualizacion
                WHERE id = @Id;
            ", new
            {
                Id = order.Id.ToString(),
                order.Estado,
                FechaActualizacion = order.FechaActualizacion?.ToString("o")
            });
        }

        private List<OrderItem> GetItemsByOrderId(Guid orderId)
        {
            using var connection = CreateConnection();

            var rows = connection.Query(@"
                SELECT
                    product_id,
                    cantidad,
                    precio_unitario
                FROM order_items
                WHERE order_id = @OrderId;
            ", new { OrderId = orderId.ToString() });

            var items = new List<OrderItem>();

            foreach (var row in rows)
            {
                var item = new OrderItem();

                item.ProductoId = Guid.Parse((string)row.product_id);
                item.Cantidad = Convert.ToInt32(row.cantidad);
                item.PrecioUnitario = Convert.ToDecimal(row.precio_unitario);

                items.Add(item);
            }

            return items;
        }

        private Order MapOrder(dynamic row)
        {
            var order = new Order();

            order.Id = Guid.Parse((string)row.id);
            order.UsuarioId = Guid.Parse((string)row.usuario_id);
            order.Total = Convert.ToDecimal(row.total);
            order.Estado = row.estado;
            order.FechaCreacion = DateTime.Parse((string)row.fecha_creacion);

            if (row.fecha_actualizacion == null)
            {
                order.FechaActualizacion = null;
            }
            else
            {
                order.FechaActualizacion = DateTime.Parse((string)row.fecha_actualizacion);
            }

            return order;
        }
    }
}