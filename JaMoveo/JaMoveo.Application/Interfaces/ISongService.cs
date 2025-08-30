using JaMoveo.Application.Providers;
using JaMoveo.Core.DTOs;
using JaMoveo.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JaMoveo.Application.Interfaces
{
    public interface ISongService
    {
        Task<Tab4USearchResponse> SearchSongsAsync(string query, int page);
        Task<SongDto> GetSongByIdAsync(int id );
        Task<SongDto> GetSongByProviderAsync(SongResult songResult);
    }
}
