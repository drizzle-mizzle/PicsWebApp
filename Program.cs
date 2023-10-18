using PicsWebApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Microsoft.OpenApi.Models;

namespace PicsWebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddControllers();
            builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(builder.Configuration.GetConnectionString("sqlite"));
            });
            builder.Services.AddSwaggerGen(config =>
            {
                config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Токен, полученный при авторизации",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "bearer"
                });
                config.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                config.SwaggerDoc("v1", new() { Title = "Описание эндпоинтов", Version = "v1" });
                var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
                config.IncludeXmlComments(xmlPath);
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Errors/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            var db = app.Services.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.MigrateAsync();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Pages}/{action=Home}/{id?}");
            app.MapControllerRoute(
                name: "Register",
                pattern: "sign-up",
                defaults: new { controller = "Pages", action = "Register" });
            app.MapControllerRoute(
                name: "Login",
                pattern: "sign-in",
                defaults: new { controller = "Pages", action = "Login" });
            app.MapControllerRoute(
                name: "Users",
                pattern: "user/{id}",
                defaults: new { controller = "Pages", action = "UserProfile" });
            app.MapControllers();

            app.UseSwagger();
            app.UseSwaggerUI(config =>
            {
                config.SwaggerEndpoint("v1/swagger.json", "API");
            });

            app.Run();
        }
    }
}