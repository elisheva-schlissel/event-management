import { Component, OnInit, OnDestroy, signal, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { EventsService } from '../../core/events.service';
import { RealtimeService } from '../../core/realtime.service';
import { AuthService } from '../../core/auth.service';
import { EventDto } from '../../core/models';

/**
 * מסך הסדרן — סוגר את ה-Flow הנדרש E2E:
 * טוען את האירועים הקיימים, ומאזין ל-SignalR לאירועים חדשים שנדחפים בזמן אמת.
 */
@Component({
  selector: 'app-dispatcher',
  imports: [DatePipe],
  templateUrl: './dispatcher.html',
  styleUrl: './dispatcher.css',
})
export class Dispatcher implements OnInit, OnDestroy {
  private events = inject(EventsService);
  private realtime = inject(RealtimeService);
  private auth = inject(AuthService);
  private router = inject(Router);

  readonly items = signal<EventDto[]>([]);
  readonly connected = this.realtime.connected;
  readonly name = this.auth.name;
  readonly flashId = signal<string | null>(null);

  private sub?: Subscription;

  async ngOnInit(): Promise<void> {
    this.events.getAll().subscribe((list) => this.items.set(list));

    // כל אירוע חדש שנדחף מהשרת מתווסף לראש הרשימה — ללא רענון.
    this.sub = this.realtime.newEvent$.subscribe((ev) => {
      this.items.update((cur) => [ev, ...cur.filter((e) => e.id !== ev.id)]);
      this.flashId.set(ev.id);
      setTimeout(() => this.flashId.set(null), 2500);
    });

    await this.realtime.start();
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  logout(): void {
    this.realtime.stop();
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
