using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;

namespace StandardBot.Commands
{
    public class GreetCommand : ModuleBase<SocketCommandContext>
    {
        [SlashCommand("greet", "Greets the user.")]
        public async Task GreetAsync(SocketSlashCommand command)
        {
            string user = command.User.Mention;
            await command.RespondAsync($"Hello, {user}!");
        }
    }

}
