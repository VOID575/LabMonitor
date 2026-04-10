import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import {CommonModule} from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { ContainerProvider } from '../../core/api/container-provider';
import { ContainerManager } from '../../core/api/container-manager';
import { ContainerGroup } from '../../shared/Interfaces/containers/container-group.model';
import { AppRoutes} from '../../app.routes.names';
import {DockerContainer} from '../../shared/Interfaces/containers/containers.model';

@Component({
  standalone: true,
  selector: 'app-media-stack',
  imports: [CommonModule, RouterLink],
  templateUrl: './project-detail.component.html'
})
export class ProjectDetailComponent {

  readonly routes = AppRoutes; // expose les routes au template

  public stackData: StackDetails = {
    name: 'Media Stack',
    status: 'healthy',
    description: 'Suite complète de gestion multimédia avec Plex, Sonarr, Radarr et Transmission',
    category: 'Media',
    totalCpu: '15.3%',
    totalRam: '2.8 GB',
    containers: [
      {
        id: '1',
        name: 'plex-server',
        status: 'running',
        image: 'plexinc/pms-docker:latest',
        uptime: '15d 7h 23m',
        stats: {
          cpuUsagePercent: 8.2,
          memoryUsedMB: 1024,
          memoryLimitMB: 4096,
          networkRx: '125.3 GB',
          networkTx: '89.2 GB'
        },
        ports: ['32400:32400', '3005:3005']
      },
      {
        id: '2',
        name: 'sonarr',
        status: 'running',
        image: 'linuxserver/sonarr:latest',
        uptime: '15d 7h 20m',
        stats: {
          cpuUsagePercent: 3.1,
          memoryUsedMB: 512,
          memoryLimitMB: 1024,
          networkRx: '2.1 GB',
          networkTx: '1.8 GB'
        },
        ports: ['8989:8989']
      }
    ]
  };

  constructor(private router: Router) {}

  goToProject(name: string) {
    this.router.navigate([AppRoutes.PROJECT_DETAIL(name)]);
  }

  getMemoryPercentage(used: number, limit: number): number {
    if (!limit) return 0;
    return (used / limit) * 100;
  }
}
