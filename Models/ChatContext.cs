using Betalgo.Ranul.OpenAI.ObjectModels.RequestModels;
using System;
using System.Collections.Generic;

namespace FalconsRoost.Models
{
    public class ChatContext
    {
        public string UserId { get; set; } = string.Empty;
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
        public DateTime TimeStamp { get; set; } = DateTime.Now;


        public ChatContext()
        {
            Messages = new List<ChatMessage> { ChatMessage.FromSystem("You are a chat bot/assistant that interacts with comic book entheusiests and programmers in a public setting. Assume the persona of a film noir detective talking to/about their client. Do not use the word 'ah' in your first sentence. Attempt to limit your response to 1000 characters or less. Provide grounded answers for any facts, and cite sources when able. Use markdown syntax for links.") };
        }
    }
}
