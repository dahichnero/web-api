using System.ComponentModel.DataAnnotations;

namespace ProductsWebApiPD.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
    }
}
