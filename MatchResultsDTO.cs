using System;
using System.Collections.Generic;
using System.Text;

namespace MemoryGame
{
    internal class MatchResultsDTO
    {
        public int Id { get; set; }
        public int MatchId { get; set; }
        public int UserId { get; set; }
        public int Points { get; set; }
        public int CorrectPairs { get; set; }
        public int Rounds { get; set; }
        public int FlVictory { get; set; }


        public MatchResultsDTO(int userId, int points, int correctPairs, int rounds, int flVictory)
        {
            this.UserId = userId;
            this.Points = points;
            this.CorrectPairs = correctPairs;
            this.Rounds = rounds;
            this.FlVictory = flVictory;
        }
    }
}
