using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JaMoveo.Core.DTOs
{
    public class SongDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Artist { get; set; }
        public string ImageUrl { get; set; }

        public List<List<WordChordPair>> Lines { get; set; } = new List<List<WordChordPair>>();

        public string Language { get; set; }
    }

    
}
