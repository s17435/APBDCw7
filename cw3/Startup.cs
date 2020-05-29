using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cw4.DAL;
using cw4.Middlewares;
using cw4.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace cw4
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IDbService, MockDbService>();

            services.AddTransient<IStudentDBService, SqlServerStudentDbServer>();
            
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).
                AddJwtBearer(options =>
                {

                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {

                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidIssuer = "Gakko",
                        ValidAudience = "Students",
                        IssuerSigningKey = new SymmetricSecurityKey
                            (Encoding.UTF8.GetBytes(Configuration["SecretKey"]))
                    };
                });
            
            
            
            
            services.AddControllers()
                .AddXmlSerializerFormatters();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IStudentDBService studentDbService)
        {
            // app.UseWhen(context => context.Request.Path.ToString().Contains("secret"), app =>
            // {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
        



        // app.UseMiddleware<LoggingMiddleware>();
            //     app.Use(async (context, next) =>
            //     {
            //         if (!context.Request.Headers.ContainsKey("Index"))
            //         {
            //             context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            //             await context.Response.WriteAsync("Student not found :(");
            //             return;
            //         }
            //
            //         string index = context.Request.Headers["Index"].ToString();
            //         var student = studentDbService.GetStudent(index);
            //         Console.WriteLine(student.FirstName);
            //         if (student == null)
            //         {
            //             context.Response.StatusCode = StatusCodes.Status404NotFound;
            //             await context.Response.WriteAsync("Musisz podać prawidłowy numer indeksu");
            //             return;
            //         }
            //
            //
            //         await next();
            //     });
        
        

        app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            
        }
    }
}
