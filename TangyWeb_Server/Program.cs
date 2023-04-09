using Microsoft.EntityFrameworkCore;
using Tangy_Business.Repository;
using Tangy_Business.Repository.IRepository;
using Tangy_DataAccess.Data;
using TangyWeb_Server.Data;
using TangyWeb_Server.Service;
using TangyWeb_Server.Service.IService;
using Syncfusion.Blazor;
using Microsoft.AspNetCore.Identity;

namespace TangyWeb_Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddSyncfusionBlazor();

            builder.Services.AddSingleton<WeatherForecastService>();
            builder.Services.AddDbContext<ApplicationDbContext>(options => 
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
            );

            builder.Services.AddIdentity<IdentityUser,IdentityRole>().AddDefaultTokenProviders().AddDefaultUI()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IProductPriceRepository, ProductPriceRepository>();
            builder.Services.AddScoped<IDbInitializer, DbInitializer>();
            builder.Services.AddScoped<IFileUpload, FileUpload>();

            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            var app = builder.Build();

            //Register Syncfusion license
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MTQ3NDA5OEAzMjMxMmUzMTJlMzMzNU9xR2prYWxodEx5OUJlZEJLVElNNkNnMWc2ek9MOFZQMTd3Y1M1aHRNc3c9");

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            //using (var scope = app.Services.CreateScope())
            //{
            //    var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
            //    dbInitializer.Initialize();
            //}

            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");
            app.UseAuthentication();
            app.UseAuthorization();

            app.Run();
        }
    }
}