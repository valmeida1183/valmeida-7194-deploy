using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shop.Data;

namespace Shop
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // Método que adiciona os servicos do Asp.net Core ou externos que a aplicação deseja usar.
        public void ConfigureServices(IServiceCollection services)
        {
            // adiciona o CORS
            services.AddCors();

            // Configura o Gzip e compacta todo o response que for do mimeType "application/json"
            services.AddResponseCompression(opt =>
            {
                opt.Providers.Add<GzipCompressionProvider>();
                opt.MimeTypes.Concat(new[] { "application/json" });
            });

            //para cachear toda a aplicação, o melhor é definir o cache por endepoint
            //services.AddResponseCaching();

            // noviade do Aspnet Core 3.0.0 +, não precisa acrescentar o MVC, pois em uma WebApi só é necessário as controllers
            services.AddControllers();

            // Configs de Autenticação e Jwt
            var key = Encoding.ASCII.GetBytes(Settings.Secret);
            services.AddAuthentication(opt =>
            {
                // Diz que a autenticação desta Api será por Jwt
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(opt =>
            {
                // Configura o Jwt
                opt.RequireHttpsMetadata = false;
                opt.SaveToken = true;
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    // Configs para validar o token
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            // Config do EF com database em memória
            //services.AddDbContext<DataContext>(opt => opt.UseInMemoryDatabase("MyMemoryDatabase"));

            //Config do EF com databse real
            services.AddDbContext<DataContext>(opt => opt.UseSqlServer(Configuration.GetConnectionString("connectionString")));

            // toda requisição cria um datacontext na memória, toda vez que algúem pedir um ddatacontext é devolvido o da memória
            // quando a requisição acaba o datacontext é destruído e consequentemente fechando a conexão com o banco.
            
            //services.AddScoped<DataContext, DataContext>(); --> No asp.netCore 3.0+ não é mais necessário essa linha pois o "services.AddDbContext<DataContext>" já faz isso por debaixo dos panos 
            //services.AddTransient<DataContext, DataContext>(); --> cria uma nova instância toda vez que é solicitado, criando uma nova conexão com o banco (não é o que queremos)
            //services.AddSingleton<DataContext, DataContext>(); --> cria uma única instância quando a aplicação inicia e sempre devolve o mesmo, não fehcnado nunca a conexão com o db (não é o que queremos)

            services.AddSwaggerGen(c => 
            {
                c.SwaggerDoc("v1", new OpenApiInfo 
                {
                    Title = "Shop Api", 
                    Version = "v1"
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            /* if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            } */

            app.UseDeveloperExceptionPage();

            app.UseSwagger();
            app.UseSwaggerUI(c => 
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shop API V1");        
            });

            app.UseHttpsRedirection();           

            app.UseRouting();            
            
            app.UseCors(
                x => x.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
