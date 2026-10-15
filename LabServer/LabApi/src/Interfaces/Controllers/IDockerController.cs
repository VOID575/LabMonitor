using LabApi.Models;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Mvc;

namespace LabApi.Interfaces.Controllers;

public interface IDockerController
{
    Task<List<Container>> GetAllContainersAsync();
    Task StopDynamicStackAsync(string projectName);
}