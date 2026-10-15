using System.Diagnostics;
using Docker.DotNet;
using Docker.DotNet.Models;
using Ductus.FluentDocker.Builders;

namespace LabApi.Services;

public class ComposeManager
{
    private readonly DockerClient _dockerClient;

    public ComposeManager(DockerClient dockerClient)
    {
        _dockerClient = dockerClient;
    }
    
    private async Task<string> GetComposeFilePathAsync(string projectName)
    {
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
            throw new Exception($"Aucun conteneur existant trouvé pour le projet '{projectName}'. Impossible de localiser le fichier YAML.");

        targetContainer.Labels.TryGetValue("com.docker.compose.project.working_dir", out var workingDir);
        targetContainer.Labels.TryGetValue("com.docker.compose.project.config_files", out var configFile);

        if (string.IsNullOrEmpty(workingDir) || string.IsNullOrEmpty(configFile))
            throw new Exception("Les labels de chemin Docker Compose sont manquants sur ce conteneur.");

        var fullPath = Path.Combine(workingDir, configFile);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Le fichier Compose est introuvable sur le disque : {fullPath}");

        return fullPath;
    }

    public async Task StartDynamicStackAsync(string projectName)
    {
        var fullPath = await this.GetComposeFilePathAsync(projectName);
        
        new Builder()
            .UseContainer()
            .UseCompose()
            .FromFile(fullPath)
            .RemoveOrphans()
            .Build()
            .Start();
    }
    
    public async Task StopDynamicStackAsync(string projectName)
    {
        var fullPath = await this.GetComposeFilePathAsync(projectName);

        // 2. On configure le processus pour qu'il soit invisible et capture les erreurs
        var processInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = $"compose -f \"{fullPath}\" stop",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // 3. On lance la commande dans le système d'exploitation
        using var process = Process.Start(processInfo);
    
        if (process == null)
        {
            throw new Exception("Le système n'a pas pu démarrer le processus Docker.");
        }

        // 4. On attend que la commande se termine (sans bloquer le thread de l'API)
        await process.WaitForExitAsync();

        // 5. On vérifie le code de retour (0 = Succès, tout le reste = Erreur)
        if (process.ExitCode != 0)
        {
            // On lit le message d'erreur généré par Docker pour l'envoyer au Front-end Angular
            string errorOutput = await process.StandardError.ReadToEndAsync();
            throw new Exception($"Échec de l'arrêt Docker pour '{projectName}': {errorOutput}");
        }
    }
    
    public async Task DownDynamicStackAsync(string projectName)
    {
        var fullPath = await this.GetComposeFilePathAsync(projectName);
        
        new Builder()
            .UseContainer()
            .UseCompose()
            .FromFile(fullPath)
            .Build()
            .Dispose();
    }
}