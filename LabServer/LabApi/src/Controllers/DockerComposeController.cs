using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Mvc;
using LabApi.Services;
using LabApi.Models;
using LabApi.Services.docker;
using LabApi.Enum;

namespace LabApi.Controllers;

[ApiController]
[Route("api/docker-compose")]
public class DockerComposeController : ControllerBase   
{
    
    private readonly ComposeManager _composeManager;
    private readonly HttpErrorCodeResolver _httpErrorCodeResolver;
    
    public DockerComposeController(
        ComposeManager composeManager, 
        HttpErrorCodeResolver httpErrorCodeResolver
        )
    {
        this._composeManager = composeManager;
        this._httpErrorCodeResolver = httpErrorCodeResolver;
    }

    [HttpGet("startProject/{projectName}")]
    public async Task<IActionResult> StartContainers(string projectName)
    {
        try
        {
            await this._composeManager.StartDynamicStackAsync(projectName);
            return this._httpErrorCodeResolver.Resolve(Result.Success());
        }
        catch (Exception exception)
        {
            return this._httpErrorCodeResolver.Resolve(Result.Failure(exception));
        }
    }
    
    [HttpGet("stopProject/{projectName}")]
    public async Task<IActionResult> StopContainers(string projectName)
    {
        try
        {
            await this._composeManager.StopDynamicStackAsync(projectName);
            return this._httpErrorCodeResolver.Resolve(Result.Success());
        }
        catch (Exception exception)
        {
            return this._httpErrorCodeResolver.Resolve(Result.Failure(exception));
        }
    }
    
    [HttpGet("downProject/{projectName}")]
    public async Task<IActionResult> DownContainers(string projectName)
    {
        try
        {
            await this._composeManager.DownDynamicStackAsync(projectName);
            return this._httpErrorCodeResolver.Resolve(Result.Success());
        }
        catch (Exception exception)
        {
            return this._httpErrorCodeResolver.Resolve(Result.Failure(exception));
        }
    }
}