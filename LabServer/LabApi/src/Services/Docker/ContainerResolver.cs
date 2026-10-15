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
    private readonly ModelConverter _modelConverter;
    
    public ContainerResolver(IDockerClient dockerClient, ModelConverter modelConverter)
    {
        this._dockerClient = dockerClient;
        this._modelConverter = modelConverter;
    }

    public async Task<List<Container>> GetAllContainers()
    {
        var response = await this._dockerClient.Containers.ListContainersAsync(
            new ContainersListParameters() { All = true }
            );

        return response == null ? new List<Container>() : this._modelConverter.MapToDto(response).ToList();
    }
    
    public async Task<List<Container>> GetFilteredContainers(List<ContainerFilter> filters)
    {
        var dockerFilters = new Dictionary<string, IDictionary<string, bool>>();
        var containerFilters = new ObjectInspector<Container>();
        
        foreach (var filter in filters)
        {
            var field = filter.Field ?? string.Empty;

            string? labelKey = this._modelConverter.ConvertLabelFieldToDockerLabel(field);

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

            var dockerField = this._modelConverter.ConvertModelFieldToDockerField(field) ?? field.ToLowerInvariant();

            if (filter.Operator == ContainerFilterOperator.eq)
            {
                if (!dockerFilters.ContainsKey(dockerField))
                    dockerFilters[dockerField] = new Dictionary<string, bool>();

                dockerFilters[dockerField][filter.Value] = true;
            }
        }
        var parameters = new ContainersListParameters { All = true, Filters = dockerFilters };
        
        var rawContainers = await _dockerClient.Containers.ListContainersAsync(parameters);

        var mappedContainers = rawContainers == null ? 
            new List<Container>() : 
            this._modelConverter.MapToDto(rawContainers).ToList();

        foreach (var filter in filters)
        {
            var field = filter.Field ?? string.Empty;

            var labelAccessor = this._modelConverter.GetContainerLabelAccessor(field);
            if (labelAccessor != null)
            {
                mappedContainers = ApplyStringOperatorFilter(mappedContainers, labelAccessor, filter).ToList();
                continue;
            }

            if (filter.Operator == ContainerFilterOperator.contains ||
                filter.Operator == ContainerFilterOperator.startswith ||
                filter.Operator == ContainerFilterOperator.endswith ||
                filter.Operator == ContainerFilterOperator.eq)
            {
                if (field.Equals("name", StringComparison.OrdinalIgnoreCase))
                {
                    mappedContainers = ApplyStringOperatorFilter(mappedContainers, (Container c) => c.Name, filter).ToList();
                }
                else if (field.Equals("image", StringComparison.OrdinalIgnoreCase))
                {
                    mappedContainers = ApplyStringOperatorFilter(mappedContainers, (Container c) => c.Image, filter).ToList();
                }
                else if (field.Equals("id", StringComparison.OrdinalIgnoreCase))
                {
                    mappedContainers = ApplyStringOperatorFilter(mappedContainers, (Container c) => c.Id, filter).ToList();
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
            case ContainerFilterOperator.eq:
                return items.Where(c => string.Equals(accessor(c) ?? string.Empty, value, StringComparison.OrdinalIgnoreCase));
            default:
                return items;
        }
    }
}