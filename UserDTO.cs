using System;
using System.Collections.Generic;
using System.Text;

namespace Memorija
{
    public class UserDTO
    {
        public const int LOCAL_USER_ID = 1;
        public int Id { get; set; }
        //public Guid UUID { get; set; }
        public string Username { get; set; }
        internal string Password { get; set; }
        internal string Email { get; set; }
        public DateTime RegistrationDate { get; set; }
        public int TotalWins { get; set; }
        public int TotalPoints { get; set; }

        public UserDTO() { }

        public UserDTO(int id, /*Guid uuid,*/ string username, DateTime registrationDate)
        {
            this.Id = id;
            //this.UUID = uuid;
            this.Username = username;
            this.RegistrationDate = registrationDate;
        }
    }
}
