using LabApi.Enum;

namespace LabApi.Models;

public record ContainerFilter
{
    public required string Field { get; init; }
    public required string Value { get; init; }
    public required ContainerFilterOperator Operator { get; init; } 
}   