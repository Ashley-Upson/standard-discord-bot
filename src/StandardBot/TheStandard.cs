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

    StandardToCEntry[] Parse(ZipArchive standardZip)
    {
        foreach (ZipArchiveEntry entry in standardZip.Entries)
        {
            Console.WriteLine(entry.FullName);
            if (entry.FullName == "The-Standard-master/README.md")
            {
                using (var stream = entry.Open())
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd()
                        .Split("\n")
                        .Skip(6)
                        .Select(l => l.Trim())
                        .Where(l => l.Length > 0 && !l.StartsWith("##"))
                        .Select(l => new StandardToCEntry
                        {
                            Title = l.Split('(')[0].Replace("- ", "").Trim("[]".ToArray()),
                            Link = l.Split('(')[1].Split(')')[0]
                        })
                        .Select(i =>
                        {
                            i.ContentReference = i.Link.Replace("https://github.com/hassanhabib/The-Standard/blob/master/", "").Replace("%20", " ");
                            i.FilePath = i.ContentReference.Split("#")[0].Replace("%20", " ");
                            i.Content = GetContentFor(standardZip.Entries.FirstOrDefault(e => e.FullName == $"The-Standard-master/{i.FilePath}"), i.ContentReference);
                            return i;
                        })
                        .ToArray();
                }
            }
        }

        return Array.Empty<StandardToCEntry>();
    }
    string GetContentFor(ZipArchiveEntry standardChapterEntry, string contentReference)
    {
        if (standardChapterEntry is null)
            return null;

        var refParts = contentReference.Split("#");

        if (refParts.Length == 2)
        {
            var sectionHeader = "# " + refParts[1].Replace("-", " ").ToLowerInvariant();
            var allContent = new StreamReader(standardChapterEntry.Open()).ReadToEnd();

            List<string> lines = allContent.Split('\n').ToList();
            var headerLineIndex = lines.FindIndex(l => l.Replace(".", "").ToLowerInvariant() == sectionHeader);
            var lastLineIndex = lines.FindIndex(headerLineIndex + 1, l => l.StartsWith("#")) -1;

            if (lastLineIndex == -2)
                lastLineIndex = allContent.Length;

            return string.Join("\n", lines.Where(l => l.Length > 0).Skip(headerLineIndex).Take(lastLineIndex - headerLineIndex));
        }
        else
            return new StreamReader(standardChapterEntry.Open()).ReadToEnd();
    }
}
