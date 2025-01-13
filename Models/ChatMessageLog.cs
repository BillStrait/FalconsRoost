using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FalconsRoost.Models
{
    public class ChatMessageLog
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string? Channel { get; set; } = string.Empty;
        public string? ChannelId { get; set; } = string.Empty;
        public string? Guild { get; set; } = string.Empty;
        public string? GuildId { get; set; } = string.Empty;
        public string? Message { get; set; } = string.Empty;
        public DateTime TimeStamp { get; set; } = DateTime.Now;

        public ChatMessageLog()
        {
            Id = Guid.NewGuid();
        }
        public ChatMessageLog(string userId, string source, string? channel, string? channelId, string? guild, string? guildId, string? message)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Source = source;
            ChannelId = channelId;
            Channel = channel;
            Guild = guild;
            GuildId = guildId;
            Message = message;
        }

        public ChatMessageLog(CommandContext ctx)
        {
            Id = Guid.NewGuid();
            UserId = ctx.User.Id.ToString();
            Source = ctx.Client.CurrentUser.Username;
            Channel = ctx.Channel.Name;
            ChannelId = ctx.Channel.Id.ToString();
            Guild = ctx.Guild.Name;
            GuildId = ctx.Guild.Id.ToString();
            Message = ctx.Message.Content;
        }

        public ChatMessageLog(CommandContext ctx, string source, string message)
        {
            Id = Guid.NewGuid();
            UserId = ctx.User.Id.ToString();
            Source = source;
            Channel = ctx.Channel.Name;
            ChannelId = ctx.Channel.Id.ToString();
            Guild = ctx.Guild.Name;
            GuildId = ctx.Guild.Id.ToString();
            Message = message;
        }
    }
}
