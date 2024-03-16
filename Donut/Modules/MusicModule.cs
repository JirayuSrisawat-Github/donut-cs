using Discord;
using Discord.Interactions;
using Lavalink4NET;
using Lavalink4NET.DiscordNet;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Rest.Entities.Tracks;


namespace Donut.Modules
{
    public class MusicModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IAudioService _audioService;
        public MusicModule(IAudioService audioService)
        {
            ArgumentNullException.ThrowIfNull(audioService);

            _audioService = audioService;
        }

        [SlashCommand("disconnect", "Disconnects from the current voice channel connected to", runMode: RunMode.Async)]
        public async Task Disconnect()
        {
            var player = await GetPlayerAsync().ConfigureAwait(false);

            if (player is null)
            {
                return;
            }

            var embed = new EmbedBuilder()
                .WithColor(16761035)
                .WithDescription("Disconnected.")
                .WithCurrentTimestamp()
                .Build();

            await player.DisconnectAsync().ConfigureAwait(false);
            await RespondAsync(null, [embed]).ConfigureAwait(false);
        }

        [SlashCommand("play", description: "Plays music", runMode: RunMode.Async)]
        public async Task Play(string query)
        {
            await DeferAsync().ConfigureAwait(false);

            var player = await GetPlayerAsync(connectToVoiceChannel: true).ConfigureAwait(false);

            if (player is null)
            {
                return;
            }

            var track = await _audioService.Tracks
                .LoadTrackAsync(query, TrackSearchMode.YouTube)
                .ConfigureAwait(false);

            var embed = new EmbedBuilder()
                .WithColor(16761035)
                .WithCurrentTimestamp();

            if (track is null)
            {
                embed.WithDescription("😖 No results.");
                await FollowupAsync(null, [embed.Build()]).ConfigureAwait(false);
                return;
            }

            var position = await player.PlayAsync(track).ConfigureAwait(false);

            if (position is 0)
            {
                embed.WithTitle("Starting playing music.");
                embed.WithFooter($"Source: {track.SourceName} | Duration: {track.Duration}");
                embed.WithDescription($"🔈 Playing: [{track.Title}]({track.Uri})");
                
                if (track.ArtworkUri is not null)
                {
                    embed.WithImageUrl(track.ArtworkUri.ToString());
                }

                await FollowupAsync(null, [embed.Build()]).ConfigureAwait(false);
            }
            else
            {
                embed.WithDescription($"🔈 Added to queue: {track.Uri}");
                await FollowupAsync(null, [embed.Build()]).ConfigureAwait(false);
            }
        }

        [SlashCommand("loop", description: "Repeat", runMode: RunMode.Async)]
        public async Task Loop()
        {
            var player = await GetPlayerAsync(connectToVoiceChannel: false).ConfigureAwait(false);

            if (player is null)
            {
                return;
            }

            var embed = new EmbedBuilder()
                .WithColor(16761035)
                .WithCurrentTimestamp();

            if (player.CurrentTrack is null)
            {
                embed.WithDescription("Nothing playing!");
                await RespondAsync(null, [embed.Build()]).ConfigureAwait(false);
                return;
            }

            player.RepeatMode = player.RepeatMode == TrackRepeatMode.Queue ? TrackRepeatMode.None : TrackRepeatMode.Queue;
            embed.WithDescription(player.RepeatMode == TrackRepeatMode.Queue ? "Disabled loop" : "Enabled loop");
            await RespondAsync(null, [embed.Build()]).ConfigureAwait(false);
        }

        [SlashCommand("position", description: "Shows the track position", runMode: RunMode.Async)]
        public async Task Position()
        {
            var player = await GetPlayerAsync(connectToVoiceChannel: false).ConfigureAwait(false);
            
            if (player is null)
            {
                return;
            }

            var embed = new EmbedBuilder()
                .WithColor(16761035)
                .WithCurrentTimestamp();

            if (player.CurrentTrack is null)
            {
                embed.WithDescription("Nothing playing!");
                await RespondAsync(null, [embed.Build()]).ConfigureAwait(false);
                return;
            }

            embed.WithDescription($"Position: {player.Position?.Position} / {player.CurrentTrack.Duration}.");
            await RespondAsync(null, [embed.Build()]).ConfigureAwait(false);
        }

        [SlashCommand("stop", description: "Stops the current track", runMode: RunMode.Async)]
        public async Task Stop()
        {
            var player = await GetPlayerAsync(connectToVoiceChannel: false);

            if (player is null)
            {
                return;
            }

            var embed = new EmbedBuilder()
                .WithColor(16761035)
                .WithCurrentTimestamp();

            if (player.CurrentItem is null)
            {
                embed.WithDescription("Nothing playing!");
                await RespondAsync(null, [embed.Build()]).ConfigureAwait(false);
                return;
            }

            embed.WithDescription("Stopped playing.");

            await player.StopAsync().ConfigureAwait(false);
            await RespondAsync(null, [embed.Build()]).ConfigureAwait(false);
        }

        [SlashCommand("volume", description: "Sets the player volume (0 - 1000%)", runMode: RunMode.Async)]
        public async Task Volume(int volume = 100)
        {
            var embed = new EmbedBuilder()
                .WithColor(16761035)
                .WithCurrentTimestamp();

            if (volume is > 1000 or < 0)
            {
                embed.WithDescription("Volume out of range: 0% - 1000%!");
                await RespondAsync(null, [embed.Build()]).ConfigureAwait(false);
                return;
            }

            var player = await GetPlayerAsync(connectToVoiceChannel: false).ConfigureAwait(false);

            if (player is null)
            {
                return;
            }

            embed.WithDescription($"Volume updated: {volume}%");

            await player.SetVolumeAsync(volume / 100f).ConfigureAwait(false);
            await RespondAsync(null, [embed.Build()]).ConfigureAwait(false);
        }

        [SlashCommand("skip", description: "Skips the current track", runMode: RunMode.Async)]
        public async Task Skip()
        {
            var player = await GetPlayerAsync(connectToVoiceChannel: false);

            if (player is null)
            {
                return;
            }

            var embed = new EmbedBuilder()
                .WithColor(16761035)
                .WithCurrentTimestamp();

            if (player.CurrentItem is null)
            {
                embed.WithDescription("Nothing playing!");
                await RespondAsync(null, [embed.Build()]).ConfigureAwait(false);
                return;
            }

            await player.SkipAsync().ConfigureAwait(false);

            var track = player.CurrentItem;

            if (track is not null)
            {
                embed.WithDescription($"Skipped. Now playing: {track.Track!.Uri}");
                await RespondAsync(null, [embed.Build()]).ConfigureAwait(false);
            }
            else
            {
                embed.WithDescription("Skipped. Stopped playing because the queue is now empty.");
                await RespondAsync(null, [embed.Build()]).ConfigureAwait(false);
            }
        }

        [SlashCommand("pause", description: "Pauses the player.", runMode: RunMode.Async)]
        public async Task PauseAsync()
        {
            var player = await GetPlayerAsync(connectToVoiceChannel: false);

            if (player is null)
            {
                return;
            }

            var embed = new EmbedBuilder()
                .WithColor(16761035)
                .WithCurrentTimestamp();

            if (player.State is PlayerState.Paused)
            {
                embed.WithDescription("Player is already paused.");
                await RespondAsync(null, [embed.Build()]).ConfigureAwait(false);
                return;
            }

            embed.WithDescription("Paused.");

            await player.PauseAsync().ConfigureAwait(false);
            await RespondAsync(null, [embed.Build()]).ConfigureAwait(false);
        }

        [SlashCommand("resume", description: "Resumes the player.", runMode: RunMode.Async)]
        public async Task ResumeAsync()
        {
            var player = await GetPlayerAsync(connectToVoiceChannel: false);

            if (player is null)
            {
                return;
            }

            var embed = new EmbedBuilder()
                .WithColor(16761035)
                .WithCurrentTimestamp();

            if (player.State is not PlayerState.Paused)
            {
                embed.WithDescription("Player is not paused.");
                await RespondAsync(null, [embed.Build()]).ConfigureAwait(false);
                return;
            }

            embed.WithDescription("Resumed.");

            await player.ResumeAsync().ConfigureAwait(false);
            await RespondAsync(null, [embed.Build()]).ConfigureAwait(false);
        }

        private async ValueTask<QueuedLavalinkPlayer?> GetPlayerAsync(bool connectToVoiceChannel = true)
        {
            var channelBehavior = connectToVoiceChannel
                ? PlayerChannelBehavior.Join
                : PlayerChannelBehavior.None;

            var retrieveOptions = new PlayerRetrieveOptions(ChannelBehavior: channelBehavior);

            var result = await _audioService.Players
                .RetrieveAsync(Context, playerFactory: PlayerFactory.Queued, retrieveOptions)
                .ConfigureAwait(false);

            if (!result.IsSuccess)
            {
                var errorMessage = result.Status switch
                {
                    PlayerRetrieveStatus.UserNotInVoiceChannel => "You are not connected to a voice channel.",
                    PlayerRetrieveStatus.BotNotConnected => "The bot is currently not connected.",
                    _ => "Unknown error.",
                };

                var embed = new EmbedBuilder()
                    .WithColor(16761035)
                    .WithDescription(errorMessage)
                    .WithCurrentTimestamp();

                await FollowupAsync(null, [embed.Build()]).ConfigureAwait(false);
                return null;
            }

            return result.Player;
        }
    }
}
