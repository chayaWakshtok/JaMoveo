using JaMoveo.Application.Interfaces;
using JaMoveo.Application.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JaMoveo.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SongsController : ControllerBase
    {
        private readonly ISongService _songService;
        private readonly ILogger<SongsController> _logger;

        public SongsController(ISongService songService, ILogger<SongsController> logger)
        {
            _songService = songService;
            _logger = logger;
        }

        [HttpGet("search")]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> SearchSongs([FromQuery] string query, [FromQuery] int page)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest(new { message = "חובה להזין מילת חיפוש" });
                }

                var songs = await _songService.SearchSongsAsync(query,page);

                _logger.LogInformation("חיפוש שירים בוצע עבור: {Query}, נמצאו {Count} תוצאות", query, songs.TotalResults);

                return Ok(songs);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בחיפוש שירים עבור: {Query}", query);
                return StatusCode(500, new { message = "אירעה שגיאה במערכת" });
            }
        }

        [HttpPost("GetSong")]
        public async Task<IActionResult> GetSong(SongResult songResult)
        {
            try
            {
                var song = await _songService.GetSongByProviderAsync(songResult);

                _logger.LogInformation("שיר נטען בהצלחה: {SongId} - {SongName}", songResult.SongId, song.Name);

                return Ok(song);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "השיר לא נמצא" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בטעינת שיר: {SongId}", songResult.SongId);
                return StatusCode(500, new { message = "אירעה שגיאה במערכת" });
            }
        }
    }
}