using System.IO.Compression;

public class TheStandard
{
    public async Task<StandardToCEntry[]> SearchAsync(string searchTerm)
    {
        StandardToCEntry[] toc = await GetTableOfContents();

        Console.WriteLine($"Searching The Standard for {searchTerm} ... ");
        return toc.Where(e => e.Title.Contains(searchTerm)).ToArray();
    }

    public async Task<StandardToCEntry[]> GetTableOfContents()
    {
        Stream standardZipStream = await Download();
        var standardZip = new ZipArchive(standardZipStream);

        StandardToCEntry[]? toc = null;

        foreach (ZipArchiveEntry entry in standardZip.Entries)
        {
            if (entry.Name == "README.md")
            {
                Console.WriteLine(entry.FullName);
                using (var stream = entry.Open())
                using (var reader = new StreamReader(stream))
                {
                    toc = reader.ReadToEnd()
                        .Split("\n")
                        .Skip(6)
                        .Select(l => l.Trim())
                        .Where(l => l.Length > 0 && !l.StartsWith("##"))
                        .Select(l => new StandardToCEntry
                        {
                            Title = l.Split('(')[0].Replace("- ", "").Trim("[]".ToArray()),
                            Link = l.Split('(')[1].Split(')')[0]
                        })
                        .ToArray();
                }
            }
        }

        return toc;
    }

    public async Task<Stream> Download()
    {
        var client = new HttpClient() { BaseAddress = new Uri("https://github.com/") };
        return await client.GetStreamAsync("hassanhabib/The-Standard/archive/refs/heads/master.zip");
    }
}
