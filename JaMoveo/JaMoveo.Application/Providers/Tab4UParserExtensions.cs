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
        public List<List<WordChordPair>> Lines { get; set; } = new List<List<WordChordPair>>();

        public List<TraditionalLine> ToTraditionalFormat()
        {
            var traditionalLines = new List<TraditionalLine>();

            foreach (var line in Lines)
            {
                var traditional = ConvertLineToTraditional(line);
                if (traditional != null)
                {
                    traditionalLines.Add(traditional);
                }
            }

            return traditionalLines;
        }

        private TraditionalLine ConvertLineToTraditional(List<WordChordPair> wordPairs)
        {
            if (wordPairs == null || wordPairs.Count == 0)
                return null;

            var chordsLine = new StringBuilder();
            var lyricsLine = new StringBuilder();

            foreach (var pair in wordPairs)
            {
                var word = pair.Lyrics ?? "";
                var chord = pair.Chords ?? "";

                // Check if this is a section header
                if (word.StartsWith("[") && word.EndsWith("]"))
                {
                    return new TraditionalLine
                    {
                        ChordsLine = "",
                        LyricsLine = word,
                        IsSectionHeader = true
                    };
                }

                // Add chord above the word position
                if (!string.IsNullOrEmpty(chord))
                {
                    // Add spaces to align chord position
                    while (chordsLine.Length < lyricsLine.Length)
                    {
                        chordsLine.Append(" ");
                    }
                    chordsLine.Append(chord);
                }

                // Add the word to lyrics line
                if (lyricsLine.Length > 0)
                {
                    lyricsLine.Append(" ");
                }
                lyricsLine.Append(word);

                // If we added a chord, pad it to at least match the word length
                if (!string.IsNullOrEmpty(chord))
                {
                    int targetLength = lyricsLine.Length;
                    while (chordsLine.Length < targetLength)
                    {
                        chordsLine.Append(" ");
                    }
                }
            }

            return new TraditionalLine
            {
                ChordsLine = chordsLine.ToString(),
                LyricsLine = lyricsLine.ToString(),
                IsSectionHeader = false
            };
        }
    }
}

    public class TraditionalLine
    {
        public string ChordsLine { get; set; }
        public string LyricsLine { get; set; }
        public bool IsSectionHeader { get; set; }

        public override string ToString()
        {
            if (IsSectionHeader)
            {
                return LyricsLine;
            }

            var result = new StringBuilder();
            if (!string.IsNullOrEmpty(ChordsLine.Trim()))
            {
                result.AppendLine(ChordsLine);
            }
            result.Append(LyricsLine);
            return result.ToString();
        }
    }



    public class WordChordPair
    {
        public string Lyrics { get; set; }
        public string Chords { get; set; }

        // Constructor for JSON serialization
        public WordChordPair() { }

        public WordChordPair(string lyrics, string chords = null)
        {
            this.Lyrics = lyrics;
            this.Chords = chords;
        }
    }


