import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth.service';

/**
 * SKELETON (§11): מסך הטכנאי. המבנה, ה-routing וה-auth מוגדרים.
 * התצוגות המלאות (אירועים שלי, עדכון סטטוס, בקשת אירוע פנוי, הערה לסדרן)
 * ימומשו בהמשך — כאן מוצג scaffold בלבד.
 */
@Component({
  selector: 'app-technician',
  templateUrl: './technician.html',
  styleUrl: './technician.css',
})
export class Technician {
  private auth = inject(AuthService);
  private router = inject(Router);
  readonly name = this.auth.name;

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
