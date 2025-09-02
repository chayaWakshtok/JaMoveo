import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { RehearsalSession, Song } from '../models/song.model';
import { API_CONFIG } from '../config/api.config';

@Injectable({
  providedIn: 'root'
})
export class RehearsalService {

  constructor(private http: HttpClient) {}

  createSession(): Observable<RehearsalSession> {
    return this.http.post<RehearsalSession>(`${API_CONFIG.BASE_URL}rehearsal/create`, {});
  }

  getActiveSession(): Observable<RehearsalSession> {
    return this.http.get<RehearsalSession>(`${API_CONFIG.BASE_URL}rehearsal/active`);
  }

  joinSession(sessionId: string): Observable<any> {
    return this.http.post(`${API_CONFIG.BASE_URL}rehearsal/join/${sessionId}`, {});
  }

  leaveSession(sessionId: string): Observable<any> {
    return this.http.post(`${API_CONFIG.BASE_URL}rehearsal/leave/${sessionId}`, {});
  }

  selectSong(songId: number): Observable<any> {
    return this.http.post(`${API_CONFIG.BASE_URL}rehearsal/select-song/${songId}`, {});
  }

  endSession(): Observable<any> {
    return this.http.post(`${API_CONFIG.BASE_URL}rehearsal/end`, {});
  }

  getCurrentSong(): Observable<Song> {
    return this.http.get<Song>(`${API_CONFIG.BASE_URL}rehearsal/current-song`);
  }

  getConnectedUsers(sessionId: string): Observable<string[]> {
    return this.http.get<string[]>(`${API_CONFIG.BASE_URL}rehearsal/connected-users/${sessionId}`);
  }
}
