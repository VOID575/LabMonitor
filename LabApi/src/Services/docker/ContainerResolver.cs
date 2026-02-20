using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Mvc;
using LabApi.Models;
using System.Runtime.CompilerServices;

namespace LabApi.Services.docker;

public class ContainerResolver
{
    private readonly IDockerClient _dockerClient;
    
    public ContainerResolver(IDockerClient dockerClient)
    {
        this._dockerClient = dockerClient;
    }

    public async Task<IList<ContainerListResponse>> GetAllContainers()
        => await this._dockerClient.Containers.ListContainersAsync(new ContainersListParameters() { All = true });
}