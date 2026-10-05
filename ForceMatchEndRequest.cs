using Memorija;
using System;
using System.Collections.Generic;
using System.Text;

namespace MemoryGame
{
    public class ForceMatchEndRequest : SocketRequest
    {
        public string Message { get; set; }

        public ForceMatchEndRequest() { }
        public ForceMatchEndRequest(string tipPoruke, string message) : base(tipPoruke)
        {
            this.Message = message;
            Pack();
        }
    }
}
