# LabMonitor application

## LabApi :

### Sequence diagram for starting a container :
This diagram illustrates the flow of a request to start a Docker container through the LabApi, showcasing the exception handling.

```mermaid
sequenceDiagram
    autonumber
    
    box "Client" #f9f9f9
        actor Client as Angular / Swagger
    end

    box "Couche Présentation (API)" #e1f5fe
        participant Ctl as DockerController
        participant Base as BaseApiController
    end

    box "Couche Application (Métier)" #fff9c4
        participant Svc as DockerService
        participant Model as Result (Factory)
    end

    box "Infrastructure" #ffe0b2
        participant Lib as Docker.DotNet (Lib)
        participant Daemon as Docker Daemon (Socket)
    end

    %% DÉBUT DU FLUX
    Client->>Ctl: POST /api/docker/start/{id}
    activate Ctl
    
    note over Ctl: Reçoit la requête HTTP
    Ctl->>Svc: StartContainerAsync(id)
    activate Svc

    %% BLOC TRY / CATCH DANS LE SERVICE
    rect rgb(0, 0, 240)
        note right of Svc: Bloc try { ... }
        Svc->>Lib: client.Containers.StartContainerAsync(...)
        activate Lib
        Lib->>Daemon: POST /containers/{id}/start (Unix Socket)
        activate Daemon
        
        alt Cas 1 : Succès (Happy Path)
            Daemon-->>Lib: 204 No Content
            Lib-->>Svc: (void)
            
            Svc->>Model: Result.Success()
            activate Model
            Model-->>Svc: Retourne Result { IsSuccess=true }
            deactivate Model

        else Cas 2 : Erreur (ex: Introuvable)
            Daemon-->>Lib: 404 Not Found
            deactivate Daemon
            Lib-->>Svc: THROW DockerContainerNotFoundException
            deactivate Lib
            
            note right of Svc: catch (Exception ex)
            Svc->>Model: Result.Failure(ex)
            activate Model
            
            note over Model: SWITCH (ex)<br/>DockerContainerNotFoundException<br/>=> ErrorType.NotFound
            
            Model-->>Svc: Retourne Result { IsSuccess=false, Type=NotFound }
            deactivate Model
        end
    end

    %% RETOUR AU CONTROLEUR
    Svc-->>Ctl: Retourne objet Result
    deactivate Svc

    %% TRAITEMENT PAR LE BASE CONTROLLER
    Ctl->>Base: ProcessResult(result)
    activate Base
    
    alt Si IsSuccess == true
        Base-->>Ctl: Retourne Ok() (HTTP 200)
    else Si IsSuccess == false
        note over Base: SWITCH (result.Type)<br/>ErrorType.NotFound => NotFound()<br/>ErrorType.Conflict => Conflict()
        Base-->>Ctl: Retourne NotFound(...) (HTTP 404)
    end
    deactivate Base

    %% RÉPONSE FINALE
    Ctl-->>Client: Réponse HTTP + JSON
    deactivate Ctl
```

### Global application architecture

Show the overall architecture of the LabMonitor application, highlighting the interactions between the Angular client, ASP.NET Core backend, and Docker daemon.
```mermaid
graph TD
    Client[Client Angular]
    
    subgraph "ASP.NET Core (Backend)"
        Router[Aiguillage HTTP/WS]
        
        subgraph "Rest API (Stateless)"
            Controller[DockerController]
        end
        
        subgraph "Real-Time (Stateful)"
            Hub[LogHub]
        end
        
        Service[DockerService / DockerManager]
    end
    
    Docker[Docker Daemon]

    %% Flux REST
    Client -- "1. POST /start (HTTP)" --> Router
    Router --> Controller
    Controller -- "StartAsync()" --> Service

    %% Flux SignalR
    Client -- "2. Connect Stream (WebSocket)" --> Router
    Router --> Hub
    Hub -- "StreamLogs()" --> Service

    %% Le Service parle à Docker pour tout le monde
    Service <--> Docker
```

### Log streaming architecture
This diagram illustrates the architecture for streaming logs from Docker containers to the Angular client using SignalR,

This is the code that allows to handle the concurrency and buffering of logs in memory, ensuring that the logs are sent to the client in a thread-safe manner.

```csharp

```

```mermaid
[ Thread Docker (Task.Run) ] 
       |
       | (Push binaire démultiplexé)
       v
[ PIPE (Buffer FIFO en RAM) ] <--- Gestion automatique de la concurrence (Locks)
       |
       | (Pull via StreamReader)
       v
[ Thread SignalR (IAsyncEnumerable) ]
       |
       | (Yield Return)
       v
[ Client Web (WebSocket) ]
```

### Test the log streaming :

To test the log streaming, you can use the Angular client to connect to the SignalR hub and display the logs in real-time. You can also use tools like Postman or curl to send requests to the REST API to start containers and trigger log streaming.

First :
```json
{"protocol":"json","version":1}
```

Then :
```json
{"type":4,"invocationId":"1","target":"GetLogStream","arguments":["7026077ea5f4"]}
```

**You must keep the special character ""** at the end of each message, as it is used by SignalR to delimit messages in the WebSocket stream.