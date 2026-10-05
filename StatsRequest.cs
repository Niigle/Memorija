using Memorija;
using System;
using System.Collections.Generic;
using System.Text;

namespace MemoryGame
{
    public class StatsRequest : SocketRequest
    {
        public string Username { get; set; }

        public StatsRequest() { }
        public StatsRequest(string tipPoruke, string username) : base(tipPoruke)
        {
            this.Username = username;
            Pack();
        }
    }
}
