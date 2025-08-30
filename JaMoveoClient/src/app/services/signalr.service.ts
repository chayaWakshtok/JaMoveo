import { inject, Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { AuthService } from './auth.service';
import { Song } from '../models/song.model';
import { API_CONFIG } from '../config/api.config';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: HubConnection | undefined;

  public songSelected = new Subject<Song>();
  public sessionEnded = new Subject<void>();
  public userJoined = new Subject<string>();
  public userLeft = new Subject<string>();

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
      await this.joinRehearsal();

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

    this.hubConnection.on('SongSelected', (song: Song) => {
      this.songSelected.next(song);
    });

    this.hubConnection.on('SessionEnded', () => {
      this.sessionEnded.next();
    });

    this.hubConnection.on('UserJoined', (username: string) => {
      this.userJoined.next(username);
    });

    this.hubConnection.on('UserLeft', (username: string) => {
      this.userLeft.next(username);
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
