using JaMoveo.Application.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JaMoveo.Application.Interfaces
{
    public interface IExternalSongProvider
    {
        Task<Tab4USearchResponse> FetchSongAsync(string query, int page);
        Task<SongContent> GetSongDetailsAsync(string songUrl);
    }
}
