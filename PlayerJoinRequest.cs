using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace Memorija
{
    internal class PlayerJoinRequest : SocketRequest
    {

        // private static Enum LOGIN, KLIK_KARTICE. CHAT...

        public string Username { get; set; }

        public string Password { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public int Id { get; set; }
        /*public int poeni { get; set; } = 0;
        public bool isHost { get; set; }*/
        //public string tipPoruke { get; set; }
        //public string nivo { get; set; } //8,18,32,50

        /*public string[,] tabla;
        public int kartica1;
        public int kartica2;

        public string porukaZaProtivnika;*/

        public PlayerJoinRequest() { }
        public PlayerJoinRequest(string tipPoruke, string username, string password, string email, string message, int id) : base(tipPoruke)
        {
            this.Username = username;
            this.Password = password;
            this.Email = email;
            this.Message = message;
            this.Id = id;
            Pack();
        }
    }
}
