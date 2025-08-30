using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JaMoveo.Infrastructure.Entities
{
    public class SongWord
    {
        [Key]
        public int Id { get; set; }

        public string Chords { get; set; }
        
        public string Lyrics { get; set; }

        public int SongId { get; set; }
        public virtual Song Song { get; set; }
    }
}
