using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JaMoveo.Application.Providers
{
    public static class Tab4UParserExtensions
    {
        public static SongContent ParseSongContent(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var songContent = new SongContent();

            // Extract song title
            var titleNode = doc.DocumentNode.SelectSingleNode("//h1[@id='song_name']");
            if (titleNode != null)
            {
                songContent.Title = titleNode.InnerText.Trim();
            }

            // Extract artist
            var artistNode = doc.DocumentNode.SelectSingleNode("//span[@id='artist_name']");
            if (artistNode != null)
            {
                songContent.Artist = artistNode.InnerText.Trim();
            }

            // Extract chords and lyrics content
            var contentNode = doc.DocumentNode.SelectSingleNode("//div[@id='song_content']");
            if (contentNode != null)
            {
                songContent.ChordsAndLyrics = contentNode.InnerText;
            }

            return songContent;
        }
    }

    public class SongContent
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public List<string> Artists { get; set; } = new List<string>();
        public string ChordsAndLyrics { get; set; }
        public string Category { get; set; }
        public int SongId { get; set; }
        public List<ChordLyricSection> Sections { get; set; } = new List<ChordLyricSection>();
        public Dictionary<string, string> ChordDefinitions { get; set; } = new Dictionary<string, string>();

    }

    public class ChordLyricSection
    {
        public string SectionType { get; set; } // "verse", "chorus", "intro", "bridge", etc.
        public List<ChordLyricLine> Lines { get; set; } = new List<ChordLyricLine>();

    }

    public class ChordLyricLine
    {
        public string Chords { get; set; }
        public string Lyrics { get; set; }
        public List<ChordPosition> ChordPositions { get; set; } = new List<ChordPosition>();
    }

    public class ChordPosition
    {
        public string ChordName { get; set; }
        public int Position { get; set; }
        public string ChordDefinition { get; set; }
    }

}
