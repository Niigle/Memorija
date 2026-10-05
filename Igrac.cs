using Fleck;
using System;
using System.Collections.Generic;
using System.Text;

namespace Memorija
{
    public class Igrac
    {
        public IWebSocketConnection Connection { get; set; }
        public string Username { get; set; }
        public int UserId { get; set; }
        public int Poeni { get; set; }
        public string Nivo { get; set; }
        public bool IsHost { get; set; }
        public bool IsNaPotezu { get; set; }
        public int kartica1 { get; set; }
        public int kartica2 { get; set; }
        public bool FlLocal { get; set; }

        public Igrac(IWebSocketConnection konekcija, string ime)
        {
            this.Connection = konekcija;
            this.Username = ime;
            //this.IsHost = isHost;
            //this.Nivo = nivo;
            this.Poeni = 0;
            this.kartica1 = -1;
            this.kartica2 = -1;
        }
    }
}
