using Google.Apis.Sheets.v4;
using WordpressAdmin.API.Utilities;
using WordpressAdmin.API;

namespace WordpressAdmin.API;
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

    // Configure the HTTP request pipeline
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
