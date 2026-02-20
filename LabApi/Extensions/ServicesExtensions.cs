using Docker.DotNet;
using LabApi.Services;
using LabApi.Services.docker;

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

        builder.Services.AddSingleton<HttpErrorCodeResolver>();
        builder.Services.AddScoped<ContainerLifeCycleManager>();
        builder.Services.AddScoped<ContainerResolver>();
        builder.Services.AddScoped<ContainerLogManager>();
    }
}