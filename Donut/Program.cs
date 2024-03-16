
using Discord.Interactions;
using Lavalink4NET.Extensions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Donut.Services;
using Microsoft.Extensions.Logging;
using Lavalink4NET.InactivityTracking.Trackers.Idle;
using Lavalink4NET.InactivityTracking.Trackers.Users;
using Lavalink4NET.InactivityTracking.Extensions;
using Lavalink4NET.InactivityTracking;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.AddYamlFile("_config.yml", false);
    })
    .ConfigureServices(services =>
    {
        services.AddSingleton<DiscordSocketClient>();
        services.AddSingleton<InteractionService>();
        services.AddHostedService<InteractionHandlingService>();
        services.AddHostedService<DiscordStartupService>();
        services.AddLogging(x => x.AddConsole().SetMinimumLevel(LogLevel.Trace));
        services.AddLavalink();
        services.Configure<IdleInactivityTrackerOptions>(options =>
        {
            options.Timeout = TimeSpan.FromSeconds(10);
        });
        services.Configure<UsersInactivityTrackerOptions>(config =>
        {
            config.Timeout = TimeSpan.FromSeconds(10);
        });
        services.ConfigureInactivityTracking(options =>
        {
            options.DefaultTimeout = TimeSpan.FromSeconds(30);
            options.DefaultPollInterval = TimeSpan.FromSeconds(5);
            options.TrackingMode = InactivityTrackingMode.Any;
            options.UseDefaultTrackers = true;
        });
        services.ConfigureLavalink(config =>
        {
            config.BaseAddress = new Uri("http://lavalink.jirayu.pw:2333");
            config.WebSocketUri = new Uri("ws://lavalink.jirayu.pw:2333/v4/websocket");
            config.ReadyTimeout = TimeSpan.FromSeconds(10);
            config.Label = "SAKURA";
            config.Passphrase = "youshallnotpass";
            config.HttpClientName = "SAKURA";
        });
    })
    .Build();

await host.RunAsync();