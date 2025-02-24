using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FalconsRoost.Models.Alerts
{
    public class AlertMessage
    {
        public Guid Id { get; set; }
        public ulong ChannelId { get; set; }
        public string AlertTarget { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public string GenerateAlertMessage(string? message)
        {
            var content = message ?? Message;

            if (!string.IsNullOrWhiteSpace(AlertTarget))
            {
                content = $"@{AlertTarget} - {content}";
            }

            return content;
        }
    }
}
