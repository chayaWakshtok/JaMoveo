// See https://aka.ms/new-console-template for more information
using JaMoveo.Application.Providers;

Console.WriteLine("Hello, World!");


var scraper = new Tab4UProvider();

try
{
    // Search for songs
    string searchTerm = "אבא מלך";
    var results = await scraper.FetchSongAsync(searchTerm, 1);

    Console.WriteLine($"Found {results.TotalResults} results for '{results.SearchTerm}'");
    Console.WriteLine($"Showing {results.Songs.Count} songs on this page");
    Console.WriteLine();

    foreach (var song in results.Songs)
    {
        Console.WriteLine($"Title: {song.Title}");
        Console.WriteLine($"Artist: {song.Artist}");
        Console.WriteLine($"URL: {song.Url}");
        Console.WriteLine($"Song ID: {song.SongId}");
        Console.WriteLine($"Has Tabs: {song.HasTabs}");
        Console.WriteLine($"Has Notes: {song.HasNotes}");
        Console.WriteLine(new string('-', 50));
    }

    // Get next page if available
    if (results.HasNextPage)
    {
        Console.WriteLine("Getting next page...");
        var nextPageResults = await scraper.FetchSongAsync(searchTerm, 2);
        Console.WriteLine($"Next page has {nextPageResults.Songs.Count} more songs");
    }

    // Example: Get content of the first song
    if (results.Songs.Count > 0)
    {
        Console.WriteLine("\nFetching content of first song...");
        var firstSong = results.Songs[1];
        var songDetails = await scraper.GetSongDetailsAsync(firstSong.Url);
        Console.WriteLine($"Title: {songDetails.Title}");
        Console.WriteLine($"Artist: {songDetails.Artist}");
        Console.WriteLine($"Artists: {string.Join(", ", songDetails.Artists)}");
        Console.WriteLine($"Category: {songDetails.Category}");
        Console.WriteLine($"Song ID: {songDetails.SongId}");
        Console.WriteLine($"Number of sections: {songDetails.Sections.Count}");
        Console.WriteLine($"Known chords: {string.Join(", ", songDetails.ChordDefinitions.Keys)}");

        Console.WriteLine("\nWord-by-word format (first 5 lines):");
        for (int i = 0; i < songDetails.Sections.Count; i++)
        {
            var line = songDetails.Sections[i];
            Console.WriteLine($"Line {i + 1}:");
            foreach (var pair in line.Lines)
            {
                if (!string.IsNullOrEmpty(pair.Chords))
                    Console.WriteLine($"  {pair.Chords}");
                Console.WriteLine($" {pair.Lyrics}");
            }
            Console.WriteLine();
        }


        // Here you would parse the song content to extract chords and lyrics
        //ParseSongContent(songContent);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

