using Docker.DotNet;
using Docker.DotNet.Models;
using Ductus.FluentDocker.Builders;

public class ComposeManager
{
    private readonly DockerClient _dockerClient;

    public ComposeManager(DockerClient dockerClient)
    {
        _dockerClient = dockerClient;
    }

    public async Task StartDynamicStackAsync(string projectName)
    {
        // 1. On interroge l'API Docker pour trouver n'importe quel conteneur de ce projet
        // (Même s'il est arrêté, All = true permet de le trouver !)
        var parameters = new ContainersListParameters
        {
            All = true,
            Filters = new Dictionary<string, IDictionary<string, bool>>
            {
                { "label", new Dictionary<string, bool> { { $"com.docker.compose.project={projectName}", true } } }
            }
        };

        var containers = await _dockerClient.Containers.ListContainersAsync(parameters);
        var targetContainer = containers.FirstOrDefault();

        if (targetContainer == null)
            throw new Exception($"Aucun conteneur trouvé pour le projet '{projectName}'.");

        // 2. On extrait les labels magiques générés par Compose
        targetContainer.Labels.TryGetValue("com.docker.compose.project.working_dir", out var workingDir);
        targetContainer.Labels.TryGetValue("com.docker.compose.project.config_files", out var configFile);

        if (string.IsNullOrEmpty(workingDir) || string.IsNullOrEmpty(configFile))
            throw new Exception("Les labels Docker Compose sont manquants sur ce conteneur.");

        // 3. On assemble le VRAI chemin absolu de ta machine !
        // Ex: "/home/user/media-stack/docker-compose.yml"
        var fullPath = Path.Combine(workingDir, configFile);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Le fichier Compose est introuvable sur le disque : {fullPath}");

        // 4. On lance l'artillerie lourde avec FluentDocker
        // Ça marchera du premier coup car l'API est sur l'hôte !
        new Builder()
            .UseContainer()
            .UseCompose()
            .FromFile(fullPath)
            .RemoveOrphans()
            .Build()
            .Start();
    }
}