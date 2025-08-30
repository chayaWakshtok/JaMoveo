using JaMoveo.Application.Interfaces;
using JaMoveo.Core.Interfaces;
using JaMoveo.Infrastructure.Data;
using JaMoveo.Infrastructure.Entities;
using JaMoveo.Infrastructure.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JaMoveo.Application.Hubs
{
    [Authorize]
    public class RehearsalHub : Hub
    {
        private readonly IRehearsalService _rehearsalService;
        private readonly ISongService _songService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RehearsalHub> _logger;

        public RehearsalHub(
            IRehearsalService rehearsalService,
            ISongService songService,
            UserManager<ApplicationUser> userManager,
            ILogger<RehearsalHub> logger)
        {
            _rehearsalService = rehearsalService;
            _songService = songService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task JoinRehearsal()
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _userManager.FindByIdAsync(userId.ToString());
                var username = user?.UserName ?? "Unknown";

                // מציאת חדר החזרות הפעיל
                var activeSession = await _rehearsalService.GetActiveSessionAsync();
                if (activeSession != null)
                {
                    await _rehearsalService.JoinSessionAsync(userId, activeSession.SessionId);
                    await Groups.AddToGroupAsync(Context.ConnectionId, "rehearsal");

                    _logger.LogInformation("משתמש {Username} הצטרף לחדר החזרות", username);

                    // הודעה לכל המשתמשים על הצטרפות
                    await Clients.Group("rehearsal").SendAsync("UserJoined", username);

                    // אם יש שיר פעיל, שלח אותו למשתמש החדש
                    if (activeSession.CurrentSongId.HasValue)
                    {
                        var currentSong = await _songService.GetSongByIdAsync(activeSession.CurrentSongId.Value);
                        await Clients.Caller.SendAsync("SongSelected", currentSong);
                    }
                }
                else
                {
                    await Clients.Caller.SendAsync("NoActiveSession");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בהצטרפות לחדר חזרות");
                await Clients.Caller.SendAsync("Error", "שגיאה בהצטרפות לחדר החזרות");
            }
        }

        public async Task LeaveRehearsal()
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _userManager.FindByIdAsync(userId.ToString());
                var username = user?.UserName ?? "Unknown";

                var activeSession = await _rehearsalService.GetActiveSessionAsync();
                if (activeSession != null)
                {
                    await _rehearsalService.LeaveSessionAsync(userId, activeSession.SessionId);
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, "rehearsal");

                    _logger.LogInformation("משתמש {Username} עזב את חדר החזרות", username);

                    await Clients.Group("rehearsal").SendAsync("UserLeft", username);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה ביציאה מחדר חזרות");
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task SelectSong(int songId)
        {
            try
            {
                var adminId = GetCurrentUserId();
                var success = await _rehearsalService.SelectSongAsync(songId, adminId);

                if (success)
                {
                    var song = await _songService.GetSongByIdAsync(songId);

                    _logger.LogInformation("מנהל בחר שיר: {SongName} - {Artist}", song.Name, song.Artist);

                    // שלח את השיר לכל המשתמשים המחוברים
                    await Clients.Group("rehearsal").SendAsync("SongSelected", song);
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", "לא ניתן לבחור את השיר");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בבחירת שיר: {SongId}", songId);
                await Clients.Caller.SendAsync("Error", "שגיאה בבחירת השיר");
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task QuitSession()
        {
            try
            {
                var adminId = GetCurrentUserId();
                var success = await _rehearsalService.EndSessionAsync(adminId);

                if (success)
                {
                    _logger.LogInformation("מנהל סיים את חדר החזרות");

                    // הודעה לכל המשתמשים שהחזרה הסתיימה
                    await Clients.Group("rehearsal").SendAsync("SessionEnded");
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", "לא ניתן לסיים את החדר");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בסיום חדר חזרות");
                await Clients.Caller.SendAsync("Error", "שגיאה בסיום החדר");
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _userManager.FindByIdAsync(userId.ToString());
                var username = user?.UserName ?? "Unknown";

                var activeSession = await _rehearsalService.GetActiveSessionAsync();
                if (activeSession != null)
                {
                    await _rehearsalService.LeaveSessionAsync(userId, activeSession.SessionId);
                }

                await Clients.Group("rehearsal").SendAsync("UserLeft", username);

                _logger.LogInformation("משתמש {Username} התנתק", username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בניתוק משתמש");
            }

            await base.OnDisconnectedAsync(exception);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = Context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("משתמש לא מזוהה");
            }
            return userId;
        }
    }
}