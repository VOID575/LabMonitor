using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Mvc;
using LabApi.Models;
using System.Runtime.CompilerServices;
using LabApi.Validators;  


namespace LabApi.Services;

// This service will be used to perform ore complex filter request
public class ModelConverter
{
    private readonly ObjectInspector<Container> _containerFieldInspector;
    
    public ModelConverter() => this._containerFieldInspector = new ObjectInspector<Container>();
    
    public readonly Dictionary<string, string> ContainerToDockerFieldMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "id", "id" },
        { "name", "name" },
        
        { "status", "status" },
        { "state", "status" }, 
        
        { "image", "ancestor" },
        
        { "network", "network" },
        { "volume", "volume" }
    };

    public readonly Dictionary<string, string> LabelFieldMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "projecthash", "com.docker.compose.config-hash" },
        { "projectname", "com.docker.compose.project" },
        { "service", "com.docker.compose.service" },
        { "version", "com.docker.compose.version" }
    };
    
    public List<Container> MapToDto(IList<ContainerListResponse> rawContainers)
    {
        return rawContainers.Select(container => new Container
        {
            Id = container.ID,
            Image = container.Image,
            Created = container.Created,
            Status = container.Status,
            Name = container.Names?.FirstOrDefault() ?? string.Empty,
            State = container.State,
            Ports = container.Ports,
            Labels = MapContainerResponseLabelsToContainerDtoLabels(container.Labels),
        }).ToList();
    }
    
    public ContainerLabel MapContainerResponseLabelsToContainerDtoLabels(IDictionary<string, string> labels)
    {
        return new ContainerLabel
        {
            ProjectHash = labels.TryGetValue(LabelFieldMap["projecthash"], out var hash) ? hash : string.Empty,
            ProjectName = labels.TryGetValue(LabelFieldMap["projectname"], out var project) ? project : string.Empty,
            Service = labels.TryGetValue(LabelFieldMap["service"], out var service) ? service : string.Empty,
            Version = labels.TryGetValue(LabelFieldMap["version"], out var version) ? version : string.Empty
        };
    }

    public string? ConvertLabelFieldToDockerLabel(string modelField)
    {
        if (string.IsNullOrWhiteSpace(modelField))
            return null;

        // accepter les formes "labels.projectHash" ou "projectHash"
        var candidate = modelField.StartsWith("labels.", StringComparison.OrdinalIgnoreCase)
            ? modelField.Substring("labels.".Length)
            : modelField;

        return LabelFieldMap.TryGetValue(candidate, out var dockerLabel) ? dockerLabel : null;
    }
    
    public Func<Container, string?>? GetContainerLabelAccessor(string modelField)
    {
        if (string.IsNullOrWhiteSpace(modelField))
            return null;

        var candidate = modelField.StartsWith("labels.", StringComparison.OrdinalIgnoreCase)
            ? modelField.Substring("labels.".Length)
            : modelField;

        return candidate.ToLowerInvariant() switch
        {
            "projecthash" => (Func<Container, string?>)(c => c.Labels?.ProjectHash),
            "projectname" => c => c.Labels?.ProjectName,
            "service" => c => c.Labels?.Service,
            "version" => c => c.Labels?.Version,
            _ => null
        };
    }

    public string? ConvertModelFieldToDockerField(string modelField)
    {
        if (string.IsNullOrWhiteSpace(modelField))
            return null;

        return ContainerToDockerFieldMap.TryGetValue(modelField, out var dockerField) ? dockerField : null;
    }
    
    // TODO : Move the converting from ContainerListResponse to Container here
}