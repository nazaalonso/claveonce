namespace Claveonce.Models
{
    public record Item
    {
        public long Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public double Price { get; init; }
        public long Stock { get; init; }
        public string CreatedAt { get; init; } = string.Empty;
        public string? UpdatedAt { get; init; }
    }

    public record CreateItemRequest(
        string Name,
        string? Description,
        decimal Price,
        int Stock
    );

    public record UpdateItemRequest(
        string Name,
        string? Description,
        decimal Price,
        int Stock
    );
}