using Microsoft.AspNetCore.SignalR;
using LabApi.Services.docker;
using System.Runtime.CompilerServices;

namespace LabApi.Hubs;

public class LogHub : Hub
{
    private readonly ContainerLogManager _containerLogManager;
    
    public LogHub(ContainerLogManager dockerService)
    {
        this._containerLogManager = dockerService;
    }
    
    public async IAsyncEnumerable<string> GetLogStream(
        string containerId, 
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var line in this._containerLogManager.StreamContainerLogsAsync(containerId, cancellationToken))
        {
            yield return line;
        }
    }
}