import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE } from './api.config';
import { EventDto } from './models';

@Injectable({ providedIn: 'root' })
export class EventsService {
  constructor(private http: HttpClient) {}

  getAll(): Observable<EventDto[]> {
    return this.http.get<EventDto[]>(`${API_BASE}/api/events`);
  }

  getHistory(id: string): Observable<any[]> {
    return this.http.get<any[]>(`${API_BASE}/api/events/${id}/history`);
  }
}
