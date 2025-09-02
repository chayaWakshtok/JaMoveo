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
    return this.http.post<RehearsalSession>(`${API_CONFIG.BASE_URL}Rehearsal/create`, {});
  }

  getActiveSession(): Observable<RehearsalSession> {
    return this.http.get<RehearsalSession>(`${API_CONFIG.BASE_URL}Rehearsal/active`);
  }

  joinSession(sessionId: string): Observable<any> {
    return this.http.post(`${API_CONFIG.BASE_URL}Rehearsal/join/${sessionId}`, {});
  }

  leaveSession(sessionId: string): Observable<any> {
    return this.http.post(`${API_CONFIG.BASE_URL}Rehearsal/leave/${sessionId}`, {});
  }

  selectSong(songId: number): Observable<any> {
    return this.http.post(`${API_CONFIG.BASE_URL}Rehearsal/select-song/${songId}`, {});
  }

  endSession(): Observable<any> {
    return this.http.post(`${API_CONFIG.BASE_URL}Rehearsal/end`, {});
  }

  getCurrentSong(): Observable<Song> {
    return this.http.get<Song>(`${API_CONFIG.BASE_URL}Rehearsal/current-song`);
  }

  getConnectedUsers(sessionId: string): Observable<string[]> {
    return this.http.get<string[]>(`${API_CONFIG.BASE_URL}Rehearsal/connected-users/${sessionId}`);
  }
}
