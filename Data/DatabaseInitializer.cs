using Dapper;
using Microsoft.Data.Sqlite;

namespace Claveonce.Data
{
    public class DatabaseInitializer
    {
        private readonly IConfiguration _config;

        public DatabaseInitializer(IConfiguration config)
        {
            _config = config;
        }

        public void Initialize()
        {
            var connectionString = _config.GetConnectionString("DefaultConnection")
                ?? "Data Source=claveonce.db";

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS users (
                    id TEXT PRIMARY KEY,
                    nombre TEXT NOT NULL,
                    apellido TEXT NOT NULL,
                    email TEXT NOT NULL,
                    password_hash TEXT NOT NULL,
                    fecha_registro TEXT NOT NULL,
                    activo INTEGER NOT NULL,
                    intentos_fallidos INTEGER NOT NULL
                );
            ");

            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS products (
                    id TEXT PRIMARY KEY,
                    nombre TEXT NOT NULL,
                    descripcion TEXT NOT NULL,
                    precio REAL NOT NULL,
                    stock INTEGER NOT NULL,
                    categoria TEXT NOT NULL,
                    fecha_creacion TEXT NOT NULL
                );
            ");

            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS carts (
                    usuario_id TEXT PRIMARY KEY,
                    fecha_actualizacion TEXT NOT NULL,
                    FOREIGN KEY (usuario_id) REFERENCES users(id)
                );
            ");

            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS cart_items (
                    usuario_id TEXT NOT NULL,
                    producto_id TEXT NOT NULL,
                    cantidad INTEGER NOT NULL,
                    PRIMARY KEY (usuario_id, producto_id),
                    FOREIGN KEY (usuario_id) REFERENCES carts(usuario_id),
                    FOREIGN KEY (producto_id) REFERENCES products(id)
                );
            ");

            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS orders (
                    id TEXT PRIMARY KEY,
                    usuario_id TEXT NOT NULL,
                    total REAL NOT NULL,
                    estado TEXT NOT NULL,
                    fecha_creacion TEXT NOT NULL,
                    fecha_actualizacion TEXT NULL,
                    FOREIGN KEY (usuario_id) REFERENCES users(id)
                );
            ");

            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS order_items (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    order_id TEXT NOT NULL,
                    product_id TEXT NOT NULL,
                    cantidad INTEGER NOT NULL,
                    precio_unitario REAL NOT NULL,
                    FOREIGN KEY (order_id) REFERENCES orders(id),
                    FOREIGN KEY (product_id) REFERENCES products(id)
                );
            ");

            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS notifications (
                    id TEXT PRIMARY KEY,
                    usuario_id TEXT NOT NULL,
                    mensaje TEXT NOT NULL,
                    tipo TEXT NOT NULL,
                    estado TEXT NOT NULL,
                    fecha_envio TEXT NOT NULL,
                    FOREIGN KEY (usuario_id) REFERENCES users(id)
                );
            ");
        }
    }
}