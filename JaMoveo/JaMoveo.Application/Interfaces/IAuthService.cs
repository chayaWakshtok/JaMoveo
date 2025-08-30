using JaMoveo.Core.DTOs;
using JaMoveo.Core.DTOs.Auth;
using JaMoveo.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JaMoveo.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequest);
        Task<bool> SignUpAsync(SignUpRequestDto signUpRequest);
        Task<bool> SignUpAdminAsync(SignUpRequestDto signUpRequest);
        Task<bool> IsUsernameAvailableAsync(string username);
        Task<UserDto> GetUserByIdAsync(int userId);
        Task<string> GenerateJwtTokenAsync(ApplicationUser user);
        bool ValidateToken(string token);
    }
}
