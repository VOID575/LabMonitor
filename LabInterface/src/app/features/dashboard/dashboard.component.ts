import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ContainerProvider } from '../../core/api/container-provider';
import { DockerContainer } from '../../shared/Interfaces/containers/containers.model';
import { ContainerManager } from '../../core/api/container-manager';
import { ContainerGroup } from '../../shared/Interfaces/containers/container-group.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {

  containerGroups: ContainerGroup[] = [];
  isLoading: boolean = true;
  error: string | null = null;
  containerManager : ContainerManager;
  containerProvider : ContainerProvider;

  constructor(private cdr: ChangeDetectorRef) {
    this.containerManager = new ContainerManager();
    this.containerProvider = new ContainerProvider();
  }

  async ngOnInit() {
    try {
      const containers: DockerContainer[] = await this.containerProvider.getAllContainers();
      console.log('[Dashboard] Conteneurs reçus :', containers);
      this.containerGroups = this.containerManager.groupContainers(containers);
      console.log('[Dashboard] Groupes générés :', this.containerGroups);
    } catch (e: any) {
      this.error = e?.message ?? 'Erreur inconnue lors du chargement des conteneurs.';
      console.error('[Dashboard] Erreur :', e);
    } finally {
      this.isLoading = false;
      this.cdr.detectChanges();
    }
  }

}
