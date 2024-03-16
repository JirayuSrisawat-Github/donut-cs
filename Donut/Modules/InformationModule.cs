using Discord;
using Discord.Interactions;
using Donut.Utility;

namespace Donut.Modules
{
    public class InformationModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("info", description: "Get information about bot", runMode: RunMode.Async)]
        public async Task Info()
        {
            var metrics = new MemoryMetricsClient().GetMetrics();
            var embed = new EmbedBuilder()
                .WithColor(16761035)
                .WithTitle("DONUT Information")
                .WithDescription($"> Framework: `Discord.NET v3.13.1`\n> Memory: {Math.Floor(metrics.Used / 1024)}GB/{Math.Floor(metrics.Total / 1024)}GB")
                .WithCurrentTimestamp()
                .Build();

            await RespondAsync(null, [embed]).ConfigureAwait(false);
        }
    }
}