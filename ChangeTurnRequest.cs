using System;
using System.Collections.Generic;
using System.Text;

namespace Memorija
{
    public class ChangeTurnRequest : SocketRequest
    {
        public bool IsNaPotezu { get; set; }
        public int Vreme{ get; set; }


        public ChangeTurnRequest() { }
        public ChangeTurnRequest(string tipPoruke, bool isNaPotezu) : base(tipPoruke)
        {
            this.IsNaPotezu = isNaPotezu;
            this.Vreme = 30;
            Pack();
        }
    }
}
