namespace ProductsWebApiPD.DataTransfer
{
    public class ProductInOrderDTO
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Photo { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Price { get; set; }
    }
}
