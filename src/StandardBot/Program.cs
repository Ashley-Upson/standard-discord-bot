using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StandardBot;
using StandardBot.Modules;
using System.Reflection;

public class Program
{
    private DiscordSocketClient discordClient;
    private CommandService commandService;

    static async Task Main(string[] args)
    {
        Console.WriteLine("Initialising ...");
        var program = new Program();

        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .Build();

        await StandardModule.Standard.Download();
        await program.BuildHost(config);
    }

    async Task BuildHost(IConfiguration config)
    {
        Console.WriteLine("  Building host");

        using IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) => ConfigureServices(services, config))
            .Build();

        await RunAsync(host);
    }

    static void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        Console.WriteLine("  Configuring Services");

        // Add the configuration to the registered services
        services.AddSingleton(config);

        // Add the DiscordSocketClient, along with specifying the GatewayIntents and user caching
        services.AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged,
            LogGatewayIntentWarnings = false,
            AlwaysDownloadUsers = true,
            LogLevel = LogSeverity.Debug
        }));

        // Used for slash commands and their registration with Discord
        services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));

        // Required to subscribe to the various client events used in conjunction with Interactions
        services.AddSingleton<InteractionHandler>();

        // Adding the prefix Command Service
        services.AddSingleton(x => new CommandService(new CommandServiceConfig
        {
            LogLevel = LogSeverity.Debug,
            DefaultRunMode = Discord.Commands.RunMode.Async
        }));
    }

    async Task RunAsync(IHost host)
    {
        Console.WriteLine("  Connecting to discord");

        IServiceProvider provider = host.Services;

        var commands = provider.GetRequiredService<InteractionService>();
        discordClient = provider.GetRequiredService<DiscordSocketClient>();
        var config = provider.GetRequiredService<IConfiguration>();

        await provider.GetRequiredService<InteractionHandler>().InitializeAsync();

        // Subscribe to log events
        discordClient.Log += Log;
        commands.Log += Log;

        discordClient.Ready += async () =>
        {
            await commands.RegisterCommandsGloballyAsync(true);
        };

        await discordClient.LoginAsync(
            TokenType.Bot, 
            Environment.GetEnvironmentVariable("StandardBotToken", EnvironmentVariableTarget.Machine)
        );

        await discordClient.StartAsync();

        Console.WriteLine("Done - Ready to receive commands!");

        await Task.Delay(-1);
    }

    Task Log(LogMessage arg)
    {
        Console.WriteLine(arg.Message);
        return Task.CompletedTask;
    }
}
