using Docker.DotNet;
using Docker.DotNet.Models;
using System.Runtime.CompilerServices;
using System.IO.Pipes;

namespace LabApi.Services.docker;

// To test this feature on a local machine, you can use the following command to run a container that generates logs:
// docker run --rm --name test-logger alpine /bin/sh -c "while true; do echo '[INFO] Tout va bien...' ; sleep 2 ; echo '[ERROR] Attention, probleme' >&2 ; sleep 2 ; done"
public class ContainerLogManager
{
    private readonly IDockerClient _dockerClient;
    
    public ContainerLogManager(IDockerClient dockerClient)
    {
        this._dockerClient = dockerClient;
    }
    
    public async IAsyncEnumerable<string> StreamContainerLogsAsync(
        string containerId, 
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Using pipes to communicate between the Provider and the Consumer (SignalR Hub)
        using var pipeServer = new AnonymousPipeServerStream(PipeDirection.Out);
        using var pipeClient = new AnonymousPipeClientStream(PipeDirection.In, pipeServer.GetClientHandleAsString());

        // Creating another thread on which the Log Provider will run 
        Task producerTask = Task.Run(async () => 
            await RunLogProviderAsync(containerId, pipeServer, cancellationToken), cancellationToken);

        // Consuming the logs
        using var reader = new StreamReader(pipeClient);

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync();
            if (line != null)
            {
                yield return line;
            }
        }
    }
    
    private async Task RunLogProviderAsync(
        string containerId, 
        Stream targetStream, 
        CancellationToken token)
    {
        MultiplexedStream? dockerStream = null;

        try
        {
            dockerStream = await _dockerClient.Containers.GetContainerLogsAsync(containerId,
                false,
                new ContainerLogsParameters
                {
                    ShowStdout = true,
                    ShowStderr = true,
                    Follow = true,
                    Tail = "50"
                },
                token);

            // Demultiplexing the stream from Docker and writing it to the target stream (pipe)
            await dockerStream.CopyOutputToAsync(null, targetStream, targetStream, token);
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            // TODO: Add logging here
        }
        finally
        {
            // Closing pipes and the connexion to docker  
            targetStream.Dispose();
            dockerStream?.Dispose();
        }
    }
    
    // Enumerator Cancellation ?
    // using l60 ?
    // StreamReader ?
    // public async IAsyncEnumerable<string> StreamContainerLogsAsync(
    //     string containerId, 
    //     [EnumeratorCancellation] CancellationToken cancellationToken)
    // {
    //     var stream = await _dockerClient.Containers.GetContainerLogsAsync(containerId,
    //         false, // tty (mettre true si ton conteneur est en mode interactif)
    //         new ContainerLogsParameters
    //         {
    //             ShowStdout = true,
    //             ShowStderr = true,
    //             Follow = true,      
    //             Tail = "50"         
    //         },
    //         cancellationToken);     
    //     
    //     using var reader = new StreamReader(stream);
    //     
    //     while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
    //     {
    //         var line = await reader.ReadLineAsync(cancellationToken);
    //         
    //         if (line != null)
    //         {
    //             yield return line;
    //         }
    //     }
    // }
}
