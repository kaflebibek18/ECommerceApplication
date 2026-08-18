namespace ECommerce.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Price { get; set; }
        public string Size { get; set; } = "";      // e.g. "38-45" or CSV
        public string Color { get; set; } = "";
        public string Brand { get; set; } = "";
        public int? StyleId { get; set; }
        public int StockQuantity { get; set; }
        public string ImagePath { get; set; } = ""; // "/uploads/xxxx.jpg"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
