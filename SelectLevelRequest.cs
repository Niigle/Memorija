using Memorija;
using System;
using System.Collections.Generic;
using System.Text;

namespace MemoryGame
{
    public class SelectLevelRequest : SocketRequest
    {
        public string Level { get; set; }

        public SelectLevelRequest() { }
        public SelectLevelRequest(string tipPoruke, string level) : base(tipPoruke)
        {
            this.Level = level;
            Pack();
        }
    }
}
