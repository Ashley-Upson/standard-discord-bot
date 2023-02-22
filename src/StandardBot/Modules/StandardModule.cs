using Discord.Interactions;

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

            if (contents.Length == 0)
                await RespondAsync("Have you read the standard at all, it doesn't seem to mention that!");
            else 
            {
                var response = string.Join("\n", contents.Select(c => FormatContent(c, contents.Length == 1)));

                if (response.Length > 2000)
                    response = response[..1997] + "...";

                await RespondAsync(response);
            }
        }

        string FormatContent(StandardToCEntry content, bool returnFullContent) =>
            returnFullContent
                ? $"{content.Title} - {content.Link}\n{content.Content}"
                : $"{content.Title} - {content.Link}";
    }
}