using System;
using System.Collections.Generic;
using System.Text;

namespace Memorija
{
    public class OkretanjeKarticeRequest : SocketRequest
    {
        public int Index { get; set; }

        public OkretanjeKarticeRequest() { }
        public OkretanjeKarticeRequest(string tipPoruke, int index) : base(tipPoruke)
        {
            this.Index = index;
            Pack();
        }

    }
}
