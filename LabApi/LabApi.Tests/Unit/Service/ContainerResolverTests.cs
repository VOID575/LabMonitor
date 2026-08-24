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

namespace LabApi.tests.Unit.Service;

public class ContainerResolverTests
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

    private ContainerResolver SetupMockedService()
    {
        var mockContainerResolver = new Mock<IContainerOperations>();
        var mockDockerClient = new Mock<IDockerClient>();
        
        mockDockerClient.Setup(c => c.Containers).Returns(mockContainerResolver.Object);

        mockContainerResolver
            .Setup(op => op.ListContainersAsync(
                It.IsAny<ContainersListParameters>(), 
                It.IsAny<CancellationToken>() 
            ))
            .ReturnsAsync(GetFakeContainers());

        return new ContainerResolver(mockDockerClient.Object);
    }

    #region Tests pour le champ "id"

    [Fact]
    public async Task GetFilteredContainers_should_return_filter_containers_by_id_eq_always()
    {
        // Arrange
        var service = SetupMockedService();
        var filters = new List<ContainerFilter>() { new ContainerFilter() { Field = "id", Value = "123", Operator = ContainerFilterOperator.eq } };
        
        // Act
        var result = await service.GetFilteredContainers(filters);

        // Assert
        Assert.Single(result);
        Assert.Equal("nginx-web", result[0].Name);
    }

    [Fact]
    public async Task GetFilteredContainers_should_return_filter_containers_by_id_contains_always()
    {
        // Arrange
        var service = SetupMockedService();
        var filters = new List<ContainerFilter>() { new ContainerFilter() { Field = "id", Value = "2", Operator = ContainerFilterOperator.contains } };
        
        // Act
        var result = await service.GetFilteredContainers(filters);

        // Assert
        Assert.Single(result);
        Assert.Equal("nginx-web", result[0].Name);
    }

    [Fact]
    public async Task GetFilteredContainers_should_return_filter_containers_by_id_startswith_always()
    {
        // Arrange
        var service = SetupMockedService();
        var filters = new List<ContainerFilter>() { new ContainerFilter() { Field = "id", Value = "12", Operator = ContainerFilterOperator.startswith } };
        
        // Act
        var result = await service.GetFilteredContainers(filters);

        // Assert
        Assert.Single(result);
        Assert.Equal("nginx-web", result[0].Name);
    }

    [Fact]
    public async Task GetFilteredContainers_should_return_filter_containers_by_id_endswith_always()
    {
        // Arrange
        var service = SetupMockedService();
        var filters = new List<ContainerFilter>() { new ContainerFilter() { Field = "id", Value = "23", Operator = ContainerFilterOperator.endswith } };
        
        // Act
        var result = await service.GetFilteredContainers(filters);

        // Assert
        Assert.Single(result);
        Assert.Equal("nginx-web", result[0].Name);
    }

    #endregion

    #region Tests pour le champ "name"

    [Fact]
    public async Task GetFilteredContainers_should_return_filter_containers_by_name_eq_always()
    {
        // Arrange
        var service = SetupMockedService();
        var filters = new List<ContainerFilter>() { new ContainerFilter() { Field = "name", Value = "nginx-db", Operator = ContainerFilterOperator.eq } };
        
        // Act
        var result = await service.GetFilteredContainers(filters);

        // Assert
        Assert.Single(result);
        Assert.Equal("nginx-db", result[0].Name);
    }

    [Fact]
    public async Task GetFilteredContainers_should_return_filter_containers_by_name_contains_always()
    {
        // Arrange
        var service = SetupMockedService();
        var filters = new List<ContainerFilter>() { new ContainerFilter() { Field = "name", Value = "db", Operator = ContainerFilterOperator.contains } };
        
        // Act
        var result = await service.GetFilteredContainers(filters);

        // Assert
        Assert.Single(result);
        Assert.Equal("nginx-db", result[0].Name);
    }

    [Fact]
    public async Task GetFilteredContainers_should_return_filter_containers_by_name_startswith_always()
    {
        // Arrange
        var service = SetupMockedService();
        var filters = new List<ContainerFilter>() { new ContainerFilter() { Field = "name", Value = "nginx-d", Operator = ContainerFilterOperator.startswith } };
        
        // Act
        var result = await service.GetFilteredContainers(filters);

        // Assert
        Assert.Single(result);
        Assert.Equal("nginx-db", result[0].Name);
    }

    [Fact]
    public async Task GetFilteredContainers_should_return_filter_containers_by_name_endswith_always()
    {
        // Arrange
        var service = SetupMockedService();
        var filters = new List<ContainerFilter>() { new ContainerFilter() { Field = "name", Value = "-db", Operator = ContainerFilterOperator.endswith } };
        
        // Act
        var result = await service.GetFilteredContainers(filters);

        // Assert
        Assert.Single(result);
        Assert.Equal("nginx-db", result[0].Name);
    }

    #endregion

    #region Tests pour le champ "image"

    [Fact]
    public async Task GetFilteredContainers_should_return_filter_containers_by_image_eq_always()
    {
        // Arrange
        var service = SetupMockedService();
        var filters = new List<ContainerFilter>() { new ContainerFilter() { Field = "image", Value = "nginx:alpine", Operator = ContainerFilterOperator.eq } };
        
        // Act
        var result = await service.GetFilteredContainers(filters);

        // Assert
        Assert.Single(result);
        Assert.Equal("nginx-web", result[0].Name);
    }

    [Fact]
    public async Task GetFilteredContainers_should_return_filter_containers_by_image_contains_always()
    {
        // Arrange
        var service = SetupMockedService();
        var filters = new List<ContainerFilter>() { new ContainerFilter() { Field = "image", Value = "alpine", Operator = ContainerFilterOperator.contains } };
        
        // Act
        var result = await service.GetFilteredContainers(filters);

        // Assert
        Assert.Single(result);
        Assert.Equal("nginx-web", result[0].Name);
    }

    [Fact]
    public async Task GetFilteredContainers_should_return_filter_containers_by_image_startswith_always()
    {
        // Arrange
        var service = SetupMockedService();
        var filters = new List<ContainerFilter>() { new ContainerFilter() { Field = "image", Value = "nginx", Operator = ContainerFilterOperator.startswith } };
        
        // Act
        var result = await service.GetFilteredContainers(filters);

        // Assert
        Assert.Single(result);
        Assert.Equal("nginx-web", result[0].Name); 
    }

    [Fact]
    public async Task GetFilteredContainers_should_return_filter_containers_by_image_endswith_always()
    {
        // Arrange
        var service = SetupMockedService();
        var filters = new List<ContainerFilter>() { new ContainerFilter() { Field = "image", Value = "13", Operator = ContainerFilterOperator.endswith } };
        
        // Act
        var result = await service.GetFilteredContainers(filters);

        // Assert
        Assert.Single(result);
        Assert.Equal("nginx-db", result[0].Name); 
    }

    #endregion
}