using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuarterlySales.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace QuarterlySales
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
            services.AddRouting(options => options.LowercaseUrls = true);

            services.AddMemoryCache();
            services.AddSession();

            services.AddControllersWithViews().AddNewtonsoftJson();

            services.AddHttpContextAccessor();
            services.AddTransient<IQuarterlySalesUnitOfWork, QuarterlySalesUnitOfWork>();

            services.AddTransient(typeof(IRepository<>), typeof(Repository<>));

            services.AddDbContext<SalesContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("SalesContext")));

            services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
            }).AddEntityFrameworkStores<SalesContext>()
                .AddDefaultTokenProviders();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "PagingSortingFiltering",
                    pattern: "{controller}/{action}/page/{pagenumber}/size/{pagesize}/sort/{sortfield}/{sortdirection}/filterby/{employee}/{year}/{quarter}");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}/{slug?}");
            });

            SalesContext.CreateAdminUser(app.ApplicationServices).Wait();
        }
    }
}
