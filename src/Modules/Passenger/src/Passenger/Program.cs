using BuildingBlocks.CAP;
using BuildingBlocks.Domain;
using BuildingBlocks.Exception;
using BuildingBlocks.Jwt;
using BuildingBlocks.Logging;
using BuildingBlocks.MediatR;
using BuildingBlocks.Swagger;
using BuildingBlocks.Web;
using Figgle;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace Passenger
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            Startup.ConfigureServices(builder);
            var app = builder.Build();
            Startup.Configure(app, app.Environment);
            var appOptions = app.Services.GetService<IOptions<AppOptions>>().Value;
            app.MapGet("/", x => x.Response.WriteAsync(appOptions.Name));
            app.Run();
        }
    }

    public static class Startup
    {
        public static void ConfigureServices(WebApplicationBuilder builder)
        {
            var services = builder.Services;
            var configuration = builder.Configuration;
            var env = builder.Environment;

            var appOptions = services.GetOptions<AppOptions>("AppOptions");
            Console.WriteLine(FiggleFonts.Standard.Render(appOptions.Name));

            builder.AddCustomSerilog(env);
            services.AddJwt();
            services.AddControllers();
            services.AddHttpContextAccessor();

            services.AddCustomCap();
            services.AddTransient<IBusPublisher, BusPublisher>();
            services.AddCustomVersioning();

            services.AddCustomSwagger(configuration,
                typeof(PassengerRoot).Assembly);

            services.AddCustomProblemDetails();

            services.AddPassengerModules(configuration);

            services.AddEasyCaching(options => { options.UseInMemory(configuration, "mem"); });

            services.AddCustomMediatR(
                typeof(PassengerRoot).Assembly
            );
        }

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                var provider = app.ApplicationServices.GetService<IApiVersionDescriptionProvider>();
                app.UseCustomSwagger(provider);
            }

            app.UseSerilogRequestLogging();
            app.UseCorrelationId();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseHttpsRedirection();

            app.UsePassengerModules();

            app.UseProblemDetails();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
