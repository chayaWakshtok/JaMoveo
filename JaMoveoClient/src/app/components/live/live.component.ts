import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { Subject, interval } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AuthService } from '../../services/auth.service';
import { SignalRService } from '../../services/signalr.service';
import { Song } from '../../models/song.model';
import { Router } from '@angular/router';

@Component({
  selector: 'app-live',
  templateUrl: './live.component.html',
  styleUrls: ['./live.component.scss']
})
export class LiveComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private readonly authService = inject(AuthService);
  private readonly signalRService = inject(SignalRService);
  //private rehearsalService: RehearsalService
  readonly router = inject(Router);

  currentSong: Song | null = null;
  isAdmin!: boolean;
  isSinger!: boolean;
  autoScroll = false;
  loading = true;
  error = '';

  private scrollInterval: any;

  ngOnInit(): void {
    this.isAdmin = this.authService.isAdmin();
    this.isSinger = this.authService.isSinger();
    debugger
    this.loadCurrentSong();
    this.setupSignalRListeners();

    // Get current song if available
    // This would typically come from a service or route state
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.stopAutoScroll();
  }

  private async loadCurrentSong(): Promise<void> {
    try {
      // נסה קודם לקבל את השיר הנוכחי מ-SignalR Service
      const cachedSong = this.signalRService.getCurrentSong();

      if (cachedSong) {
        console.log('🎵 Got cached song from SignalR:', cachedSong);
        this.currentSong = cachedSong;
        this.loading = false;
        return;
      }

    } catch (error) {
      console.error('❌ Error in loadCurrentSong:', error);
      this.error = 'שגיאה בטעינת השיר';
      this.loading = false;
    }
  }

  private setupSignalRListeners(): void {
    console.log('🎧 Setting up SignalR listeners...');

    // האזנה לשיר חדש
    this.signalRService.songSelected
      .pipe(takeUntil(this.destroy$))
      .subscribe((song: Song) => {
        console.log('🎵 New song selected in Live Component:', song);
        this.currentSong = song;
        this.loading = false;
        this.error = '';
      });

    // האזנה לשיר נוכחי (BehaviorSubject)
    this.signalRService.currentSong$
      .pipe(takeUntil(this.destroy$))
      .subscribe((song: Song | null) => {
        console.log('🎵 Current song updated:', song);
        if (song) {
          this.currentSong = song;
          this.loading = false;
          this.error = '';
        }
      });

    // האזנה לסיום חזרה
    this.signalRService.sessionEnded
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        console.log('🔚 Session ended, returning to main');
        this.router.navigate(['/main']);
      });

    // האזנה לשגיאות
    this.signalRService.error
      .pipe(takeUntil(this.destroy$))
      .subscribe((error) => {
        console.error('❌ SignalR error in Live Component:', error);
        this.error = error;
      });
  }


  toggleAutoScroll(): void {
    this.autoScroll = !this.autoScroll;

    if (this.autoScroll) {
      this.startAutoScroll();
    } else {
      this.stopAutoScroll();
    }
  }

  private startAutoScroll(): void {
    this.scrollInterval = setInterval(() => {
      window.scrollBy(0, 1); // Slow scroll down
    }, 100);
  }

  private stopAutoScroll(): void {
    if (this.scrollInterval) {
      clearInterval(this.scrollInterval);
      this.scrollInterval = null;
    }
  }

  async quitSession(): Promise<void> {
    if (this.isAdmin) {
      await this.signalRService.quitSession();
    }
  }

  formatLyrics(lyrics: string): string[] {
    return lyrics ? lyrics.split('\n') : [];
  }

  formatChords(chords: string): string[] {
    return chords ? chords.split('\n') : [];
  }

  refreshSong(): void {
    this.loading = true;
    this.error = '';
    this.loadCurrentSong();
  }
}
