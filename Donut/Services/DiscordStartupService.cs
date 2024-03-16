using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Donut.Services
{
    public class DiscordStartupService : IHostedService
    {
        private readonly DiscordSocketClient _discord;
        private readonly IConfiguration _config;
        private readonly ILogger<DiscordSocketClient> _logger;

        public DiscordStartupService(DiscordSocketClient discord, IConfiguration config, ILogger<DiscordSocketClient> logger)
        {
            _discord = discord;
            _config = config;
            _logger = logger;

            _discord.Log += msg => LogHelper.OnLogAsync(_logger, msg);
            _discord.Ready += OnReady;
            _discord.Log += LogAsync;
        }

        private Task OnReady()
        {
            Console.WriteLine($"Connected to these servers as '{_discord.CurrentUser.Username}': ");
            foreach (var guild in _discord.Guilds)
                Console.WriteLine($"- {guild.Name}");

            _discord.SetStatusAsync(UserStatus.Idle);
            _discord.SetGameAsync("DONUT!",
                type: ActivityType.Watching);
            Console.WriteLine($"Activity set to '{_discord.Activity.Name}'");

            return Task.CompletedTask;
        }

        private static Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());

            return Task.CompletedTask;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _discord.LoginAsync(TokenType.Bot, _config["token"]);
            await _discord.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _discord.LogoutAsync();
            await _discord.StopAsync();
        }
    }
}