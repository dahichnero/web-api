using System.ComponentModel.DataAnnotations;

namespace ProductsWebApiPD.DataTransfer
{
    public class CreateOrderDTO
    {
        public class ProductsCount
        {
            public int ProductId { get; set; }
            [Range(1,30)]
            public int Count { get; set; }
        }
        [Required]
        public string Address { get; set; }
        [Required]
        public List<ProductsCount> Products { get; set; } = new();
    }
}
