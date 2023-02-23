using System.IO.Compression;
using Discord;

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
                currentSection.Content += $"{line}\n";
        }

        sections.Add(currentSection);

        return sections.ToArray();
    }

    public List<StandardEmbedItem> PrepareContentForEmbed(string content)
    {
        List<StandardEmbedItem> items = null;

        foreach(string item in content.Split("\n").ToArray())
        {
            var type = item.Contains("<") && item.Contains(">")
                ? (item.Contains("img") ? "image" : "html")
                : "text";

            var itemContent = "";

            if (type == "image")
            {
                var httpStart = item.IndexOf("http");
                var endQuote = item.IndexOf('"', httpStart);
                itemContent = item.Substring(httpStart, (endQuote - httpStart));
            } else if (type == "text")
                itemContent = item;
            
            items.Add(new StandardEmbedItem
            {
                Type = type,
                Content = itemContent
            });
        }
        
        return items;
    }
    
    public Embed[] BuildEmbedResponse(StandardToCEntry[] contents)
    {
        List<Embed> embeds = null;

        if (contents.Length == 1)
        {
            var content = contents[0];

            embeds.Add(new EmbedBuilder().WithTitle(content.Title).WithDescription(content.Link).Build());
            
            // embedBuilder.WithTitle(content.Title);
            // embedBuilder.WithDescription(content.Link);
            // embedBuilder.WithImageUrl(item.Content);
            // embedBuilder.AddField("More Text", item.Content);

            foreach (var item in PrepareContentForEmbed(content.Content))
            {
                if (item.Type == "image")
                    embeds.Add(new EmbedBuilder().WithImageUrl(item.Content).Build());
                else
                {
                    embeds.Add(new EmbedBuilder().WithDescription(item.Content).Build());
                }
            }
        }
        else
        {
            embeds.Add(new EmbedBuilder().WithTitle("Here are the results I found for your search:").Build());
            // embedBuilder.WithTitle();
            
            foreach (var content in contents)
            {
                embeds.Add(new EmbedBuilder().WithDescription(content.Title).Build());
                embeds.Add(new EmbedBuilder().WithUrl(content.Link).Build());
                // embedBuilder.AddField("More Text", content.Title);
                // embedBuilder.WithUrl(content.Link);
            }
        }


        return embeds.ToArray();
    }
}