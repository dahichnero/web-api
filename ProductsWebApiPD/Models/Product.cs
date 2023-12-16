using System.ComponentModel.DataAnnotations;

namespace ProductsWebApiPD.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Photo { get; set; }

        public decimal Price { get; set; }

        public int CategoryId { get; set; }

        public Category Category { get; set; } = null!;
    }
}
