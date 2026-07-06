import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { API_BASE } from './api.config';
import { LoginResponse } from './models';

const TOKEN_KEY = 'fem.token';
const ROLE_KEY = 'fem.role';
const NAME_KEY = 'fem.name';

/** מנהל התחברות ושמירת ה-JWT. הרשאות נאכפות בשרת — כאן רק ל-UX/ניתוב. */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly _token = signal<string | null>(localStorage.getItem(TOKEN_KEY));
  readonly role = signal<string | null>(localStorage.getItem(ROLE_KEY));
  readonly name = signal<string | null>(localStorage.getItem(NAME_KEY));
  readonly isLoggedIn = computed(() => !!this._token());

  constructor(private http: HttpClient) {}

  get token(): string | null {
    return this._token();
  }

  async login(username: string, password: string): Promise<LoginResponse> {
    const res = await firstValueFrom(
      this.http.post<LoginResponse>(`${API_BASE}/api/auth/login`, { username, password })
    );
    localStorage.setItem(TOKEN_KEY, res.token);
    localStorage.setItem(ROLE_KEY, res.role);
    localStorage.setItem(NAME_KEY, res.name);
    this._token.set(res.token);
    this.role.set(res.role);
    this.name.set(res.name);
    return res;
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(ROLE_KEY);
    localStorage.removeItem(NAME_KEY);
    this._token.set(null);
    this.role.set(null);
    this.name.set(null);
  }
}
