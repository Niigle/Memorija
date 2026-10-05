using System;
using System.Collections.Generic;
using System.Text;

namespace Memorija
{
    public class OkretanjeKarticeResponse : SocketRequest
    {
        public int Index1 { get; set; }
        public int Index2 { get; set; }
        public string Card1 { get; set; }
        public string Card2 { get; set; }
        public bool Match { get; set; }


        public OkretanjeKarticeResponse() { }
        public OkretanjeKarticeResponse(string tipPoruke, int index1, int index2, string card1, string card2, bool match) : base(tipPoruke)
        {
            this.Index1 = index1;
            this.Index2 = index2;
            this.Card1 = card1;
            this.Card2 = card2;
            this.Match = match;
            Pack();
        }
    }
}
