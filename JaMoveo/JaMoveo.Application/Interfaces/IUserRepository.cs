using JaMoveo.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JaMoveo.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<ApplicationUser> GetByIdAsync(int id);
        Task<ApplicationUser> GetByUsernameAsync(string username);
        Task<bool> ExistsByUsernameAsync(string username);
        Task<ApplicationUser> UpdateAsync(ApplicationUser user);
        Task<bool> DeleteAsync(int id);
        Task<List<ApplicationUser>> GetAllAsync();
        Task<List<ApplicationUser>> GetActiveUsersAsync();
    }
}
