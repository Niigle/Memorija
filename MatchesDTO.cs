using System;
using System.Collections.Generic;
using System.Text;

namespace MemoryGame
{
    internal class MatchesDTO
    {
        public int Id { get; set; }
        public int LevelId { get; set; }
        public int HostUserId { get; set; }
        public int GuestUserId { get; set; }
        public int WinnerUserId { get; set; }
        public DateTime MatchDatetime { get; set; }

        public MatchesDTO(int levelId, int userIdHost, int userIdGuest, int userIdWinner, DateTime matchDatetime)
        {
            this.LevelId = levelId;
            this.HostUserId = userIdHost;
            this.GuestUserId = userIdGuest;
            this.WinnerUserId = userIdWinner;
            this.MatchDatetime = matchDatetime;
        }
    }
}
