import { Component, OnInit, ChangeDetectorRef, NgZone, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { PLATFORM_ID } from '@angular/core';
import {CommonModule} from '@angular/common';
import { RouterLink } from '@angular/router';
import { ContainerProvider } from '../../core/api/container-provider';
import { DockerComposeManager } from '../../core/api/docker-compose-manager';
import { AppRoutes} from '../../app.routes.names';
import {DockerContainer} from '../../shared/Interfaces/containers/containers.model';
import { ActivatedRoute } from '@angular/router';


@Component({
  standalone: true,
  selector: 'app-media-stack',
  imports: [CommonModule, RouterLink],
  templateUrl: './project-detail.component.html'
})
export class ProjectDetailComponent implements OnInit {
  private router = inject(ActivatedRoute);
  private cdr = inject(ChangeDetectorRef);
  private ngZone = inject(NgZone);
  private platformId = inject(PLATFORM_ID);


  readonly routes = AppRoutes; // Expose AppRoutes to the template
  containerProvider : ContainerProvider = new ContainerProvider();
  dockerComposeManager : DockerComposeManager = new DockerComposeManager();
  error: string | null = null;
  containers: DockerContainer[] = [];
  isLoading = true;
  projectName = '';
  isBrowser = false;

  async ngOnInit() {
    this.isBrowser = isPlatformBrowser(this.platformId);
    if (!this.isBrowser) {
      // On server, don't attempt client-only fetch; keep template minimal.
      this.isLoading = false;
      return;
    }

    try {
      const name = this.router.snapshot.paramMap.get('projectName');
      console.log(name);
      this.projectName = name ?? '';
      const data = await this.containerProvider.getContainerByProjectName(this.projectName);
      console.log('[loadContainersProjectDetail] Conteneurs reçus :', data);

      // assign inside Angular zone to ensure change detection runs
      this.ngZone.run(() => {
        this.containers = data;
        this.isLoading = false;
        try {
          this.cdr.detectChanges();
        } catch (e) {
          console.error('[ProjectDetail] Erreur :', e);
        }
      });
    } catch (e) {
      if(e instanceof Error) {
        this.error = e?.message ?? 'Erreur inconnue lors du chargement des conteneurs.';
        console.error('[ProjectDetail] Erreur :', e);
        this.ngZone.run(() => { this.isLoading = false; try { this.cdr.detectChanges(); } catch (e){console.error('[ProjectDetail] Erreur :', e);} });
      }
    }
  }

  onStartStack() {
    if (!this.projectName) return;
    this.isLoading = true;

    this.dockerComposeManager.startProject(this.projectName).then(() => {
        this.isLoading = false;
      }).catch( error => {
        this.error = error;
        this.isLoading = false;
    }).finally(() => {
      this.refreshDisplayedContainers();
    })
  }

  onStopStack() {
    if (!this.projectName) return;
    this.isLoading = true;
    console.log('[ProjectDetail] Tentative d\'arrêt de la stack :', this.projectName);
    this.dockerComposeManager.stopProject(this.projectName).then(() => {
      this.isLoading = false;
    }).catch( error => {
      this.error = error;
      this.isLoading = false;
    }).finally(() => {
      this.refreshDisplayedContainers();
    })
  }

  onDownStack() {
    if (!this.projectName) return;
    this.isLoading = true;

    this.dockerComposeManager.downProject(this.projectName).then(() => {
      this.isLoading = false;
    }).catch( error => {
      this.error = error;
      this.isLoading = false;
    }).finally(() => {
      this.refreshDisplayedContainers();
    })
  }

  refreshDisplayedContainers() {
    this.loadContainers().then(() => {
      this.isLoading = false;
      this.cdr.detectChanges();
    })
  }

  async loadContainers(): Promise<void>{
    this.containers = await this.containerProvider.getContainerByProjectName(this.projectName);
  }
}
