using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Mvc;
using LabApi.Models;
using System.Runtime.CompilerServices;

namespace LabApi.Services.docker;

public class ContainerLifeCycleManager
{
    private readonly IDockerClient _dockerClient;
    
    public ContainerLifeCycleManager(IDockerClient dockerClient)
    {
        this._dockerClient = dockerClient;
    }
    
    public async Task<bool> StartContainer(string id)
        => await this._dockerClient.Containers.StartContainerAsync(id, new ContainerStartParameters());
    
    
    public async Task<bool> StopContainer(string id)
        => await this._dockerClient.Containers.StopContainerAsync(id, new ContainerStopParameters());
    
}