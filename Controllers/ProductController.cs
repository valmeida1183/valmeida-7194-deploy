using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;

namespace Shop.Controllers
{
    [Route("v1/products")]
    public class ProductController : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<Product>>> Get([FromServices] DataContext context)
        {
            
            // O Include deve vir antes de AsNoTracking
            var products = await context
                .Products
                .Include(p => p.Category)
                .AsNoTracking()
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("{id:int}")]  
        [AllowAnonymous]          
        public async Task<ActionResult<Product>> GetById(int id,
            [FromServices] DataContext context)
        {
            var product = await context
                .Products
                .Include(p => p.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            return Ok(product);
        }

        [HttpGet("categories/{id:int}")]
        [AllowAnonymous]            
        public async Task<ActionResult<List<Product>>> GetByCategory(
            int id, [FromServices] DataContext context)
        {
            var product = await context
                .Products
                .Include(p => p.Category)
                .AsNoTracking()
                .Where(p => p.CategoryId == id)
                .ToListAsync();

            return Ok(product);
        }

        [HttpPost]
        [Authorize(Roles = "employee")]
        public async Task<ActionResult<Product>> Post(
            [FromBody] Product model, [FromServices] DataContext context)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                context.Products.Add(model);
                await context.SaveChangesAsync();

                return Ok(model);
            }
            catch
            {
                return BadRequest(new { message = "Não foi possível criar o produto" });
            }
        }

    }
}