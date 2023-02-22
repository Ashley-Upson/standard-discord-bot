using System.IO.Compression;

public class TheStandard
{
    StandardToCEntry[]? toc = null;

    public async Task<StandardToCEntry[]> SearchAsync(string searchTerm)
    {
        if (toc is null)
            await Download();

        Console.WriteLine($"Searching The Standard for {searchTerm} ... ");
        var results = toc.Where(e => e.Title.ToLower().Contains(searchTerm.ToLower())).ToArray();

        return results;
    }

    public async Task<StandardToCEntry[]> GetTableOfContents()
    {
        if (toc is null)
            await Download();
       
        return toc;
    }



    public async Task<Stream> Download()
    {
        try
        {
            var client = new HttpClient() { BaseAddress = new Uri("https://github.com/") };
            var stream = await client.GetStreamAsync("hassanhabib/The-Standard/archive/refs/heads/master.zip");

            toc ??= Parse(new ZipArchive(stream));

            return stream;

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    StandardToCEntry[] Parse(ZipArchive standardZip) =>
        standardZip.Entries
            .Where(e => e.Name.EndsWith(".md"))
            .SelectMany(GetContentFor)
            .ToArray();

    IEnumerable<StandardToCEntry> GetContentFor(ZipArchiveEntry standardFile)
    {
        var fileContents = new StreamReader(standardFile.Open()).ReadToEnd();
        var lines = fileContents.Split("\n").ToList();

        var fileEntry = new StandardToCEntry
        {
            Title = standardFile.Name,
            FilePath = standardFile.FullName, 
            Link = "https://github.com/hassanhabib/The-Standard/blob/master/" + standardFile.FullName.Replace("The-Standard-master/", ""),
            Content = fileContents.Trim()
        };

        var sectionHeaders = lines.Where(l => l.StartsWith("#"));

        return new StandardToCEntry[] { fileEntry }
            .Union(
                sectionHeaders.Select(sectionHeader =>
                {
                    var headerLineIndex = lines.FindIndex(l => l == sectionHeader);
                    var lastLineIndex = lines.FindIndex(headerLineIndex + 1, l => l.StartsWith("#")) - 2;

                    if (lastLineIndex == -2)
                        lastLineIndex = lines.Count;

                    return new StandardToCEntry
                    {
                        Title = sectionHeader,
                        FilePath = standardFile.FullName,
                        Link = "https://github.com/hassanhabib/The-Standard/blob/master/" + standardFile.FullName.Replace("The-Standard-master/", ""),
                        Content = GetContentSection(lines, headerLineIndex, lastLineIndex)
                    };
                })
            );
    }

    string GetContentSection(IEnumerable<string> lines, int from, int to) =>
        string.Join("\n", lines.Skip(from).Take(to)).Trim();
}