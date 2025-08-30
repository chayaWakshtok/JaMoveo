using JaMoveo.Application.Interfaces;
using JaMoveo.Core.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JaMoveo.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp(SignUpRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _authService.SignUpAsync(request);

                return Ok(new { message = "המשתמש נוצר בהצלחה" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה ברישום משתמש: {Username}", request.Username);
                return StatusCode(500, new { message = "אירעה שגיאה במערכת" });
            }
        }

        [HttpPost("signup-admin")]
        public async Task<IActionResult> SignUpAdmin(SignUpRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _authService.SignUpAdminAsync(request);

                return Ok(new { message = "משתמש מנהל נוצר בהצלחה" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה ברישום מנהל: {Username}", request.Username);
                return StatusCode(500, new { message = "אירעה שגיאה במערכת" });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var response = await _authService.LoginAsync(request);

                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בהתחברות עבור: {Username}", request.Username);
                return StatusCode(500, new { message = "אירעה שגיאה במערכת" });
            }
        }

        [HttpGet("check-username/{username}")]
        public async Task<IActionResult> CheckUsername(string username)
        {
            try
            {
                var isAvailable = await _authService.IsUsernameAvailableAsync(username);
                return Ok(new { available = isAvailable });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בבדיקת זמינות שם משתמש: {Username}", username);
                return StatusCode(500, new { message = "אירעה שגיאה במערכת" });
            }
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized();
                }

                var userId = int.Parse(userIdClaim.Value);
                var user = await _authService.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return NotFound(new { message = "משתמש לא נמצא" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בקבלת פרופיל משתמש");
                return StatusCode(500, new { message = "אירעה שגיאה במערכת" });
            }
        }
    }
}
