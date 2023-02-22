using Discord;
using Discord.Interactions;

namespace StandardBot.Modules
{
    public class StandardModule : InteractionModuleBase<SocketInteractionContext>
    {
        public static TheStandard Standard = new();

        public InteractionService Commands { get; set; }

        [SlashCommand("search", "search the standard")]
        public async Task Search(string term)
        {
            try
            {
                var results = await Standard.SearchAsync(term);

                if (results.Length == 0)
                    await RespondAsync("Have you read the standard at all, it doesn't seem to mention that!");
                else
                {
                    var response = string.Join("\n", results.Select(c => FormatContent(c, results.Length == 1)));

                    if (response.Length > 2000)
                        response = response[..1997] + "...";

                    var messages = results
                        .SelectMany(r => FormatContent(r, results.Length == 1))
                        .ToArray();

                    await RespondAsync(messages.First());

                    foreach (var followUp in messages.Skip(1))
                    {
                        await Task.Delay(200);
                        await FollowupAsync(followUp);
                    }
                }
            }
            catch (Exception ex) 
            { 
                Console.WriteLine(ex.Message);
            }
        }

        string[] FormatContent(StandardToCEntry content, bool returnFullContent)
        {
            if (!returnFullContent)
                return new[] { content.Title, content.Link };

            var contentParts = content.Content.Split("[[IMAGE]]");

            return new[] { content.Title, content.Link }
                .Union(contentParts)
                .ToArray();
                
        }
    }
}