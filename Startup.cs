using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Google.Apis.Sheets.v4;
using Google.Apis.Services;
using WordpressAdminApi.Utilities;

namespace WordpressAdminApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Bind GoogleSheetsConfig and AppConfig to configuration sections
            services.Configure<GoogleSheetsConfig>(Configuration.GetSection("GoogleSheetsConfig"));
            services.Configure<LocalServerConfig>(Configuration.GetSection("AppConfig"));

            // Add the Google Sheets Service
            services.AddSingleton<SheetsService>(sp =>
            {
                var apiKeyPath = Configuration["GoogleSheetsConfig:ApiKeyPath"];
                var serviceInitializer = new SheetsServiceInitializer(apiKeyPath);
                return serviceInitializer.InitializeSheetsService();
            });

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
