using Claveonce.Models;
using Claveonce.Helpers;

namespace Claveonce.Endpoints
{
    public static class ProductsEndpoints
    {
        public static void MapProductsEndpoints(this WebApplication app)
        {
            var products = new List<Product>();

            // GET ALL
            app.MapGet("/api/products", (string? categoria, string? nombre) =>
            {
                var productsFiltered = products.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(categoria))
                {
                    productsFiltered = productsFiltered.Where(p => p.Categoria.ToLower() == categoria.ToLower());
                }

                if (!string.IsNullOrWhiteSpace(nombre))
                {
                    productsFiltered = productsFiltered.Where(p => p.Nombre.ToLower().Contains(nombre.ToLower()));
                }

                return Results.Ok(productsFiltered);
            })
            .WithTags("Products")
            .WithSummary("Lista productos")
            .WithDescription("Lista productos. Permite filtrar por categoria y nombre.");

            // GET BY ID
            app.MapGet("/api/products/{id}", (Guid id) =>
            {
                var product = products.FirstOrDefault(p => p.Id == id);

                if (product == null)
                {
                    return Results.NotFound(ErrorResponse.Create(
                        "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                        "Not Found",
                        404,
                        "El recurso solicitado no fue encontrado.",
                        "/api/products/" + id,
                        "PRD-001",
                        "Producto no encontrado."
                    ));
                }

                return Results.Ok(product);
            })
            .WithTags("Products")
            .WithSummary("Obtiene producto por ID")
            .WithDescription("Obtiene un producto específico utilizando su identificador.");

            // POST
            app.MapPost("/api/products", (CreateProductRequest request) =>
            {
                if (string.IsNullOrWhiteSpace(request.Nombre) ||
                    request.Precio <= 0 ||
                    request.Stock < 0 ||
                    string.IsNullOrWhiteSpace(request.Categoria))
                {
                    return Results.BadRequest(ErrorResponse.Create(
                        "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                        "Bad Request",
                        400,
                        "La solicitud contiene datos inválidos.",
                        "/api/products",
                        "PRD-002",
                        "Los datos del producto son inválidos."
                    ));
                }

                var duplicatedProduct = products.FirstOrDefault(p =>
                    p.Nombre.ToLower() == request.Nombre.ToLower() &&
                    p.Categoria.ToLower() == request.Categoria.ToLower());

                if (duplicatedProduct != null)
                {
                    return Results.Conflict(ErrorResponse.Create(
                        "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                        "Conflict",
                        409,
                        "Ya existe un recurso con esos datos.",
                        "/api/products",
                        "PRD-003",
                        "Ya existe un producto con ese nombre en la categoría '" + request.Categoria + "'."
                    ));
                }

                var product = new Product();

                product.Id = Guid.NewGuid();
                product.Nombre = request.Nombre;
                product.Descripcion = request.Descripcion;
                product.Precio = request.Precio;
                product.Stock = request.Stock;
                product.Categoria = request.Categoria;
                product.FechaCreacion = DateTime.UtcNow;

                products.Add(product);

                return Results.Created("/api/products/" + product.Id, product);
            })
            .WithTags("Products")
            .WithSummary("Crea nuevo producto")
            .WithDescription("Crea un nuevo producto con nombre, descripción, precio, stock y categoría.");

            // PUT
            app.MapPut("/api/products/{id}", (Guid id, UpdateProductRequest request) =>
            {
                var product = products.FirstOrDefault(p => p.Id == id);

                if (product == null)
                {
                    return Results.NotFound(ErrorResponse.Create(
                        "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                        "Not Found",
                        404,
                        "El recurso solicitado no fue encontrado.",
                        "/api/products/" + id,
                        "PRD-001",
                        "Producto no encontrado."
                    ));
                }

                if (string.IsNullOrWhiteSpace(request.Nombre) ||
                    request.Precio <= 0 ||
                    request.Stock < 0 ||
                    string.IsNullOrWhiteSpace(request.Categoria))
                {
                    return Results.BadRequest(ErrorResponse.Create(
                        "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                        "Bad Request",
                        400,
                        "La solicitud contiene datos inválidos.",
                        "/api/products/" + id,
                        "PRD-002",
                        "Los datos del producto son inválidos."
                    ));
                }

                product.Nombre = request.Nombre;
                product.Descripcion = request.Descripcion;
                product.Precio = request.Precio;
                product.Stock = request.Stock;
                product.Categoria = request.Categoria;

                return Results.Ok(product);
            })
            .WithTags("Products")
            .WithSummary("Actualiza producto existente")
            .WithDescription("Actualiza los datos de un producto existente.");

            // DELETE
            app.MapDelete("/api/products/{id}", (Guid id) =>
            {
                var product = products.FirstOrDefault(p => p.Id == id);

                if (product == null)
                {
                    return Results.NotFound(ErrorResponse.Create(
                        "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                        "Not Found",
                        404,
                        "El recurso solicitado no fue encontrado.",
                        "/api/products/" + id,
                        "PRD-001",
                        "Producto no encontrado."
                    ));
                }

                products.Remove(product);

                return Results.NoContent();
            })
            .WithTags("Products")
            .WithSummary("Elimina producto")
            .WithDescription("Elimina un producto existente utilizando su identificador.");
        }
    }
}