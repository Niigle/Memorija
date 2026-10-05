using Memorija;
using System;
using System.Collections.Generic;
using System.Text;

namespace MemoryGame
{
    //TODO remove
    public class LeaderboardRequest : SocketRequest
    {
        public string Username { get; set; }

        public LeaderboardRequest(string tipPoruke) : base(tipPoruke)
        {
            Pack();
        }

    }
}
