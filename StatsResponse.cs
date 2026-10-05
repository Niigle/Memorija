using Memorija;
using System;
using System.Collections.Generic;
using System.Text;

namespace MemoryGame
{
    public class StatsResponse : SocketRequest
    {
        public string Username { get; set; }
        public int Points { get; set; }
        public int TotalMatches { get; set; }
        public int Wins { get; set; }

        public StatsResponse() { }
        public StatsResponse(string tipPoruke, int points, int totalMatches, int wins) : base(tipPoruke)
        {
            this.Points = points;
            this.TotalMatches = totalMatches;
            this.Wins = wins;
            Pack();
        }
    }
}