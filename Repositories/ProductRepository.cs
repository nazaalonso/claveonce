using Claveonce.Models;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Claveonce.Repositories
{
    public class ProductRepository
    {
        private readonly IConfiguration _config;

        public ProductRepository(IConfiguration config)
        {
            _config = config;
        }

        private SqliteConnection CreateConnection()
        {
            var connectionString = _config.GetConnectionString("DefaultConnection")
                ?? "Data Source=claveonce.db";

            return new SqliteConnection(connectionString);
        }

        public List<Product> GetAll(string? categoria, string? nombre)
        {
            using var connection = CreateConnection();

            var rows = connection.Query(@"
                SELECT
                    id,
                    nombre,
                    descripcion,
                    precio,
                    stock,
                    categoria,
                    fecha_creacion
                FROM products;
            ");

            var products = new List<Product>();

            foreach (var row in rows)
            {
                var product = new Product();

                product.Id = Guid.Parse((string)row.id);
                product.Nombre = row.nombre;
                product.Descripcion = row.descripcion;
                product.Precio = Convert.ToDecimal(row.precio);
                product.Stock = Convert.ToInt32(row.stock);
                product.Categoria = row.categoria;
                product.FechaCreacion = DateTime.Parse((string)row.fecha_creacion);

                products.Add(product);
            }

            if (!string.IsNullOrWhiteSpace(categoria))
            {
                products = products
                    .Where(p => p.Categoria.ToLower() == categoria.ToLower())
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(nombre))
            {
                products = products
                    .Where(p => p.Nombre.ToLower().Contains(nombre.ToLower()))
                    .ToList();
            }

            return products;
        }

        public Product? GetById(Guid id)
        {
            using var connection = CreateConnection();

            var row = connection.QueryFirstOrDefault(@"
                SELECT
                    id,
                    nombre,
                    descripcion,
                    precio,
                    stock,
                    categoria,
                    fecha_creacion
                FROM products
                WHERE id = @Id;
            ", new { Id = id.ToString() });

            if (row == null)
            {
                return null;
            }

            var product = new Product();

            product.Id = Guid.Parse((string)row.id);
            product.Nombre = row.nombre;
            product.Descripcion = row.descripcion;
            product.Precio = Convert.ToDecimal(row.precio);
            product.Stock = Convert.ToInt32(row.stock);
            product.Categoria = row.categoria;
            product.FechaCreacion = DateTime.Parse((string)row.fecha_creacion);

            return product;
        }

        public Product? GetByNameAndCategory(string nombre, string categoria)
        {
            using var connection = CreateConnection();

            var row = connection.QueryFirstOrDefault(@"
                SELECT
                    id,
                    nombre,
                    descripcion,
                    precio,
                    stock,
                    categoria,
                    fecha_creacion
                FROM products
                WHERE LOWER(nombre) = LOWER(@Nombre)
                AND LOWER(categoria) = LOWER(@Categoria);
            ", new
            {
                Nombre = nombre,
                Categoria = categoria
            });

            if (row == null)
            {
                return null;
            }

            var product = new Product();

            product.Id = Guid.Parse((string)row.id);
            product.Nombre = row.nombre;
            product.Descripcion = row.descripcion;
            product.Precio = Convert.ToDecimal(row.precio);
            product.Stock = Convert.ToInt32(row.stock);
            product.Categoria = row.categoria;
            product.FechaCreacion = DateTime.Parse((string)row.fecha_creacion);

            return product;
        }

        public void Create(Product product)
        {
            using var connection = CreateConnection();

            connection.Execute(@"
                INSERT INTO products (
                    id,
                    nombre,
                    descripcion,
                    precio,
                    stock,
                    categoria,
                    fecha_creacion
                )
                VALUES (
                    @Id,
                    @Nombre,
                    @Descripcion,
                    @Precio,
                    @Stock,
                    @Categoria,
                    @FechaCreacion
                );
            ", new
            {
                Id = product.Id.ToString(),
                product.Nombre,
                product.Descripcion,
                Precio = product.Precio,
                product.Stock,
                product.Categoria,
                FechaCreacion = product.FechaCreacion.ToString("o")
            });
        }

        public void Update(Product product)
        {
            using var connection = CreateConnection();

            connection.Execute(@"
                UPDATE products
                SET
                    nombre = @Nombre,
                    descripcion = @Descripcion,
                    precio = @Precio,
                    stock = @Stock,
                    categoria = @Categoria
                WHERE id = @Id;
            ", new
            {
                Id = product.Id.ToString(),
                product.Nombre,
                product.Descripcion,
                Precio = product.Precio,
                product.Stock,
                product.Categoria
            });
        }

        public void Delete(Guid id)
        {
            using var connection = CreateConnection();

            connection.Execute(@"
                DELETE FROM products
                WHERE id = @Id;
            ", new { Id = id.ToString() });
        }
    }
}