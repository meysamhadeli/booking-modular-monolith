using Booking;
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
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var env = builder.Environment;

var appOptions = builder.Services.GetOptions<AppOptions>("AppOptions");
Console.WriteLine(FiggleFonts.Standard.Render(appOptions.Name));

builder.AddCustomSerilog(env);
builder.Services.AddJwt();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

builder.Services.AddCustomCap();
builder.Services.AddTransient<IBusPublisher, BusPublisher>();
builder.Services.AddCustomVersioning();

builder.Services.AddCustomSwagger(configuration,
    typeof(BookingRoot).Assembly);

builder.Services.AddCustomProblemDetails();

builder.Services.AddBookingModules(configuration);

builder.Services.AddEasyCaching(options => { options.UseInMemory(configuration, "mem"); });

builder.Services.AddCustomMediatR(
    typeof(BookingRoot).Assembly
);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    var provider = app.Services.GetService<IApiVersionDescriptionProvider>();
    app.UseCustomSwagger(provider);
}

app.UseSerilogRequestLogging();
app.UseCorrelationId();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();

app.UseBookingModules(env);
app.UseProblemDetails();

app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

app.MapGet("/", x => x.Response.WriteAsync(appOptions.Name));

app.Run();