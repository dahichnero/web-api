using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductsWebApiPD.DataTransfer;
using ProductsWebApiPD.Models;
using SixLabors.ImageSharp;
using System.Drawing;

namespace ProductsWebApiPD.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private ProductsContext context;
        private readonly IWebHostEnvironment hosting;

        public ProductsController(ProductsContext context, IWebHostEnvironment hosting)
        {
            this.context = context;
            this.hosting = hosting;
        }

        [HttpGet]
        //[Authorize(AuthenticationSchemes =JwtBearerDefaults.AuthenticationScheme)]
        public async Task<List<Product>> GetProducts()
        {
            return await context.Products.Include(p => p.Category).ToListAsync();
        }

        [HttpGet("category/{id}")]
        public async Task<List<Product>> GetProductByCategory(int id)
        {
            return await context.Products.Include(p=>p.Category).Where(z=>z.CategoryId==id).ToListAsync();
        }

        [HttpPost("add")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
        public async Task<IActionResult> AddProduct(UpdateProductDTO productDTO)
        {
            if (!context.Categories.Any(c=>c.CategoryId==productDTO.CategoryId))
            {
                return BadRequest("Invalid Category");
            }
            var product=productDTO.ToProduct();
            context.Products.Add(product);
            await context.SaveChangesAsync();
            return Ok(product);
        }

        [HttpPut("update/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
        public async Task<IActionResult> UpdateProduct(UpdateProductDTO productDTO, int id)
        {
            if (!context.Categories.Any(c=>c.CategoryId==productDTO.CategoryId))
            {
                return BadRequest("Invalid Category");
            }
            var product = context.Products.Find(id);
            if (product is null)
            {
                return BadRequest();
            }
            product.Name = productDTO.Name;
            product.Description = productDTO.Description;
            product.CategoryId = productDTO.CategoryId;
            product.Price = productDTO.Price;
            await context.SaveChangesAsync();
            return Ok(product);
        }

        [HttpDelete("delete/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = context.Products.Find(id);
            if (product is null)
            {
                return BadRequest();
            }
            context.Products.Remove(product);
            await context.SaveChangesAsync();
            return Ok(product);
        }

        [HttpPut("update/photo/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "admin")]
        public async Task<IActionResult> SetPhoto(int id, [FromForm]IFormFile file)
        {
            var product = context.Products.Find(id);
            if (product is null)
            {
                return NotFound();
            }
            if (file.Length > 2 * 1024 * 1024)
            {
                return BadRequest("Max image size is 2MB");
            }
            var stream = file.OpenReadStream();
            try
            {
                // проверяем формат файла
                var format = await SixLabors.ImageSharp.Image.DetectFormatAsync(stream);
                if (format.DefaultMimeType != "image/png")
                {
                    return BadRequest("Invalid file format");
                }
            }
            catch (UnknownImageFormatException)
            {
                return BadRequest("Invalid file format");
            }

            // если все правильно, то сохраняем в файл
            string filename = Path.Combine(hosting.WebRootPath, "images", Path.GetRandomFileName() + ".png");
            using (FileStream fs = new FileStream(filename, FileMode.Create))
            {
                stream.Position = 0;
                stream.CopyTo(fs);
            }
            // сохраним имя файла в БД
            product.Photo = Path.GetFileName(filename);
            context.SaveChanges();
            return Ok();
        }
    }
}
