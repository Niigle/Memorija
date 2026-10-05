using Fleck;
using Memorija;
using System;
using System.Collections.Generic;
using System.Text;
using static Memorija.Program;

namespace MemoryGame
{
    internal class NetworkManager
    {
        internal static void PosaljiPorukuKlijentu<T>(IWebSocketConnection connection, T request) where T : SocketRequest
        {
            //string jsonRequest = JsonSerializer.Serialize(request);

            if (connection.IsAvailable)
            {
                connection.Send(request.Json);
            }
        }

        internal static async Task ShowStats(string username, IWebSocketConnection connection)
        {
            UserRepository userRepository = new();
            StatsResponse statsResponse = await userRepository.GetUserStats(username);

            PosaljiPorukuKlijentu(connection, statsResponse);
        }

        internal static async Task GetLeaderboard(IWebSocketConnection connection)
        {
            UserRepository userRepository = new();

            Dictionary<string, List<UserDTO>> mostWins = new();
            Dictionary<string, List<UserDTO>> mostPoints = new();

            string[] levels = { "easy", "medium", "hard", "expert" };

            foreach (string level in levels)
            {
                List<UserDTO> userDTOList = await userRepository.GetMostPointsLeaderboard(level);
                mostPoints.Add(level, userDTOList);

                userDTOList = await userRepository.GetMostWinsLeaderboard(level);
                mostWins.Add(level, userDTOList);
            }

            LeaderboardResponse leaderboardResponse = new(MessageType.LEADERBOARD, mostPoints, mostWins);

            PosaljiPorukuKlijentu(connection, leaderboardResponse);
        }
    }
}
