using Memorija;
using System;
using System.Collections.Generic;
using System.Text;

namespace Memorija
{
    public class EndMatchRequest : SocketRequest
    {
        public int Points { get; set; }
        public int Index1 { get; set; }
        public int Index2 { get; set; }
        public string Card1 { get; set; }
        public string Card2 { get; set; }
        public bool IsHostWinner { get; set; }
        public List<UserDTO> Players { get; set; }

        public EndMatchRequest() { }
        public EndMatchRequest(string tipPoruke, int points, bool isWinner) : base(tipPoruke)
        {
            this.Points = points;
            this.IsHostWinner = isWinner;
            Pack();
        }
    }
}
