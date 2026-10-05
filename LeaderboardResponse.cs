using Memorija;
using System;
using System.Collections.Generic;
using System.Text;

namespace MemoryGame
{
    public class LeaderboardResponse : SocketRequest
    {
        public Dictionary<string, List<UserDTO>> MostPoints { get; set; }
        public Dictionary<string, List<UserDTO>> MostWins { get; set; }

        public LeaderboardResponse(string tipPoruke, Dictionary<string, List<UserDTO>> mostPoints, Dictionary<string, List<UserDTO>> mostWins) : base(tipPoruke)
        {
            this.MostPoints = mostPoints;
            this.MostWins = mostWins;
            Pack();
        }
    }
}
