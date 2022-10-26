using Dotmim.Sync;
using Dotmim.Sync.Enumerations;
using Dotmim.Sync.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVC_Core
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
            services.AddControllersWithViews();

            services.AddCors(opt =>
            {
                opt.AddPolicy("CorsPolicy", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().AllowCredentials().Build());
            });

            // [Required]: To be able to handle multiple sessions
            services.AddDistributedMemoryCache();
            services.AddSession(options => options.IdleTimeout = TimeSpan.FromMinutes(60));


            // [Required]: Get a connection string to your server data source
            string connectionString = Configuration.GetSection("ConnectionStrings")["DefaultConnection"];

            var options = new SyncOptions
            {
                SnapshotsDirectory = "C:\\Tmp\\Snapshots",
                BatchSize = 2000,
            };



            string[] tables = new string[] { "Test" };

            SyncSetup setup = new SyncSetup(tables)
            {
                //// optional :
                //StoredProceduresPrefix = "s",
                //StoredProceduresSuffix = "",
                //TrackingTablesPrefix = "s",
                //TrackingTablesSuffix = ""
            };


            // [Required]: Add a SqlSyncProvider acting as the server hub.
            services.AddSyncServer<SqlSyncChangeTrackingProvider>(connectionString, setup, options);
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
            }
            app.UseStaticFiles();

            app.UseRouting();
            app.UseSession();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
