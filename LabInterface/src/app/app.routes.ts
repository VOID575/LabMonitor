import { Routes } from '@angular/router';
import {DashboardComponent} from './features/dashboard/dashboard.component';
import {ProjectDetailComponent} from './features/project-detail/project-detail.component';

export const routes: Routes = [
  {
    path: 'dashboard',
    component: DashboardComponent,
  },
  {
    path: 'project/:projectName',
    component: ProjectDetailComponent,
  },
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full',
  }
];
