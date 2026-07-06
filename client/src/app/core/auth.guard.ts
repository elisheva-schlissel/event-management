import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

/** שומר ניתוב לפי התחברות ותפקיד. האכיפה האמיתית היא בשרת. */
export const roleGuard = (requiredRole: string): CanActivateFn => () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isLoggedIn()) {
    router.navigate(['/login']);
    return false;
  }
  if (auth.role() !== requiredRole) {
    router.navigate(['/login']);
    return false;
  }
  return true;
};
