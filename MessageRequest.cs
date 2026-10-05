using Memorija;
using System;
using System.Collections.Generic;
using System.Text;

namespace MemoryGame
{
    public class MessageRequest : SocketRequest
    {
        public string Message { get; set; }

        public MessageRequest() { }
        public MessageRequest(string tipPoruke, string message) : base(tipPoruke)
        {
            this.Message = message;
            Pack();
        }

    }
}
