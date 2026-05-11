using Claveonce.Models;

namespace Claveonce.Endpoints
{
    public static class ProductsEndpoints
    {
        public static void MapProductsEndpoints(this WebApplication app)
        {
            var products = new List<Product>();
            var idCounter = 1L;

            // GET all
            app.MapGet("/products", () =>
            {
                return Results.Ok(products);
            })
            .WithTags("Products");

            // GET by id
            app.MapGet("/products/{id}", (long id) =>
            {
                var product = products.FirstOrDefault(p => p.Id == id);

                if (product is null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(product);
            })
            .WithTags("Products");

            // POST
            app.MapPost("/products", (CreateProductRequest request) =>
            {
                var product = new Product
                {
                    Id = idCounter++,
                    Name = request.Name,
                    Description = request.Description,
                    Price = request.Price,
                    Stock = request.Stock
                };

                products.Add(product);

                return Results.Ok(product);
            })
            .WithTags("Products");

            // PUT
            app.MapPut("/products/{id}", (long id, UpdateProductRequest request) =>
            {
                var existing = products.FirstOrDefault(p => p.Id == id);

                if (existing is null)
                {
                    return Results.NotFound();
                }

                var updated = existing with
                {
                    Name = request.Name,
                    Description = request.Description,
                    Price = request.Price,
                    Stock = request.Stock
                };

                products.Remove(existing);
                products.Add(updated);

                return Results.Ok(updated);
            })
            .WithTags("Products");

            // DELETE
            app.MapDelete("/products/{id}", (long id) =>
            {
                var product = products.FirstOrDefault(p => p.Id == id);

                if (product is null)
                {
                    return Results.NotFound();
                }

                products.Remove(product);

                return Results.Ok();
            })
            .WithTags("Products");
        }
    }
}