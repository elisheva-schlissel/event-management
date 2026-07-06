import { Routes } from '@angular/router';
import { roleGuard } from './core/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  {
    path: 'login',
    loadComponent: () => import('./features/login/login').then((m) => m.Login),
  },
  {
    path: 'dispatcher',
    canActivate: [roleGuard('Dispatcher')],
    loadComponent: () => import('./features/dispatcher/dispatcher').then((m) => m.Dispatcher),
  },
  {
    path: 'technician',
    canActivate: [roleGuard('Technician')],
    loadComponent: () => import('./features/technician/technician').then((m) => m.Technician),
  },
  { path: '**', redirectTo: 'login' },
];
