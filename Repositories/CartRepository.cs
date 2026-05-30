using Claveonce.Models;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Claveonce.Repositories
{
    public class CartRepository
    {
        private readonly IConfiguration _config;

        public CartRepository(IConfiguration config)
        {
            _config = config;
        }

        private SqliteConnection CreateConnection()
        {
            var connectionString = _config.GetConnectionString("DefaultConnection")
                ?? "Data Source=claveonce.db";

            return new SqliteConnection(connectionString);
        }

        public Cart? GetByUserId(Guid userId)
        {
            using var connection = CreateConnection();

            var cartRow = connection.QueryFirstOrDefault(@"
                SELECT
                    usuario_id,
                    fecha_actualizacion
                FROM carts
                WHERE usuario_id = @UsuarioId;
            ", new { UsuarioId = userId.ToString() });

            if (cartRow == null)
            {
                return null;
            }

            var cart = new Cart();

            cart.UsuarioId = Guid.Parse((string)cartRow.usuario_id);
            cart.FechaActualizacion = DateTime.Parse((string)cartRow.fecha_actualizacion);
            cart.Items = GetItemsByUserId(userId);

            return cart;
        }

        public void CreateCartIfNotExists(Guid userId)
        {
            using var connection = CreateConnection();

            connection.Execute(@"
                INSERT OR IGNORE INTO carts (
                    usuario_id,
                    fecha_actualizacion
                )
                VALUES (
                    @UsuarioId,
                    @FechaActualizacion
                );
            ", new
            {
                UsuarioId = userId.ToString(),
                FechaActualizacion = DateTime.UtcNow.ToString("o")
            });
        }

        public void AddItem(Guid userId, Guid productId, int cantidad)
        {
            using var connection = CreateConnection();

            CreateCartIfNotExists(userId);

            var existingItem = connection.QueryFirstOrDefault(@"
                SELECT
                    usuario_id,
                    producto_id,
                    cantidad
                FROM cart_items
                WHERE usuario_id = @UsuarioId
                AND producto_id = @ProductoId;
            ", new
            {
                UsuarioId = userId.ToString(),
                ProductoId = productId.ToString()
            });

            if (existingItem == null)
            {
                connection.Execute(@"
                    INSERT INTO cart_items (
                        usuario_id,
                        producto_id,
                        cantidad
                    )
                    VALUES (
                        @UsuarioId,
                        @ProductoId,
                        @Cantidad
                    );
                ", new
                {
                    UsuarioId = userId.ToString(),
                    ProductoId = productId.ToString(),
                    Cantidad = cantidad
                });
            }
            else
            {
                var nuevaCantidad = Convert.ToInt32(existingItem.cantidad) + cantidad;

                connection.Execute(@"
                    UPDATE cart_items
                    SET cantidad = @Cantidad
                    WHERE usuario_id = @UsuarioId
                    AND producto_id = @ProductoId;
                ", new
                {
                    UsuarioId = userId.ToString(),
                    ProductoId = productId.ToString(),
                    Cantidad = nuevaCantidad
                });
            }

            UpdateCartDate(userId);
        }

        public void UpdateItem(Guid userId, Guid productId, int cantidad)
        {
            using var connection = CreateConnection();

            connection.Execute(@"
                UPDATE cart_items
                SET cantidad = @Cantidad
                WHERE usuario_id = @UsuarioId
                AND producto_id = @ProductoId;
            ", new
            {
                UsuarioId = userId.ToString(),
                ProductoId = productId.ToString(),
                Cantidad = cantidad
            });

            UpdateCartDate(userId);
        }

        public void DeleteItem(Guid userId, Guid productId)
        {
            using var connection = CreateConnection();

            connection.Execute(@"
                DELETE FROM cart_items
                WHERE usuario_id = @UsuarioId
                AND producto_id = @ProductoId;
            ", new
            {
                UsuarioId = userId.ToString(),
                ProductoId = productId.ToString()
            });

            UpdateCartDate(userId);
        }

        public void DeleteCart(Guid userId)
        {
            using var connection = CreateConnection();

            connection.Execute(@"
                DELETE FROM cart_items
                WHERE usuario_id = @UsuarioId;
            ", new { UsuarioId = userId.ToString() });

            connection.Execute(@"
                DELETE FROM carts
                WHERE usuario_id = @UsuarioId;
            ", new { UsuarioId = userId.ToString() });
        }

        public bool ItemExists(Guid userId, Guid productId)
        {
            using var connection = CreateConnection();

            var item = connection.QueryFirstOrDefault(@"
                SELECT producto_id
                FROM cart_items
                WHERE usuario_id = @UsuarioId
                AND producto_id = @ProductoId;
            ", new
            {
                UsuarioId = userId.ToString(),
                ProductoId = productId.ToString()
            });

            return item != null;
        }

        private List<CartItem> GetItemsByUserId(Guid userId)
        {
            using var connection = CreateConnection();

            var rows = connection.Query(@"
                SELECT
                    producto_id,
                    cantidad
                FROM cart_items
                WHERE usuario_id = @UsuarioId;
            ", new { UsuarioId = userId.ToString() });

            var items = new List<CartItem>();

            foreach (var row in rows)
            {
                var item = new CartItem();

                item.ProductoId = Guid.Parse((string)row.producto_id);
                item.Cantidad = Convert.ToInt32(row.cantidad);

                items.Add(item);
            }

            return items;
        }

        private void UpdateCartDate(Guid userId)
        {
            using var connection = CreateConnection();

            connection.Execute(@"
                UPDATE carts
                SET fecha_actualizacion = @FechaActualizacion
                WHERE usuario_id = @UsuarioId;
            ", new
            {
                UsuarioId = userId.ToString(),
                FechaActualizacion = DateTime.UtcNow.ToString("o")
            });
        }
    }
}