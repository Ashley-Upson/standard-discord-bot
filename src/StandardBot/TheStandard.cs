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
        return Split(lines, standardFile.FullName);
    }

    string GetContentSection(IEnumerable<string> lines, int from, int to) =>
        string.Join("\n", lines.Skip(from).Take(from - to)).Trim();

    public static StandardToCEntry[] Split(IEnumerable<string> lines, string path)
    {
        var sections = new List<StandardToCEntry>();
        StandardToCEntry currentSection = null;

        foreach (var line in lines)
        {
            if (line.StartsWith("#"))
            {
                if(currentSection is not null)
                    sections.Add(currentSection);

                currentSection = new StandardToCEntry
                {
                    Title = line,
                    FilePath = path,
                    Link = $"https://github.com/hassanhabib/The-Standard/blob/master/{path.Replace("The-Standard-master/", "")}".Replace(" ", "%20"),
                    Content = ""
                };
            }
            else
            {
                // Add the line to the current section
                currentSection.Content += $"{line}\n";
            }
        }

        sections.Add(currentSection);

        return sections.ToArray();
    }
}