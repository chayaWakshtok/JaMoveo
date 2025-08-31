import { Component, OnInit, OnDestroy, inject, ViewChild, ElementRef } from '@angular/core';
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

  @ViewChild('songContainer', { static: false }) songContainer!: ElementRef;

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
  processedLines: any[] = [];


  private scrollInterval: any;
  private readonly CHAR_WIDTH_PIXELS = 12; // רוחב ממוצע של תו באנגלית
  private readonly HEBREW_CHAR_WIDTH = 14; // רוחב ממוצע של תו בעברית
  private readonly MIN_CHORD_WIDTH = 30; // רוחב מינימלי לאקורד

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
        this.formatSongContent();
        this.loading = false;
        return;
      }

      console.log('🔍 No cached song, fetching from server...');
      // this.rehearsalService.getCurrentSong().subscribe({
      //   next: (song) => {
      //     console.log('🎵 Got song from server:', song);
      //     this.currentSong = song;
      //     this.formatSongContent();
      //     this.loading = false;
      //   },
      //   error: (error: any) => {
      //     console.error('❌ Error fetching current song:', error);
      //     this.error = 'שגיאה בטעינת השיר הנוכחי';
      //     this.loading = false;

      //     // אם אין שיר, חזור לעמוד הראשי
      //     setTimeout(() => {
      //       this.router.navigate(['/main']);
      //     }, 2000);
      //   }
      // });

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
        this.formatSongContent();
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
          this.formatSongContent();
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

  getChordWidth(lyrics: string): number {
    if (!lyrics) {
      return this.MIN_CHORD_WIDTH;
    }

    // זיהוי שפה
    const isHebrew = this.isHebrewText(lyrics);
    const charWidth = isHebrew ? this.HEBREW_CHAR_WIDTH : this.CHAR_WIDTH_PIXELS;

    // חישוב רוחב בסיסי
    const baseWidth = lyrics.length * charWidth;

    // רוחב מינימלי
    const calculatedWidth = Math.max(baseWidth, this.MIN_CHORD_WIDTH);

    console.log(`📏 Chord width for "${lyrics}": ${calculatedWidth}px (Hebrew: ${isHebrew})`);

    return calculatedWidth;
  }

  // בדיקה אם הטקסט בעברית
  private isHebrewText(text: string): boolean {
    return text.split('').some(char => {
      const code = char.charCodeAt(0);
      return code >= 0x0590 && code <= 0x05FF;
    });
  }


  formatSongContent(): void {
    if (!this.currentSong?.lines) {
      console.log('📝 No song content, using simple format');
      return;
    }

    // עיבוד המבנה המורכב להצגה
    this.processedLines = this.currentSong.lines.map((line, lineIndex) => {
      return {
        lineIndex,
        words: line.map((wordChord, wordIndex) => ({
          wordIndex,
          lyrics: wordChord.lyrics || '',
          chords: wordChord.chords || '',
          hasChords: !!wordChord.chords,
          width: this.getChordWidth(wordChord.lyrics || ''),
          isHebrew: this.isHebrewText(wordChord.lyrics || '')
        }))
      };
    });

    console.log('✅ Processed lines:', this.processedLines.length);
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


  refreshSong(): void {
    this.loading = true;
    this.error = '';
    this.loadCurrentSong();
  }

  // מתודות עזר נוספות
  getLineDirection(line: any): string {
    if (!line.words || line.words.length === 0) {
      return 'ltr';
    }

    // אם רוב המילים בעברית, כיוון מימין לשמאל
    const hebrewWords = line.words.filter((word: any) => word.isHebrew).length;
    const totalWords = line.words.length;

    return hebrewWords > totalWords / 2 ? 'rtl' : 'ltr';
  }
  hasChords(line: any) {
    return line.words.some((word: any) => word.hasChords);
  }
  // חישוב רווח בין מילים
  getWordSpacing(word: any, nextWord: any): number {
    if (!nextWord) return 0;

    // רווח דינמי בהתאם לאורך המילים
    const currentWidth = word.width || this.MIN_CHORD_WIDTH;
    const baseSpacing = Math.max(10, currentWidth * 0.1);

    return Math.min(baseSpacing, 30); // מקסימום 30px
  }
}

