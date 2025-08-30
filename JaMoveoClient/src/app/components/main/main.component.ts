import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AuthService } from '../../services/auth.service';
import { SignalRService } from '../../services/signalr.service';
import { SongSearchResult } from '../../models/song.model';
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

    // Listen for song selection
    this.signalRService.songSelected
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.router.navigate(['/live']);
      });
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
