# LabMonitor

> **Project Status:** This project is currently under active development. It is a **prototype** (Proof of Concept) aimed at building a robust architecture for container administration. Features are subject to change.

**LabMonitor** is a custom web dashboard (in the style of a "mini-Portainer") designed to monitor, manage, and control Docker containers and Docker Compose stacks. It provides a modern and smooth graphical user interface to replace the complexity of CLI commands, while ensuring reliable and stateless backend execution.

---

## Key Features

*   **Project Overview:** Smart grouping of containers by stack (e.g., `sae4`).
*   **Real-time Telemetry:** Live display of CPU usage, RAM consumption, and active/inactive container counts.
*   **Health Statuses:** Visual indicators (Healthy, Warning, Error) based on sub-container states (`running`, `restarting`, `exited`, `dead`).
*   **Advanced Filtering:** Powerful search engine enabling container filtering by `id`, `name`, or `image` using strict operators (`eq`, `contains`, `startswith`, `endswith`).
*   **Stack Management:** Dynamically stop and start `docker compose` environments directly from the interface.

---

## Tech Stack

The project is split into two distinct applications, promoting decoupling and testability.

### Backend (`LabApi`)
*   **Framework:** .NET 10.0 (C#)
*   **Docker Engine:** `Docker.DotNet` (via the `/var/run/docker.sock` socket)
*   **System Execution:** Native `Process.Start` API for running `docker compose` commands (replaces FluentDocker for better stateless stability).
*   **Testing:** `xUnit` and `Moq` (Interface-driven architecture and Dependency Injection).

### Frontend (`LabInterface`)
*   **Framework:** Angular 21 (with Server-Side Rendering - SSR)
*   **Styling:** Tailwind CSS v4 (Dark theme tailored for a technical dashboard)
*   **Standards:** Modern Control Flow (`@if`, `@for`), modern dependency injection (`inject()`), and strict compliance with **ESLint** & **Prettier** rules.

---

## System Architecture

The diagram below illustrates how the frontend communicates with the API, which in turn interacts with the host Linux system and the Docker daemon.

```mermaid
graph TD
    subgraph Frontend [LabInterface - Angular 21]
        UI[Dashboard / UI]
        Services[HTTP Services]
    end

    subgraph Backend [LabApi - .NET 10]
        Controllers[REST Controllers]
        Resolver[ContainerResolver]
        ProcessRunner[Process Manager]
    end

    subgraph Infrastructure [Host Infrastructure]
        DockerEngine[(Docker Engine\n/var/run/docker.sock)]
        HostOS[Host OS Terminal\nCLI]
    end

    UI -->|HTTP Calls| Controllers
    Controllers --> Resolver
    Controllers --> ProcessRunner
    
    Resolver -->|Docker.DotNet API| DockerEngine
    ProcessRunner -->|docker compose commands| HostOS
    
    classDef frontend fill:#dd0031,stroke:#a60024,color:white;
    classDef backend fill:#512bd4,stroke:#3b1e9c,color:white;
    classDef infra fill:#0db7ed,stroke:#0994c0,color:white;
    
    class Frontend frontend;
    class Backend backend;
    class Infrastructure infra;
```

## Continuous Integration (CI) & Testing

Code quality is ensured by a two-step verification pipeline: syntax validation (Linter) on the Frontend, followed by running mocked unit tests on the Backend.

```mermaid
flowchart LR
    Start([New Code]) --> Lint[🧹 ESLint & Prettier\nLabInterface]
    Lint --> Build[🔨 Build .NET 10\nLabApi]
    Build --> Test[🧪 xUnit & Moq Tests\nLabApi.Tests]
    Test --> Success([✅ Validation OK])
    
    style Start fill:#374151,stroke:#1f2937,color:white
    style Lint fill:#f59e0b,stroke:#d97706,color:white
    style Build fill:#3b82f6,stroke:#2563eb,color:white
    style Test fill:#8b5cf6,stroke:#7c3aed,color:white
    style Success fill:#10b981,stroke:#059669,color:white
```

## Focus on Test Engineering
API tests are isolated from the physical Docker engine using Moq. The system injects mock Docker clients (`IDockerClient`, `IContainerOperations`) to guarantee test execution within milliseconds, regardless of the host environment, and to validate LINQ filtering logic independently of the underlying infrastructure.
