using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductsWebApiPD.DataTransfer;
using ProductsWebApiPD.Models;
using System.Security.Claims;

namespace ProductsWebApiPD.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private ProductsContext context;
        private readonly UserManager<IdentityUser<int>> userManager;

        public OrdersController(ProductsContext context, UserManager<IdentityUser<int>> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder(CreateOrderDTO createDto)
        {
            var userId=Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (createDto.Products.DistinctBy(p=>p.ProductId).Count()!=createDto.Products.Count)
            {
                return BadRequest("Products in order must be unique");
            }
            var products = createDto.Products.Select(p => new
            {
                Product=context.Products.Find(p.ProductId),
                Count=p.Count
            });
            if (products.Any(p=>p.Product is null))
            {
                return BadRequest("Invalid product id");
            }
            var order = new Order
            {
                Address=createDto.Address,
                Time=DateTime.Now,
                UserId=userId
            };
            context.Orders.Add(order);
            context.OrderProducts.AddRange(products.Select(p => new OrderProduct
            {
                Count=p.Count,
                CurrentPrice=p.Product!.Price,
                ProductId=p.Product!.ProductId,
                Order=order
            }));
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("user/{userId}")]
        public async Task<List<OrderInfoDTO>> CreateOrder(int userId)
        {
            var orders = context.Orders.Where(o => o.UserId == userId).Select(o => new OrderInfoDTO
            {
                Address=o.Address,
                Time=o.Time,
                UserId=userId,
                Products=context.OrderProducts.Include(o=>o.Product).Where(op => op.OrderId == o.OrderId).Select(op => new ProductInOrderDTO
                {
                    Count = op.Count,
                    Name = op.Product.Name,
                    Photo = op.Product.Photo,
                    Price = op.CurrentPrice,
                    ProductId = op.Product.ProductId
                }).ToList()
            }).ToList();
            return orders;
        }
    }
}
