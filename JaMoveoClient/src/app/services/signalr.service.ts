import { inject, Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { AuthService } from './auth.service';
import { RehearsalSession, Song } from '../models/song.model';
import { API_CONFIG } from '../config/api.config';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: HubConnection | undefined;

  public songSelected = new Subject<Song>();
  public sessionEnded = new Subject<void>();
  public sessionCreated = new Subject<RehearsalSession>();
  public joinedSession = new Subject<RehearsalSession>();
  public userJoined = new Subject<string>();
  public userLeft = new Subject<string>();
  public noActiveSession = new Subject<string>();
  public newSessionAvailable = new Subject<string>();
  public error = new Subject<string>();

  authService = inject(AuthService);

  public async startConnection(): Promise<void> {
    const token = this.authService.getToken();

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(API_CONFIG.SIGNALR_URL, {
        accessTokenFactory: () => token || ''
      })
      .build();

    try {
      await this.hubConnection.start();
      console.log('SignalR connection started');

      this.setupEventHandlers();

    } catch (error) {
      console.error('Error starting SignalR connection:', error);
    }
  }

  public async stopConnection(): Promise<void> {
    if (this.hubConnection) {
      await this.leaveRehearsal();
      await this.hubConnection.stop();
    }
  }

  private setupEventHandlers(): void {
    if (!this.hubConnection) return;

    // הודעות על שירים
    this.hubConnection.on('SongSelected', (song: Song) => {
      debugger
      this.songSelected.next(song);
    });

    // הודעות על חדר חזרות
    this.hubConnection.on('SessionCreated', (session: RehearsalSession) => {
      console.log('Session created:', session);
      this.sessionCreated.next(session);
    });

    this.hubConnection.on('JoinedSession', (session: RehearsalSession) => {
      console.log('Joined session:', session);
      this.joinedSession.next(session);
    });

    this.hubConnection.on('SessionEnded', () => {
      this.sessionEnded.next();
    });

    this.hubConnection.on('NoActiveSession', (message: string) => {
      console.log('No active session:', message);
      this.noActiveSession.next(message);
    });

    this.hubConnection.on('NewSessionAvailable', (message: string) => {
      console.log('New session available:', message);
      this.newSessionAvailable.next(message);
    });

    // הודעות על משתמשים
    this.hubConnection.on('UserJoined', (username: string) => {
      this.userJoined.next(username);
    });

    this.hubConnection.on('UserLeft', (username: string) => {
      this.userLeft.next(username);
    });

    // הודעות שגיאה
    this.hubConnection.on('Error', (message: string) => {
      console.error('SignalR Error:', message);
      this.error.next(message);
    });
  }

  public async joinRehearsal(): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.invoke('JoinRehearsal');
    }
  }

  public async leaveRehearsal(): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.invoke('LeaveRehearsal');
    }
  }

  public async createNewSession(): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.invoke('CreateNewSession');
    }
  }

  public async selectSong(songId: number): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.invoke('SelectSong', songId);
    }
  }

  public async quitSession(): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.invoke('QuitSession');
    }
  }
}
