using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Data;
using Shop.Models;

namespace Shop.Controllers
{
    [Route("v1")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]        
        public async Task<ActionResult<dynamic>> Get([FromServices] DataContext context)
        {
            var employee = new User { Id = 1, Username = "Mr. White", Password = "123456", Role = "employee" };    
            var manager = new User { Id = 2, Username = "Mr. Orange", Password = "123456", Role = "manager" };    
            var category = new Category { Id = 1, Title = "Informática" };    
            var product = new Product { Id = 1, Category = category, CategoryId = 1, Title = "Mouse", Price = 299, Description = "Mouse Ótico" };    

            context.Users.Add(employee);
            context.Users.Add(manager);
            context.Categories.Add(category);
            context.Products.Add(product);
            await context.SaveChangesAsync();
                        
            return Ok(new
            {
               message = "Dados Configurados"     
            });
        }

        [HttpGet("compression-test")]
        [AllowAnonymous]        
        public ActionResult<dynamic> GetCompressionTest()
        {
            dynamic data = new List<string>();  
            for (int i = 1; i <= 100; i++)  
            {  
                data.Add("ID :" + i.ToString());  
                data.Add("Name :" + i.ToString());  
                data.Add("Address :" + i.ToString());  
                data.Add("Email :" + i.ToString());  
                data.Add("Telephone :" + i.ToString());  
            }
             
           return Ok(data);  
        }        
    }    
}