using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;

namespace Shop.Controllers
{
    [Route("v1/categories")]
    public class CategoryController : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(VaryByHeader = "User-Agent", Location = ResponseCacheLocation.Any, Duration = 30)]
        // quando o cache está definido no startup e se deseja remover o cache do endpoint.
        /* [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]  */ 
        public async Task<ActionResult<List<Category>>> Get(
            [FromServices] DataContext context)
        {
            // AsNoTracking é para não trazer infos que o EF usa para rastrear mudanças no objeto, no caso
            // como o objeto será devolvido na requisição essas infos não são necessárias, tornando a busca menos custosa.

            // ToList/ToListAsync toda vez que é chamado ele executa a query de fato, portanto qualquer sort, where, skip, take... 
            // deve ser feito ANTES de chamar o ToListAsync, caso contrário o sort, where... serão feitos em memória causando um leak na Applicação.
            var categories = await context.Categories.AsNoTracking().ToListAsync();
            return Ok(categories);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        //[Route("{id:int}")]    
        public async Task<ActionResult<Category>> GetById(
            int id,
            [FromServices] DataContext context)
        {
            var category = await context.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return Ok(category);
        }

        [HttpPost]
        [Authorize(Roles = "employee")]
        public async Task<ActionResult<Category>> Post(
            [FromBody] Category model, [FromServices] DataContext context)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                context.Categories.Add(model);
                await context.SaveChangesAsync();

                return Ok(model);
            }
            catch
            {
                return BadRequest(new { message = "Não foi possível criar a categoria" });
            }
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "employee")]
        //[Route("{id:int}")]    
        public async Task<ActionResult<Category>> Put(
            int id,
            [FromBody] Category model,
            [FromServices] DataContext context)
        {
            // Verifica se o ID informado é o mesmo do modelo
            if (id != model.Id)
            {
                return NotFound(new { message = "Categoria não encontrada" });
            }

            // Verifica se os dados são inválidos
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Desta Forma o EF vai verificar o que foi mudado em relação ao registro do banco e persistir no banco somente o que foi mudado.
                context.Entry<Category>(model).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return Ok(model);
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(new { message = "Este registro já foi atualizado" });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Não foi possível atualizar a categoria" });
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "employee")]
        //[Route("{id:int}")]    
        public async Task<ActionResult<Category>> Delete(
            int id,
            [FromServices] DataContext context)
        {
            var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if (category == null)
            {
                return NotFound(new { message = "Categoria não encontrda" });
            }

            try
            {
                context.Categories.Remove(category);
                await context.SaveChangesAsync();
                return Ok(new { message = "Categoria removida com sucesso" });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Não foi possível remover a categoria" });
            }
        }
    }
}
