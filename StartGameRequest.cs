using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Memorija
{
    public class StartGameRequest : SocketRequest
    {
        /*public string[,] tabla; { get; set; }
        public string[][] tabla { get; set; }*/

        //public string[] Tabla { get; set; }//ne treba slati tablu
        public string HostUsername { get; set; }
        public string GuestUsername { get; set; }
        public int Points { get; set; }
        public int Vreme { get; set; }
        public bool IsHost { get; set; }
        public bool IsNaPotezu { get; set; }
        public string Level { get; set; }

        public StartGameRequest() { }
        public StartGameRequest(/*string[/*,] tabla,*/ string tipPoruke, string host, string guest, bool isHost, bool isNaPotezu, string level) : base (tipPoruke)
        {
            //this.Tabla = tabla;
            this.HostUsername = host;
            this.GuestUsername = guest;
            this.Points = 0;
            this.Vreme = 30;
            this.IsHost = isHost;
            this.IsNaPotezu = isNaPotezu;
            this.Level = level;
            Pack();
        }
    }
}
