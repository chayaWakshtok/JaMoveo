using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JaMoveo.Application.Providers
{
    public static class Tab4USongParser
    {
        public static SongContent ParseSongPage(string html, string url = "")
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var songContent = new SongContent();

            // Extract song ID from URL if provided
            if (!string.IsNullOrEmpty(url))
            {
                var idMatch = System.Text.RegularExpressions.Regex.Match(url, @"songs/(\d+)_");
                if (idMatch.Success)
                {
                    songContent.SongId = int.Parse(idMatch.Groups[1].Value);
                }
            }

            // Extract song title - look for h1 with song title pattern
            var titleNode = doc.DocumentNode.SelectSingleNode("//h1[contains(text(), 'אקורדים לשיר')]");
            if (titleNode != null)
            {
                var titleText = titleNode.InnerText;
                // Extract title from "אקורדים לשיר [TITLE] של [ARTIST]"
                var titleMatch = System.Text.RegularExpressions.Regex.Match(titleText, @"אקורדים לשיר\s+(.+?)\s+של");
                if (titleMatch.Success)
                {
                    songContent.Title = titleMatch.Groups[1].Value.Trim();
                }
            }

            // Extract artist names
            var artistLinks = doc.DocumentNode.SelectNodes("//a[@class='artistTitle rightKotInSong hT']");
            if (artistLinks != null)
            {
                foreach (var artistLink in artistLinks)
                {
                    var artistName = artistLink.InnerText.Trim();
                    if (!string.IsNullOrEmpty(artistName))
                    {
                        songContent.Artists.Add(artistName);
                    }
                }
                songContent.Artist = string.Join(" ו", songContent.Artists);
            }

            // Extract category
            var categoryLink = doc.DocumentNode.SelectSingleNode("//a[@class='catLinkInSong hT']");
            if (categoryLink != null)
            {
                songContent.Category = categoryLink.InnerText.Trim();
            }

            // Extract main song content
            var contentNode = doc.DocumentNode.SelectSingleNode("//div[@id='songContentTPL']");
            if (contentNode != null)
            {
                songContent.ChordsAndLyrics = contentNode.InnerText;

                // Parse structured chord and lyric data
                ParseStructuredContent(contentNode, songContent);
            }

            return songContent;
        }

        private static void ParseStructuredContent(HtmlNode contentNode, SongContent songContent)
        {
            var tables = contentNode.SelectNodes(".//table");
            if (tables == null) return;

            ChordLyricSection currentSection = null;
            string currentSectionType = "verse";

            foreach (var table in tables)
            {
                // Check if this table contains a section header
                var sectionHeader = table.SelectSingleNode(".//span[@class='titLine']");
                if (sectionHeader != null)
                {
                    currentSectionType = sectionHeader.InnerText.Trim().TrimEnd(':');
                    currentSection = new ChordLyricSection { SectionType = currentSectionType };
                    songContent.Sections.Add(currentSection);
                    continue;
                }

                // If no current section, create a default one
                if (currentSection == null)
                {
                    currentSection = new ChordLyricSection { SectionType = currentSectionType };
                    songContent.Sections.Add(currentSection);
                }

                ParseTableForWordFormat(table, currentSection, songContent);
            }

        }

        private static string ExtractChordsWithPositions(HtmlNode chordCell, Dictionary<string, string> chordDefinitions)
        {
            // First try to get chords from spans
            var chordSpans = chordCell.SelectNodes(".//span[contains(@class, 'c_C')]");

            if (chordSpans != null)
            {
                foreach (var span in chordSpans)
                {
                    var chordName = span.InnerText.Trim();
                    if (!string.IsNullOrEmpty(chordName))
                    {
                        // Extract chord definition from onmouseover attribute
                        var onMouseOver = span.GetAttributeValue("onmouseover", "");
                        if (!string.IsNullOrEmpty(onMouseOver))
                        {
                            var defMatch = System.Text.RegularExpressions.Regex.Match(onMouseOver, @"'([^']+)'");
                            if (defMatch.Success && !chordDefinitions.ContainsKey(chordName))
                            {
                                chordDefinitions[chordName] = defMatch.Groups[1].Value;
                            }
                        }
                    }
                }
            }

            // Return the full text with positioning preserved
            return chordCell.InnerText ?? "";
        }

        private static void ParseTableForWordFormat(HtmlNode table, ChordLyricSection section, SongContent songContent)
        {
            var rows = table.SelectNodes(".//tr");
            if (rows == null) return;

            string chordLine = "";
            string lyricLine = "";

            foreach (var row in rows)
            {
                var chordCell = row.SelectSingleNode(".//td[@class='chords']");
                var lyricCell = row.SelectSingleNode(".//td[@class='song']");

                if (chordCell != null)
                {
                    chordLine = ExtractChordsWithPositions(chordCell, songContent.ChordDefinitions);
                }
                else if (lyricCell != null)
                {
                    lyricLine = CleanLyricsText(lyricCell.InnerText);

                    // Process the chord/lyric pair
                    if (!string.IsNullOrEmpty(lyricLine.Trim()))
                    {
                        // Also add to legacy format
                        var legacyLine = new ChordLyricLine
                        {
                            Chords = chordLine,
                            Lyrics = lyricLine,
                        };
                        section.Lines.Add(legacyLine);
                    }
                    // Reset for next pair
                    chordLine = "";
                    lyricLine = "";
                }
            }
        }


        private static string CleanLyricsText(string text)
        {
            // Replace HTML entities and clean up spacing
            return System.Web.HttpUtility.HtmlDecode(text)
                .Replace("&nbsp;", " ")
                .Replace("  ", " ")
                .Trim();
        }
       
    }
}
