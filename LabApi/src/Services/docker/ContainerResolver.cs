using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Mvc;
using LabApi.Models;
using LabApi.Validators;
using LabApi.Enum;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using LabApi.Enum;
using LabApi.Models;

namespace LabApi.Services.docker;

public class ContainerResolver
{
    private readonly IDockerClient _dockerClient;
    
    public ContainerResolver(IDockerClient dockerClient)
    {
        this._dockerClient = dockerClient;
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
    
    // public async Task<List<Container>> GetFilteredContainers(List<ContainerFilter> filters)
    // {
    //     var dockerFilters = new Dictionary<string, IDictionary<string, bool>>();
    //
    //     var statusFilters = filters.Where(f => f.Field.ToLower() == "status" && f.Operator == ContainerFilterOperator.eq);
    //     foreach (var filter in statusFilters)
    //     {
    //         if (!dockerFilters.ContainsKey("status"))
    //             dockerFilters["status"] = new Dictionary<string, bool>();
    //         
    //         dockerFilters["status"].Add(filter.Value, true);
    //     }
    //
    //     var parameters = new ContainersListParameters { All = true, Filters = dockerFilters };
    //
    //     var rawContainers = await _dockerClient.Containers.ListContainersAsync(parameters);
    //
    //     var mappedContainers = rawContainers.Select(c => MapToDto(c)).AsQueryable();
    //
    //     var nameContainsFilters = filters.Where(f => f.Field.ToLower() == "name" && f.Operator == "contains");
    //     foreach (var filter in nameContainsFilters)
    //     {
    //         mappedContainers = mappedContainers.Where(c => c.Name.Contains(filter.Value, StringComparison.OrdinalIgnoreCase));
    //     }
    //
    //     return mappedContainers.ToList();
    // }
    
    public async Task<List<Container>> GetFilteredContainers(List<ContainerFilter> filters)
    {
        var dockerFilters = new Dictionary<string, IDictionary<string, bool>>();
        var containerFilters = new ObjectInspector<Container>();
        
        foreach (var filter in filters)
        {
            var field = filter.Field ?? string.Empty;

            string? labelKey = ModelConverter.ConvertLabelFieldToDockerLabel(field);

            if (labelKey != null)
            {
                if (filter.Operator == ContainerFilterOperator.eq)
                {
                    if (!dockerFilters.ContainsKey("label"))
                        dockerFilters["label"] = new Dictionary<string, bool>();

                    dockerFilters["label"][
                        $"{labelKey}={filter.Value}"] = true;
                    continue;
                }
            }

            var dockerField = ModelConverter.ConvertModelFieldToDockerField(field) ?? field.ToLowerInvariant();

            if (filter.Operator == ContainerFilterOperator.eq)
            {
                if (!dockerFilters.ContainsKey(dockerField))
                    dockerFilters[dockerField] = new Dictionary<string, bool>();

                dockerFilters[dockerField][filter.Value] = true;
            }
        }
        var parameters = new ContainersListParameters { All = true, Filters = dockerFilters };
        
        var rawContainers = await _dockerClient.Containers.ListContainersAsync(parameters);

        var mappedContainers = rawContainers.Select(container => new Container
        {
            Id = container.ID,
            Image = container.Image,
            Created = container.Created,
            Status = container.Status,
            Name = container.Names?.FirstOrDefault() ?? string.Empty,
            State = container.State,
            Ports = container.Ports,
            Labels = MapLabels(container.Labels)
        }).ToList();

        foreach (var filter in filters)
        {
            var field = filter.Field ?? string.Empty;

            var labelAccessor = ModelConverter.GetContainerLabelAccessor(field);
            if (labelAccessor != null)
            {
                mappedContainers = ApplyStringOperatorFilter(mappedContainers, labelAccessor, filter).ToList();
                continue;
            }

            if (filter.Operator == ContainerFilterOperator.contains ||
                filter.Operator == ContainerFilterOperator.startswith ||
                filter.Operator == ContainerFilterOperator.endswith)
            {
                if (field.Equals("name", StringComparison.OrdinalIgnoreCase))
                {
                    mappedContainers = ApplyStringOperatorFilter(mappedContainers, (Container c) => c.Name, filter).ToList();
                }
                else if (field.Equals("image", StringComparison.OrdinalIgnoreCase))
                {
                    mappedContainers = ApplyStringOperatorFilter(mappedContainers, (Container c) => c.Image, filter).ToList();
                }
            }
        }

        return mappedContainers;
    }

    private static IEnumerable<Container> ApplyStringOperatorFilter(IEnumerable<Container> items, Func<Container, string?> accessor, ContainerFilter filter)
    {
        if (filter == null || accessor == null)
            return items;

        var value = filter.Value ?? string.Empty;
        switch (filter.Operator)
        {
            case ContainerFilterOperator.contains:
                return items.Where(c => (accessor(c) ?? string.Empty).Contains(value, StringComparison.OrdinalIgnoreCase));
            case ContainerFilterOperator.startswith:
                return items.Where(c => (accessor(c) ?? string.Empty).StartsWith(value, StringComparison.OrdinalIgnoreCase));
            case ContainerFilterOperator.endswith:
                return items.Where(c => (accessor(c) ?? string.Empty).EndsWith(value, StringComparison.OrdinalIgnoreCase));
            default:
                return items;
        }
    }
}