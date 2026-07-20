using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Mvc;
using LabApi.Services;
using LabApi.Models;
using LabApi.Services.docker;
using LabApi.Enum;
using LabApi.Interfaces;

namespace LabApi.Controllers
{
    [Route("api/docker")]
    public class DockerController : ControllerBase 
    {
        private readonly IDockerClient _dockerClient;
        private readonly ContainerLifeCycleManager _containerLifeCycleManager;
        private readonly ContainerResolver _containerResolver;
        private readonly ContainerLogManager _containerLogManager;
        private readonly HttpErrorCodeResolver _httpErrorCodeResolver;

        public DockerController(IDockerClient dockerClient, 
            ContainerLifeCycleManager containerLifeCycleManager, 
            ContainerLogManager containerLogManager,
            ContainerResolver containerResolver,
            HttpErrorCodeResolver httpErrorCodeResolver
        )
        {
            this._dockerClient = dockerClient;
            this._containerLifeCycleManager = containerLifeCycleManager;
            this._containerLogManager = containerLogManager;
            this._containerResolver = containerResolver;
            this._httpErrorCodeResolver = httpErrorCodeResolver;
        }

        [HttpGet("containers")]
        public async Task<IActionResult> GetAllContainers()
        {   
            try
            {
                var containers = await this._containerResolver.GetAllContainers();
                return this._httpErrorCodeResolver.Resolve(Result<List<Container>>.Success(containers));
            }
            catch (Exception exception)
            {
                return this._httpErrorCodeResolver.Resolve(Result.Failure(exception));
            }
        }

        [HttpGet("containersRaw")]
        public async Task<IActionResult> GetContainers()
        {   
            try
            {
                var containers = await this._dockerClient.Containers.ListContainersAsync(new ContainersListParameters() { All = true });
                return this._httpErrorCodeResolver.Resolve(Result<IList<ContainerListResponse>>.Success(containers));
            }
            catch (Exception exception)
            {
                return this._httpErrorCodeResolver.Resolve(Result.Failure(exception));
            }
        }
        
        [HttpGet("containersById/{id}")]
        public async Task<IActionResult> GetContainerById(string id)
        {   
            try
            {
                var filter = new ContainerFilter() { Field = "id", Operator = ContainerFilterOperator.eq, Value = id };
                var containers = await this._containerResolver.GetFilteredContainers(new List<ContainerFilter> { filter });

                if (containers == null || containers.Count == 0)
                    return this._httpErrorCodeResolver.Resolve(Result.Failure(new KeyNotFoundException($"Container '{id}' not found")));

                return this._httpErrorCodeResolver.Resolve(Result<Container>.Success(containers.First()));
            }
            catch (Exception exception)
            {
                return this._httpErrorCodeResolver.Resolve(Result.Failure(exception));
            }
        }
        
        [HttpGet("containersByProjectName/{projectName}")]
        public async Task<IActionResult> GetContainersByProjectName(string projectName)
        {   
            try
            {
                var filter = new ContainerFilter() { Field = "projectName", Operator = ContainerFilterOperator.eq, Value = projectName };
                var containers = await this._containerResolver.GetFilteredContainers(new List<ContainerFilter> { filter });
                
                // Can be moved in an appropriate function or method
                // Used to clear ghost containers
                containers.RemoveAll(container => container.Labels.ProjectHash.Equals(""));
                
                return this._httpErrorCodeResolver.Resolve(Result<List<Container>>.Success(containers));
            }
            catch (Exception exception)
            {
                return this._httpErrorCodeResolver.Resolve(Result.Failure(exception));
            }
        }
        
        [HttpPost("startContainer/{id}")]
        public async Task<IActionResult> StartContainer(string id)
        {
            try
            {
                await this._containerLifeCycleManager.StartContainer(id);
                return this._httpErrorCodeResolver.Resolve(Result.Success());
            }
            catch (Exception exception)
            {
                return this._httpErrorCodeResolver.Resolve(Result.Failure(exception));
            }
        }

        [HttpPost("stopContainer/{id}")]
        public async Task<IActionResult> StopContainer(string id)
        {
            try
            {
                await this._containerLifeCycleManager.StopContainer(id);
                return this._httpErrorCodeResolver.Resolve(Result.Success());
            }
            catch (Exception exception)
            {
                return this._httpErrorCodeResolver.Resolve(Result.Failure(exception));
            }
        }
    }
}
