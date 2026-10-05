using System;
using System.Collections.Generic;
using System.Text;

namespace Memorija
{
    public class UpdateGameStatusRequest : SocketRequest
    {
        public int Points { get; set; }
        public int Index1 { get; set; }
        public int Index2 { get; set; }
        public string Card1 { get; set; }
        public string Card2 { get; set; }
        public bool IsPogodak { get; set; }

        public UpdateGameStatusRequest() { }
        public UpdateGameStatusRequest(string tipPoruke, int points, int index1, int index2, string card1, string card2, bool isPogodak) : base(tipPoruke)
        {
            this.Points = points;
            this.Index1 = index1;
            this.Index2 = index2;
            this.Card1 = card1;
            this.Card2 = card2;
            this.IsPogodak = isPogodak;
            Pack();
        }

    }
}
