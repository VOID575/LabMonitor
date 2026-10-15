using Docker.DotNet;
using Docker.DotNet.Models;
using LabApi.Enum;

namespace LabApi.Models;

public class Container
{
    // Using init lock on the DTO to be immutable, which is a good practice for data transfer objects
    public required string Id { get; init; }
    public required string Image { get; init; }
    public required DateTime Created { get; init; }
    public required IList<Port> Ports { get; init; }
    public required string Name { get; init; }
    public required string State { get; init; }
    public required string Status { get; init; }
    public required ContainerLabel Labels { get; init; }
}