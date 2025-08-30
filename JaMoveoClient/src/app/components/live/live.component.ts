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
  private readonly router = inject(Router);

  currentSong: Song | null = null;
  isAdmin!: boolean;
  isSinger!: boolean;
  autoScroll = false;
  private scrollInterval: any;

  ngOnInit(): void {
    this.isAdmin = this.authService.isAdmin();
    this.isSinger = this.authService.isSinger();
    this.setupSignalRListeners();

    // Get current song if available
    // This would typically come from a service or route state
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.stopAutoScroll();
  }

  private setupSignalRListeners(): void {
    this.signalRService.songSelected
      .pipe(takeUntil(this.destroy$))
      .subscribe((song: Song) => {
        this.currentSong = song;
      });

    this.signalRService.sessionEnded
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.router.navigate(['/main']);
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
}
