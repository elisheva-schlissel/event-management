import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-login',
  imports: [FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  username = 'dispatcher';
  password = 'Passw0rd!';
  error = signal<string | null>(null);
  busy = signal(false);

  constructor(private auth: AuthService, private router: Router) {}

  async submit(): Promise<void> {
    this.error.set(null);
    this.busy.set(true);
    try {
      const res = await this.auth.login(this.username, this.password);
      this.router.navigate([res.role === 'Dispatcher' ? '/dispatcher' : '/technician']);
    } catch {
      this.error.set('שם משתמש או סיסמה שגויים');
    } finally {
      this.busy.set(false);
    }
  }
}
