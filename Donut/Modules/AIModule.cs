using Discord;
using Discord.Interactions;
using Newtonsoft.Json.Linq;

namespace Donut.Modules
{
    public class AIModule : InteractionModuleBase<SocketInteractionContext>
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        [SlashCommand("ask", description: "Ask sakura ai", runMode: RunMode.Async)]
        public async Task Ask(string prompt)
        {
            await DeferAsync();
            var responseString = await _httpClient.GetAsync($"https://llm.jirayu.pw/{Uri.EscapeDataString(prompt)}");
            var result = responseString.Content.ReadAsStringAsync().Result;
            JObject jsonObject = JObject.Parse(result);
            var embed = new EmbedBuilder()
                .WithColor(16761035)
                .WithTitle("SAKURA AI")
                .AddField("Prompt", $"> {prompt}")
                .WithDescription(jsonObject["response"]["response"].ToString())
                .WithCurrentTimestamp();

            await ModifyOriginalResponseAsync(message =>
            {
                message.Content = "";
                message.Embed = embed.Build();
            });
        }
    }
}
