namespace teal_haden;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

internal class Program
{
    private static DiscordSocketClient? _client;

    public struct Config
    {
        public string token;
        public ulong dorms_role;
    }

    private static async Task Main(string[] args)
    {
        Config bot_config = GetConfig();

        DiscordSocketConfig socket_config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.All,
            MessageCacheSize = 100,
        };
        _client = new DiscordSocketClient(socket_config);

        _client.Log += Log;
        _client.ThreadCreated += ThreadCreated;
        _client.ReactionAdded += ReactionAdded;
        _client.ReactionRemoved += ReactionRemoved;


        await _client.LoginAsync(TokenType.Bot, bot_config.token);
        await _client.StartAsync();

        await Task.Delay(-1);
    }

    public static Task Log(LogMessage msg)
    {
        Console.WriteLine("LOG- {0}", msg.Message);
        return Task.CompletedTask;
    }

    public static async Task ThreadCreated(SocketThreadChannel chan)
    {
        await Behaviors._DormsThreadCheck(chan, GetConfig().dorms_role);
        return;
    }

    public static async Task ReactionAdded(Cacheable<IUserMessage, ulong> msg, Cacheable<IMessageChannel, ulong> chan, SocketReaction reaction)
    {
        var message = await msg.GetOrDownloadAsync();
        var channel = await chan.GetOrDownloadAsync();

        await Behaviors._PinMessageOnReact(message, channel, reaction);
    }

    public static async Task ReactionRemoved(Cacheable<IUserMessage, ulong> msg, Cacheable<IMessageChannel, ulong> chan, SocketReaction reaction)
    {
        var message = await msg.GetOrDownloadAsync();
        var channel = await chan.GetOrDownloadAsync();

        await Behaviors._UnpinMessageOnReact(message, channel, reaction);
    }

    private static Config GetConfig()
    {
        return JsonConvert.DeserializeObject<Config>(File.ReadAllText("./config.json"));
    }

}
