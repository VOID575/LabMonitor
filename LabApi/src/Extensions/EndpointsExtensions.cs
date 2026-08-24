namespace LabApi.Extensions;

public static class EndpointExtensions
{
    public static void MapAllHubs(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHub<Hubs.LogHub>("/hubs/docker/logs");
    }
}