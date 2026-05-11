namespace Claveonce.Models
{
    public record Product
    {
        public long Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public double Price { get; init; }
        public int Stock { get; init; }
    }

    public record CreateProductRequest(
        string Name,
        string Description,
        double Price,
        int Stock
    );

    public record UpdateProductRequest(
        string Name,
        string Description,
        double Price,
        int Stock
    );
}