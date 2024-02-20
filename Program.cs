using Discord;
using Discord.Rest;
using Discord.WebSocket;
using teal_haden;

internal class Program
{
    private static DiscordSocketClient? _client;

    private static async Task Main(string[] args)
    {
        string token = args[0];

        DiscordSocketConfig config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.All,
            MessageCacheSize = 100,
        };
        _client = new DiscordSocketClient(config);

        _client.Log += Log;
        //_client.MessageReceived += MessageReceived;
        _client.ThreadCreated += ThreadCreated;
        _client.ReactionAdded += ReactionAdded;
        _client.ReactionRemoved += ReactionRemoved;


        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await Task.Delay(-1);
    }

    public static Task Log(LogMessage msg)
    {
        Console.WriteLine("LOG- {0}", msg.Message);
        return Task.CompletedTask;
    }

    // Unused
    public static Task MessageReceived(SocketMessage msg)
    {
        return Task.CompletedTask;
    }

    public static async Task ThreadCreated(SocketThreadChannel chan)
    {
        await Behaviors._DeleteDormsThreadIfAlreadyPresent(chan);
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


}
