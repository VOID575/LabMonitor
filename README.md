# LabMonitor

> **État du projet :** Ce projet est actuellement en cours de développement. Il s'agit d'un **prototype** (Proof of Concept) visant à créer une architecture robuste pour l'administration de conteneurs. Les fonctionnalités sont sujettes à évolution.

**LabMonitor** est un tableau de bord web sur-mesure (façon "mini-Portainer") conçu pour surveiller, piloter et administrer des conteneurs Docker et des stacks Docker Compose. Il offre une interface graphique moderne et fluide pour remplacer la complexité des lignes de commande CLI, tout en garantissant une exécution backend fiable et "stateless".

---

## Fonctionnalités Principales

*   **Vue d'ensemble par Projet :** Regroupement intelligent des conteneurs par stack (ex: `sae4`).
*   **Télémétrie en temps réel :** Affichage de la consommation CPU, de la RAM, et du nombre de conteneurs actifs/inactifs.
*   **Statuts de santé :** Indicateurs visuels (Sain, Attention, Erreur) basés sur l'état des sous-conteneurs (running, restarting, exited, dead).
*   **Filtrage Avancé :** Moteur de recherche puissant permettant de filtrer les conteneurs par `id`, `name` ou `image` via des opérateurs stricts (`eq`, `contains`, `startswith`, `endswith`).
*   **Gestion des Stacks :** Arrêt et démarrage dynamique des environnements `docker compose` directement depuis l'interface.

---

## Stack Technique

Le projet est divisé en deux applications distinctes, favorisant le découplage et la testabilité.

### Backend (`LabApi`)
*   **Framework :** .NET 10.0 (C#)
*   **Moteur Docker :** `Docker.DotNet` (via le socket `/var/run/docker.sock`)
*   **Exécution Système :** API `Process.Start` native pour l'exécution des commandes `docker compose` (remplace FluentDocker pour une meilleure stabilité stateless).
*   **Tests :** `xUnit` et `Moq` (Architecture orientée Interfaces et Injection de Dépendances).

### Frontend (`LabInterface`)
*   **Framework :** Angular 21 (avec Server-Side Rendering - SSR)
*   **Styling :** Tailwind CSS v4 (Thème sombre orienté Dashboard technique)
*   **Standards :** Utilisation du Control Flow moderne (`@if`, `@for`), de l'injection moderne (`inject()`), et respect strict des règles **ESLint** & **Prettier**.

---

## Architecture Système

Le diagramme ci-dessous illustre comment l'interface communique avec l'API, qui elle-même interagit avec le système Linux hôte et le démon Docker.

```mermaid
graph TD
    subgraph Frontend [LabInterface - Angular 21]
        UI[Tableau de Bord / UI]
        Services[Services HTTP]
    end

    subgraph Backend [LabApi - .NET 10]
        Controllers[Contrôleurs REST]
        Resolver[ContainerResolver]
        ProcessRunner[Process Manager]
    end

    subgraph Infrastructure Hôte
        DockerEngine[(Docker Engine\n/var/run/docker.sock)]
        HostOS[Terminal OS\nCLI]
    end

    UI -->|Appels HTTP| Controllers
    Controllers --> Resolver
    Controllers --> ProcessRunner
    
    Resolver -->|API Docker.DotNet| DockerEngine
    ProcessRunner -->|Commandes docker compose| HostOS
    
    classDef frontend fill:#dd0031,stroke:#a60024,color:white;
    classDef backend fill:#512bd4,stroke:#3b1e9c,color:white;
    classDef infra fill:#0db7ed,stroke:#0994c0,color:white;
    
    class Frontend frontend;
    class Backend backend;
    class Infrastructure infra;
```

## Intégration Continue (CI) & Tests

La qualité du code est assurée par un pipeline de vérification en deux étapes : la validation syntaxique (Linter) côté Frontend, suivie par l'exécution des tests unitaires mockés côté Backend.

```mermaid
flowchart LR
    Start([Nouveau Code]) --> Lint[🧹 ESLint & Prettier\nLabInterface]
    Lint --> Build[🔨 Build .NET 10\nLabApi]
    Build --> Test[🧪 Tests xUnit & Moq\nLabApi.Tests]
    Test --> Success([✅ Validation OK])
    
    style Start fill:#374151,stroke:#1f2937,color:white
    style Lint fill:#f59e0b,stroke:#d97706,color:white
    style Build fill:#3b82f6,stroke:#2563eb,color:white
    style Test fill:#8b5cf6,stroke:#7c3aed,color:white
    style Success fill:#10b981,stroke:#059669,color:white
```

## Focus sur l'ingénierie des tests
Les tests de l'API sont isolés du moteur Docker physique grâce à Moq. Le système injecte des faux clients Docker (IDockerClient, IContainerOperations) pour garantir une exécution des tests en quelques millisecondes, quel que soit l'environnement hôte, et pour valider la logique de filtrage LINQ indépendamment de l'infrastructure.