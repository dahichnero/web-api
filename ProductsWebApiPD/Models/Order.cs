using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ProductsWebApiPD.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        public DateTime Time { get; set; }

        public IdentityUser<int> User { get; set; } = null!;
        public int UserId { get; set; }

        [StringLength(250)]
        public string Address { get; set; } = string.Empty;
    }
}
