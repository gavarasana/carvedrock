
namespace CarvedRock.Core.Models
{
    public record NewProductModel
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public  double Price { get; set; }
        public string Category { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
    }

    public record ProductModel: NewProductModel
    {
        public int Id { get; set; }
    }
}
