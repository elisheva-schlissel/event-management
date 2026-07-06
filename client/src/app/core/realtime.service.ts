import { Injectable, signal } from '@angular/core';
import { Subject } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { HUB_URL } from './api.config';
import { EventDto } from './models';
import { AuthService } from './auth.service';

/**
 * שירות ה-real-time (SignalR). מתחבר ל-Hub עם ה-JWT ומפרסם אירועים חדשים.
 * withAutomaticReconnect מטפל בניתוקים זמניים (§9).
 */
@Injectable({ providedIn: 'root' })
export class RealtimeService {
  private connection?: signalR.HubConnection;
  readonly connected = signal(false);

  /** אירועים חדשים שנדחפו מהשרת (event "NewEvent"). */
  readonly newEvent$ = new Subject<EventDto>();

  constructor(private auth: AuthService) {}

  async start(): Promise<void> {
    if (this.connection) return;

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL, { accessTokenFactory: () => this.auth.token ?? '' })
      .withAutomaticReconnect()
      .build();

    this.connection.on('NewEvent', (ev: EventDto) => this.newEvent$.next(ev));
    this.connection.onreconnected(() => this.connected.set(true));
    this.connection.onclose(() => this.connected.set(false));

    await this.connection.start();
    this.connected.set(true);
  }

  async stop(): Promise<void> {
    await this.connection?.stop();
    this.connection = undefined;
    this.connected.set(false);
  }
}
