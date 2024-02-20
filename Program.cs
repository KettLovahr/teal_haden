using Discord;
using Discord.Rest;
using Discord.WebSocket;

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
        await _DeleteDormsThreadIfAlreadyPresent(chan);
        return;
    }

    public static async Task ReactionAdded(Cacheable<IUserMessage, ulong> msg, Cacheable<IMessageChannel, ulong> chan, SocketReaction reaction)
    {
        var message = await msg.GetOrDownloadAsync();
        var channel = await chan.GetOrDownloadAsync();

        await _PinMessageOnReact(message, channel, reaction);
    }

    public static async Task ReactionRemoved(Cacheable<IUserMessage, ulong> msg, Cacheable<IMessageChannel, ulong> chan, SocketReaction reaction)
    {
        var message = await msg.GetOrDownloadAsync();
        var channel = await chan.GetOrDownloadAsync();

        await _UnpinMessageOnReact(message, channel, reaction);
    }

    private static async Task _PinMessageOnReact(IUserMessage message, IMessageChannel channel, SocketReaction reaction)
    {
        Emoji pin_react = new Emoji("📌");

        if (reaction.Emote.ToString() == pin_react.ToString())
        {
            int count = message.Reactions[pin_react].NormalCount;
            // Pin message if it gets enough Pin reactions
            if (count >= 5) { await message.PinAsync(); }
            // Pin message if it is in a thread and the reaction was given
            // by the author of the thread
            if (channel is SocketThreadChannel threadchan)
            {
                if (threadchan.Owner.Id == reaction.UserId) { await message.PinAsync(); }
            }
        }
    }
    private static async Task _UnpinMessageOnReact(IUserMessage message, IMessageChannel channel, SocketReaction reaction)
    {
        Emoji pin_react = new Emoji("📌");

        if (reaction.Emote.ToString() == pin_react.ToString())
        {
            if (channel is SocketThreadChannel threadchan)
            {
                if (threadchan.Owner.Id == reaction.UserId) { await message.UnpinAsync(); }
            }
        }
    }


    private static async Task _DeleteDormsThreadIfAlreadyPresent(SocketThreadChannel chan)
    {
        var parent = chan.ParentChannel as SocketForumChannel;
        if (parent != null)
        {
            var threads = await parent.GetActiveThreadsAsync();
            if (parent.Name == "dorms")
            {
                foreach (RestThreadChannel thread in threads)
                {
                    // Different thread with the same owner found
                    if (thread.OwnerId == chan.Owner.Id && thread.Id != chan.Id)
                    {
                        try
                        {
                            await chan.Owner.SendMessageAsync(String.Format(
                                        "Hello, it seems like you already created a thread in the dorms: https://discord.com/channels/{0}/{1}\nYou may only have one thread in the dorms, so the thread you tried to create just now was deleted. If there was some kind of mistake, please contact Kett!",
                                        thread.GuildId,
                                        thread.Id
                                        ));
                        }
                        catch
                        {
                            Console.WriteLine("User {0} can't receive direct messages", chan.Owner.Username);
                        }
                        Console.WriteLine("User already created a thread '{0}'", thread.Name);
                        await chan.DeleteAsync();
                        break;
                    }
                }
            }
        }
    }

}
