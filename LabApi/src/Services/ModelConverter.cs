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
    
    private static readonly Dictionary<string, string> ContainerToDockerFieldMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "id", "id" },
        { "name", "name" },
        
        { "status", "status" },
        { "state", "status" }, 
        
        { "image", "ancestor" },
        
        { "network", "network" },
        { "volume", "volume" }
    };

    private static readonly Dictionary<string, string> LabelFieldMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "projecthash", "com.docker.compose.config-hash" },
        { "projectname", "com.docker.compose.project" },
        { "service", "com.docker.compose.service" },
        { "version", "com.docker.compose.version" }
    };

    // Retourne la clé Docker pour un champ de label du modèle (ex: "projectHash" -> "com.docker.compose.config-hash")
    public static string? ConvertLabelFieldToDockerLabel(string modelField)
    {
        if (string.IsNullOrWhiteSpace(modelField))
            return null;

        // accepter les formes "labels.projectHash" ou "projectHash"
        var candidate = modelField.StartsWith("labels.", StringComparison.OrdinalIgnoreCase)
            ? modelField.Substring("labels.".Length)
            : modelField;

        return LabelFieldMap.TryGetValue(candidate, out var dockerLabel) ? dockerLabel : null;
    }

    // Retourne un accesseur qui récupère la valeur de label correspondant sur un DTO Container
    // Ex: "projectHash" -> c => c.Labels?.ProjectHash
    public static Func<Container, string?>? GetContainerLabelAccessor(string modelField)
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

    public static string? ConvertModelFieldToDockerField(string modelField)
    {
        if (string.IsNullOrWhiteSpace(modelField))
            return null;

        return ContainerToDockerFieldMap.TryGetValue(modelField, out var dockerField) ? dockerField : null;
    }
    
    // TODO : Move the converting from ContainerListResponse to Container here
}