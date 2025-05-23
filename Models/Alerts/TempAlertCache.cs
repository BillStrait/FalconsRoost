﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FalconsRoost.Models.Alerts
{
    public static class TempAlertCache
    {
        private static Dictionary<string, TempAlertRecord> _cache = new();

        public static void Register(string id, string handle, string title, ulong channelId, ulong userId)
        {
            _cache[id] = new TempAlertRecord
            {
                Id = id,
                Handle = handle,
                Title = title,
                ChannelId = channelId,
                UserId = userId,
                Timestamp = DateTime.UtcNow,
            };
        }

        public static void Activate(string id)
        {
            if (_cache.TryGetValue(id, out var record))
            {
                record.IsWatched = true;
                record.Timestamp = DateTime.UtcNow;
            }
        }

        public static List<TempAlertRecord> GetActiveRecords()
        {
            CleanupExpired(new TimeSpan(24, 0, 0));
            return _cache.Values.Where(c=>c.IsWatched).ToList();
        }

        public static TempAlertRecord? Get(string id)
        {
            _cache.TryGetValue(id, out var record);
            return record;
        }

        public static void Remove(string id)
        {
            _cache.Remove(id);
        }

        public static void CleanupExpired(TimeSpan expiration)
        {
            var now = DateTime.UtcNow;
            _cache = _cache
                .Where(kvp => (now - kvp.Value.Timestamp) < expiration)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public static string AttemptRegister(string? targetGuid)
        {
            var message = string.Empty;
            if(string.IsNullOrWhiteSpace(targetGuid))
            {
                message = "Please provide a valid target ID.";
                return message;
            }
            var alert = TempAlertCache.Get(targetGuid);
            if (alert == null)
            {
                message = "That target does not exist or has expired. Please try again.";
            }
            else if (alert.IsWatched)
            {
                message = "That target is already being watched. This channel will be notified if the item becomes available within 24 hours.";
            }
            else
            {
                TempAlertCache.Activate(targetGuid);
                message = $"You are now watching {alert.Title} for availability. This channel will be notified if the item becomes available within 24 hours.";
            }
            
            return message;
        }
    }

    public class TempAlertRecord
    {
        public string Id { get; set; } = string.Empty;
        public string Handle { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public ulong ChannelId { get; set; }
        public ulong UserId { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsWatched { get; set; } = false;
    }

}
