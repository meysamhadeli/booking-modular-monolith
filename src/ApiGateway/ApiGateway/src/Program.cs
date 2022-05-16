using Booking;
using Booking.Configuration;
using Booking.Data;
using Booking.Extensions;
using BuildingBlocks.Domain;
using BuildingBlocks.EFCore;
using BuildingBlocks.EventStoreDB;
using BuildingBlocks.IdsGenerator;
using BuildingBlocks.Jwt;
using BuildingBlocks.Logging;
using BuildingBlocks.Mapster;
using BuildingBlocks.MassTransit;
using BuildingBlocks.OpenTelemetry;
using BuildingBlocks.Swagger;
using BuildingBlocks.Web;
using Figgle;
using Flight;
using FluentValidation;
using Hellang.Middleware.ProblemDetails;
using Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Passenger;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var env = builder.Environment;

var appOptions = builder.Services.GetOptions<AppOptions>("AppOptions");
Console.WriteLine(FiggleFonts.Standard.Render(appOptions.Name));

builder.AddCustomSerilog();
builder.Services.AddJwt();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("Yarp"));

builder.Services.AddFlightModules(configuration);
builder.Services.AddIdentityModules(configuration);
builder.Services.AddPassengerModules(configuration);
builder.Services.AddBookingModules(configuration);

builder.Services.AddTransient<IBusPublisher, BusPublisher>();
builder.Services.AddCustomVersioning();
builder.Services.AddCustomOpenTelemetry();
builder.Services.AddTransient<AuthHeaderHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    var provider = app.Services.GetService<IApiVersionDescriptionProvider>();
    app.UseCustomSwagger(provider);
}

app.UseSerilogRequestLogging();
app.UseCorrelationId();
app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseProblemDetails();

app.UseFlightModules(configuration, env);
app.UseIdentityModules(configuration, env);
app.UsePassengerModules(configuration, env);
app.UseBookingModules(configuration, env);

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapReverseProxy(proxyPipeline =>
    {
        proxyPipeline.Use(async (context, next) =>
        {
            var token = await context.GetTokenAsync("access_token");
            context.Request.Headers["Authorization"] = $"Bearer {token}";

            await next().ConfigureAwait(false);
        });
    });
});

app.MapGet("/", x => x.Response.WriteAsync(appOptions.Name));

app.Run();