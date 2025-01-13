using DSharpPlus.CommandsNext;
using FalconsRoost.Models;
using FalconsRoost.Models.db;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FalconsRoost.Bots
{
    public class ExtendedCommandModule : BaseCommandModule
    {
        public FalconsRoostDBContext? _dbContext;

        public ExtendedCommandModule(FalconsRoostDBContext? context)
        {
            _dbContext = context;
        }

        protected async Task<Player> GetPlayerAndUpdateMessageHistory(CommandContext ctx)
        {
            var player = await GetPlayerAsync(ctx);
            await LogRequest(ctx);
            player.LastMessage = ctx.Message.Content;
            player.LastSeen = DateTime.Now;
            await SavePlayer(player);
            return player;
        }

        protected async Task<Player> GetPlayerAsync(CommandContext ctx)
        {
            var player = await _dbContext!.Players.FirstOrDefaultAsync(p => p.DiscordId == ctx.User.Id);
            if (player == null)
            {
                player = new Player(ctx.User);
                _dbContext!.Players.Add(player);
                await _dbContext.SaveChangesAsync();
            }
            return player;
        }

        protected async Task SavePlayer(Player player)
        {
            _dbContext!.Players.Update(player);
            await _dbContext.SaveChangesAsync();
        }

        protected async Task LogRequest(CommandContext ctx)
        {
            if (_dbContext != null)
            {
                var request = new ChatMessageLog(ctx);
                _dbContext!.ChatMessageLogs.Add(request);
                await _dbContext.SaveChangesAsync();
            }
        }

        protected async Task LogResponse(CommandContext ctx, string response)
        {
            if (_dbContext != null)
            {
                var request = new ChatMessageLog(ctx, "BaseBot", response);
                _dbContext!.ChatMessageLogs.Add(request);
                await _dbContext.SaveChangesAsync();
            }
        }

    }
}
