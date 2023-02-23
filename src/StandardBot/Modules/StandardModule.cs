using Discord;
using Discord.Interactions;
using Newtonsoft.Json;

namespace StandardBot.Modules
{
    public class StandardModule : InteractionModuleBase<SocketInteractionContext>
    {
        public static TheStandard Standard = new();

        public InteractionService Commands { get; set; }

        // Basic slash command. [SlashCommand("name", "description")]
        // Similar to text command creation, and their respective attributes
        [SlashCommand("standardping", "Receive a pong!")]
        public async Task StandardPing()
        {
            // Respond to the user
            await RespondAsync("pong");
        }

        [SlashCommand("greet", "say hello to the caller!")]
        public async Task Greet()
        {
            // Respond to the user
            await RespondAsync($"Hello {Context.User.Username}");
        }

        [SlashCommand("search", "say hello to the caller!")]
        public async Task Search(string term)
        {
            var contents = await Standard.SearchAsync(term);

            if (term.ToLower().Contains("php"))
                await RespondAsync("Go away Ash!");
            else if (contents.Length == 0)
                await RespondAsync("Have you read the standard at all, it doesn't seem to mention that!");
            else
            {
                Console.WriteLine("Building embed");
                var embeds = Standard.BuildEmbedResponse(contents);
                Console.WriteLine("Built");

                await RespondAsync("Here are the results I found:");
                
                foreach (var embed in embeds)
                {
                    await Task.Delay(200);
                    await FollowupAsync(embed: embed);
                }
                
                // await RespondAsync(embeds: embeds);

                // var response = string.Join("\n", contents.Select(c => FormatContent(c, contents.Length == 1)));

                // if (response.Length > 2000)
                // response = response[..1997] + "...";

                // await RespondAsync(response);
            }
        }

        string FormatContent(StandardToCEntry content, bool returnFullContent) =>
            returnFullContent
                ? $"{content.Title} - {content.Link}\n{content.Content}"
                : $"{content.Title} - {content.Link}";
    }
}