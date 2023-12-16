using ProductsWebApiPD.Models;
using System.ComponentModel.DataAnnotations;

namespace ProductsWebApiPD.DataTransfer
{
    public class UpdateProductDTO
    {
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }=string.Empty;

        [Required]
        public decimal Price { get; set; }
        [Required]
        public int CategoryId { get; set;}

        public Product ToProduct()
        {
            return new Product
            {
                Name = Name,
                Description = Description,
                Price = Price,
                CategoryId = CategoryId
            };
        }
    }
}
