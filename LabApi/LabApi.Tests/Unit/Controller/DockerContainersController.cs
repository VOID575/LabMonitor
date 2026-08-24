using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using LabApi.Models;
using LabApi.Controllers;
using LabApi.Enum;
using LabApi.Interfaces.Controllers;
using LabApi.Services.docker;
using ContainerResolver = LabApi.Services.docker.ContainerResolver;
using LabApi.Services;

namespace LabApi.tests.Unit.Controller;

public class DockerContainersControllerTests
{
    private List<ContainerListResponse> GetFakeContainers()
    {
        return new List<ContainerListResponse>
        {
            new ContainerListResponse()
            {
                ID = "123", 
                Names = ["nginx-web", ""], 
                Image = "nginx:alpine",
                State = "running", 
                Labels = new Dictionary<string, string>() { { "ProjectName", "sae4" } }
            },
            new ContainerListResponse()
            {   
                ID = "456", 
                Names = ["nginx-db", ""], 
                Image = "postgres:13",
                State = "running", 
                Labels = new Dictionary<string, string>() { { "ProjectName", "sae4" } }
            }
        };
    }

    private DockerContainersController SetupMockedController()
    {
        var mockContainerResolver = new Mock<ContainerResolver>(MockBehavior.Strict, new Mock<IDockerClient>().Object);
        mockContainerResolver
            .Setup(op => op.GetAllContainers())
            .ReturnsAsync(GetFakeContainers().Select(c => new Container
            {
                Id = c.ID,
                Name = c.Names[0],
                Image = c.Image,
                State = c.State,
                Labels = new ContainerLabel { ProjectName = c.Labels["ProjectName"] }
            }).ToList());

        var mockDockerClient = new Mock<IDockerClient>();
        var mockContainerLifeCycleManager = new Mock<ContainerLifeCycleManager>(mockDockerClient.Object);
        var mockContainerLogManager = new Mock<ContainerLogManager>(mockDockerClient.Object);
        var mockHttpErrorCodeResolver = new Mock<HttpErrorCodeResolver>();

        return new DockerContainersController(
            mockDockerClient.Object,
            mockContainerLifeCycleManager.Object,
            mockContainerLogManager.Object,
            mockContainerResolver.Object,
            mockHttpErrorCodeResolver.Object
        );
    }
    
    
}