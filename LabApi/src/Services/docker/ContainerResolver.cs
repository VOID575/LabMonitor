using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Mvc;
using LabApi.Models;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace LabApi.Services.docker;

public class ContainerResolver
{
    private readonly IDockerClient _dockerClient;
    
    public ContainerResolver(IDockerClient dockerClient)
    {
        this._dockerClient = dockerClient;
    }

    public async Task<List<Container>> GetAllContainers()
    {
        var response = await this._dockerClient.Containers.ListContainersAsync(new ContainersListParameters() { All = true });
        
        var containers = response.Select(container => new Container
        {
            Id = container.ID,
            Image = container.Image,
            Created = container.Created,
            Status = container.Status,
            Name = container.Names[0],
            State = container.State,
            Ports = container.Ports,
            Labels = MapLabels(container.Labels)
        }).ToList();
        

        return containers;
    }
    
    private static ContainerLabel MapLabels(IDictionary<string, string> labels)
    {
        return new ContainerLabel
        {
            ProjectHash = labels.TryGetValue("com.docker.compose.config-hash", out var hash) ? hash : string.Empty,
            ProjectName = labels.TryGetValue("com.docker.compose.project", out var project) ? project : string.Empty,
            Service = labels.TryGetValue("com.docker.compose.service", out var service) ? service : string.Empty,
            Version = labels.TryGetValue("com.docker.compose.version", out var version) ? version : string.Empty
        };
    }
}