using JaMoveo.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JaMoveo.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RehearsalController : ControllerBase
    {
        private readonly IRehearsalService _rehearsalService;
        private readonly ILogger<RehearsalController> _logger;

        public RehearsalController(IRehearsalService rehearsalService, ILogger<RehearsalController> logger)
        {
            _rehearsalService = rehearsalService;
            _logger = logger;
        }

        [HttpPost("create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSession()
        {
            try
            {
                var userId = GetCurrentUserId();
                var session = await _rehearsalService.CreateSessionAsync(userId);

                _logger.LogInformation("נוצר חדר חזרות חדש על ידי: {UserId}", userId);

                return Ok(session);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה ביצירת חדר חזרות");
                return StatusCode(500, new { message = "אירעה שגיאה במערכת" });
            }
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveSession()
        {
            try
            {
                var session = await _rehearsalService.GetActiveSessionAsync();

                if (session == null)
                {
                    return NotFound(new { message = "אין חדר חזרות פעיל כרגע" });
                }

                return Ok(session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בקבלת חדר חזרות פעיל");
                return StatusCode(500, new { message = "אירעה שגיאה במערכת" });
            }
        }

        [HttpPost("join/{sessionId}")]
        public async Task<IActionResult> JoinSession(string sessionId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _rehearsalService.JoinSessionAsync(userId, sessionId);

                if (!success)
                {
                    return BadRequest(new { message = "לא ניתן להצטרף לחדר החזרות" });
                }

                _logger.LogInformation("משתמש {UserId} הצטרף לחדר חזרות {SessionId}", userId, sessionId);

                return Ok(new { message = "הצטרפת בהצלחה לחדר החזרות" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בהצטרפות לחדר חזרות");
                return StatusCode(500, new { message = "אירעה שגיאה במערכת" });
            }
        }

        [HttpPost("leave/{sessionId}")]
        public async Task<IActionResult> LeaveSession(string sessionId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _rehearsalService.LeaveSessionAsync(userId, sessionId);

                if (!success)
                {
                    return BadRequest(new { message = "לא ניתן לעזוב את חדר החזרות" });
                }

                _logger.LogInformation("משתמש {UserId} עזב את חדר חזרות {SessionId}", userId, sessionId);

                return Ok(new { message = "עזבת את חדר החזרות" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה ביציאה מחדר חזרות");
                return StatusCode(500, new { message = "אירעה שגיאה במערכת" });
            }
        }

        [HttpPost("select-song/{songId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SelectSong(int songId)
        {
            try
            {
                var adminId = GetCurrentUserId();
                var success = await _rehearsalService.SelectSongAsync(songId, adminId);

                if (!success)
                {
                    return BadRequest(new { message = "לא ניתן לבחור את השיר" });
                }

                _logger.LogInformation("מנהל {AdminId} בחר שיר {SongId}", adminId, songId);

                return Ok(new { message = "השיר נבחר בהצלחה" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בבחירת שיר");
                return StatusCode(500, new { message = "אירעה שגיאה במערכת" });
            }
        }

        [HttpPost("end")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EndSession()
        {
            try
            {
                var adminId = GetCurrentUserId();
                var success = await _rehearsalService.EndSessionAsync(adminId);

                if (!success)
                {
                    return BadRequest(new { message = "לא ניתן לסיים את חדר החזרות" });
                }

                _logger.LogInformation("מנהל {AdminId} סיים את חדר החזרות", adminId);

                return Ok(new { message = "חדר החזרות הסתיים" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בסיום חדר חזרות");
                return StatusCode(500, new { message = "אירעה שגיאה במערכת" });
            }
        }

        [HttpGet("current-song")]
        public async Task<IActionResult> GetCurrentSong()
        {
            try
            {
                var song = await _rehearsalService.GetCurrentSongAsync();

                if (song == null)
                {
                    return NotFound(new { message = "אין שיר פעיל כרגע" });
                }

                return Ok(song);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בקבלת השיר הנוכחי");
                return StatusCode(500, new { message = "אירעה שגיאה במערכת" });
            }
        }

        [HttpGet("connected-users/{sessionId}")]
        public async Task<IActionResult> GetConnectedUsers(string sessionId)
        {
            try
            {
                var users = await _rehearsalService.GetConnectedUsersAsync(sessionId);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בקבלת משתמשים מחוברים");
                return StatusCode(500, new { message = "אירעה שגיאה במערכת" });
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim.Value);
        }
    }
}