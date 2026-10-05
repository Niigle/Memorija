using System;
using System.Collections.Generic;
using System.Text;

namespace Memorija
{
    internal class UserEntity
    {
        private int Id { get; set; }
        private Guid UUID { get; set; }
        private string Username { get; set; }
        private DateTime RegistrationDate { get; set; }

        public UserEntity(int id, Guid uuid, string username, DateTime registrationDate)
        {
            this.Id = id;
            this.UUID = uuid;
            this.Username = username;
            this.RegistrationDate = registrationDate;
        }
    }
}
