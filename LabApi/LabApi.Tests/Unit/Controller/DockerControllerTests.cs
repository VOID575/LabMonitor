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

namespace LabApi.tests.Unit.Controller;

public class DockerControllerTests
{
    [Fact]
    public async Task GetContainers_should_return_only_non_orphan_containers_always()
    {
        // Arrange 
        var mockContainerResolver = new Mock<IContainerOperations>();
        var mockDockerClient = new Mock<IDockerClient>();
        
        mockDockerClient.Setup(c => c.Containers).Returns(mockContainerResolver.Object);

        var fakeContainers = new List<ContainerListResponse>
        {
            new ContainerListResponse()
            {
                ID = "123", 
                Names = ["nginx-web",""], 
                State = "running", 
                Labels = new Dictionary<string, string>()
                {
                    { "ProjectName", "sae4" }
                }
            },new ContainerListResponse()
            {   
                ID = "456", 
                Names = ["nginx-db",""], 
                State = "running", 
                Labels = new Dictionary<string, string>()
                {
                    { "ProjectName", "sae4" }
                }
            },
        };

    mockContainerResolver
        .Setup(op => op.ListContainersAsync(
            It.IsAny<ContainersListParameters>(), 
            It.IsAny<CancellationToken>() // Très important pour Docker.DotNet !
        ))
    .ReturnsAsync(fakeContainers);

    var controller = new ContainerResolver(mockDockerClient.Object);

        // Act
        var filters = new List<ContainerFilter>()
        {
            new ContainerFilter()
            { 
                Field = "ID",  
                Value = "123",
                Operator = ContainerFilterOperator.eq
            } 
        };
        
        // Act
        var result = await controller.GetFilteredContainers(filters);

        // Assert
        // var okResult = Assert.IsType<OkObjectResult>(result);
        //
        // var returnedList = Assert.IsType<List<Container>>(okResult.Value);
        
        Assert.Single(result);
    }
}