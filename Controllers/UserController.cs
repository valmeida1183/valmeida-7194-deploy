using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;
using Shop.Services;

namespace Shop.Controllers
{
    [Route("v1/users")]
    public class UserController : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult<List<User>>> Get([FromServices] DataContext context)
        {
            var users = await context
                .Users
                .AsNoTracking()
                .ToListAsync();

            return users;
        }

        [HttpPost]
        [AllowAnonymous]
        //[Authorize(Roles = "manager")]
        public async Task<ActionResult<User>> Post([FromServices] DataContext context, [FromBody] User model)
        {
            //verifica se os dados são válidos
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Força o Usuário a ser sempre "employee"
                model.Role = "employee";

                context.Users.Add(model);
                await context.SaveChangesAsync();

                // Esconde a Senha antes de retornar o response
                model.Password = "*******";

                return Ok(model);
            }
            catch (Exception)
            {                
                return BadRequest(new { message = "Não foi possível criar o usuário"});
            }
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult<User>> Put(
            [FromServices] DataContext context, 
            int id, 
            [FromBody] User model) 
        {
            // Verifica se o ID informado é o mesmo do modelo
            if (id != model.Id)
            {
                return NotFound(new { message = "Usuário não encontrado" });
            }

            // Verifica se os dados são inválidos
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                context.Entry<User>(model).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return Ok(model);
            }
            catch (Exception)
            {                
                return BadRequest(new { message = "Não foi possível criar o usuário"});
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<dynamic>> Authenticate([FromServices] DataContext context, [FromBody] User model)
        {
            var user = await context.Users
            .AsNoTracking()
            .Where(u => u.Username == model.Username && u.Password == model.Password)
            .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new { message = "Usuário ou senha inválidos" });
            }
            
            var token = TokenService.GenerateToken(user);
            // Esconde a senha
            user.Password = "******";

            return Ok(new { user = user, token = token });
        }
    }    
}