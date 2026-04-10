using Docker.DotNet;
using LabApi.Services;
using LabApi.Services.docker;
using JsonStringEnumConverter = System.Text.Json.Serialization.JsonStringEnumConverter;
using JsonNamingPolicy = System.Text.Json.JsonNamingPolicy;

namespace LabApi.Extensions;

public static class ServicesExtensions
{
    public static void AddAllServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers(); 
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddSignalR();

        builder.Services.AddSingleton<IDockerClient>(sp =>
        {
            return new DockerClientConfiguration(
                    new Uri("unix:///var/run/docker.sock"))
                .CreateClient();
        });
        
        // Avoid CORS issues when the Angular frontend tries to access the API
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAngularFrontend",
                policy =>
                {
                    policy.WithOrigins("http://localhost:4200") 
                        .AllowAnyHeader()                     
                        .AllowAnyMethod()                     
                        .AllowCredentials();                  
                });
        });

        builder.Services.AddSingleton<HttpErrorCodeResolver>();
        builder.Services.AddScoped<ContainerLifeCycleManager>();
        builder.Services.AddScoped<ContainerResolver>();
        builder.Services.AddScoped<ContainerLogManager>();
        builder.Services.AddControllers()
            .AddJsonOptions(o =>
                o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase))); // We can precise the type of case ?!
        
    }
}