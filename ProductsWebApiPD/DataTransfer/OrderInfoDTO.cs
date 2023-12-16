using ProductsWebApiPD.Models;

namespace ProductsWebApiPD.DataTransfer
{
    public class OrderInfoDTO
    {
        public int OrderId { get; set; }
        public string Address { get; set; }
        public DateTime Time { get; set; }
        public int UserId { get; internal set; }
        public List<ProductInOrderDTO> Products { get; set; }
    }
}
