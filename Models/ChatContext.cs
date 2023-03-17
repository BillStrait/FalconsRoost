using System;
using System.Collections.Generic;
using OpenAI.GPT3.ObjectModels.RequestModels;

namespace FalconsRoost.Models
{
    public class ChatContext
    {
        public string UserId { get; set; } = string.Empty;
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
        public DateTime TimeStamp { get; set; } = DateTime.Now;


        public ChatContext()
        {
            Messages = new List<ChatMessage> { ChatMessage.FromSystem("You are a snarky assistant") };
        }
    }
}
