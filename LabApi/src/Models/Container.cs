using Docker.DotNet;
using Docker.DotNet.Models;
using LabApi.Enum;

namespace LabApi.Models;

public class Container
{
    // Using init lock on the DTO to be immutable, which is a good practice for data transfer objects
    public string Id { get; init; }
    public string Image { get; init; }
    public DateTime Created { get; init; }
    public IList<Port> Ports { get; init; }
    public string Name { get; init; } 
    public string State { get; init; }
    public string Status { get; init; }
    public ContainerLabel Labels { get; init; }
}