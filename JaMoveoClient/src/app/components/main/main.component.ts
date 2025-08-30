import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AuthService } from '../../services/auth.service';
import { SignalRService } from '../../services/signalr.service';
import { RehearsalSession, SongSearchResult } from '../../models/song.model';
import { SongService } from '../../services/song.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-main',
  imports: [FormsModule],
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.scss']
})
export class MainComponent implements OnInit, OnDestroy {

  private songService = inject(SongService);
  private signalRService = inject(SignalRService)
  private router = inject(Router);
  private destroy$ = new Subject<void>();

  authService = inject(AuthService);
  isAdmin!: boolean;
  searchQuery = '';
  searchResults: SongSearchResult[] = [];
  searching = false;
  waitingMessage = 'Waiting for next song...';
  currentSession: RehearsalSession | null = null;
  showCreateSessionButton = false;
  connectedUsers: string[] = [];


  ngOnInit(): void {
    this.isAdmin = this.authService.isAdmin();
    this.initializeSignalR();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private async initializeSignalR(): Promise<void> {

    await this.signalRService.startConnection();

    // האזנה לאירועי SignalR
    this.signalRService.sessionCreated
      .pipe(takeUntil(this.destroy$))
      .subscribe((session) => {
        this.currentSession = session;
        this.waitingMessage = 'חדר חזרות פעיל - חפש שירים';
        this.showCreateSessionButton = false;
      });
    // Listen for song selection
    this.signalRService.joinedSession
      .pipe(takeUntil(this.destroy$))
      .subscribe((session) => {
        this.currentSession = session;
        if (this.isAdmin) {
          this.waitingMessage = 'חדר חזרות פעיל - חפש שירים';
        } else {
          this.waitingMessage = 'מחובר לחדר - ממתין לבחירת שיר';
        }
        this.showCreateSessionButton = false;
      });

    this.signalRService.noActiveSession
      .pipe(takeUntil(this.destroy$))
      .subscribe((message) => {
        this.waitingMessage = message;
        if (this.isAdmin) {
          this.showCreateSessionButton = true;
        }
      });

    this.signalRService.newSessionAvailable
      .pipe(takeUntil(this.destroy$))
      .subscribe((message) => {
        // הצטרף אוטומטית לחדר החדש
        this.signalRService.joinRehearsal();
      });

    this.signalRService.songSelected
      .pipe(takeUntil(this.destroy$))
      .subscribe((song) => {
        console.log('🎵 Song selected in Main Component, navigating to live:', song.name);
        // מעבר לעמוד Live עם זמן המתנה קצר
        setTimeout(() => {
          this.router.navigate(['/live']);
        }, 500);
      });

    this.signalRService.sessionEnded
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.currentSession = null;
        this.waitingMessage = 'החזרה הסתיימה';
        if (this.isAdmin) {
          this.showCreateSessionButton = true;
        }
      });

    this.signalRService.userJoined
      .pipe(takeUntil(this.destroy$))
      .subscribe((username) => {
        if (!this.connectedUsers.includes(username)) {
          this.connectedUsers.push(username);
        }
      });

    this.signalRService.userLeft
      .pipe(takeUntil(this.destroy$))
      .subscribe((username) => {
        this.connectedUsers = this.connectedUsers.filter(user => user !== username);
      });

    this.signalRService.error
      .pipe(takeUntil(this.destroy$))
      .subscribe((error) => {
        console.error('SignalR Error:', error);
        this.waitingMessage = `שגיאה: ${error}`;
      });

    // התחבר לחדר חזרות
    await this.signalRService.joinRehearsal();
  }

  async createSession(): Promise<void> {
    if (this.isAdmin) {
      await this.signalRService.createNewSession();
    }
  }

  searchSongs(): void {
    if (!this.searchQuery.trim()) return;

    this.searching = true;
    this.songService.searchSongs(this.searchQuery)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (results) => {
          this.searchResults = results.songs;
          this.searching = false;
          this.router.navigate(['/results'], {
            state: { results: this.searchResults, query: this.searchQuery }
          });
        },
        error: () => {
          this.searching = false;
        }
      });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
