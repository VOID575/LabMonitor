using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Mvc;
using LabApi.Models;
using System.Runtime.CompilerServices;


namespace LabApi.Validators;

public class ObjectInspector<T>
{
    private static readonly HashSet<string> ValidFields = new(
        typeof(T).GetProperties().Select(p => p.Name), 
        StringComparer.OrdinalIgnoreCase);

    public bool IsValidFilterField(string fieldName)
    {
        return ValidFields.Contains(fieldName);
    }
    
    public HashSet<string> GetValidFilterField(string fieldName)
    {
        return ValidFields;
    }
}