﻿namespace TealHadenBehaviors;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

public class Behaviors
{
    public static async Task _PinMessageOnReact(IUserMessage message, IMessageChannel channel, SocketReaction reaction)
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
    public static async Task _UnpinMessageOnReact(IUserMessage message, IMessageChannel channel, SocketReaction reaction)
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


    public static async Task _DeleteDormsThreadIfAlreadyPresent(SocketThreadChannel chan)
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
