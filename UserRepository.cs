using Memorija;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Text.RegularExpressions;
using System.Transactions;
using static Memorija.Program;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MemoryGame
{
    internal class UserRepository
    {
        private readonly DatabaseManager databaseManager = new();

        public async Task<UserDTO> GetUserByUsername(string username)
        {
            /*using (MySqlConnection conn = databaseManager.GetConnection())
            {*
                conn.Open();*/

            await using MySqlConnection conn = databaseManager.GetConnection();
            //await using var conn = new MySqlConnection(connectionString);
            await conn.OpenAsync();

            string upit = "SELECT id, username, password FROM users u where u.username = @username";

            await using var cmd = new MySqlCommand(upit, conn);
            cmd.Parameters.AddWithValue("@username", username);

            await using var reader = await cmd.ExecuteReaderAsync();

            UserDTO? user = null;

            if (await reader.ReadAsync())
            {
                user = new UserDTO
                {
                    Id = reader.GetInt32("id"),
                    Username = reader.GetString("username"),
                    Password = reader.GetString("password")
                };
            }

            return user;
            /*
            UserDTO user = new ();

            while (await reader.ReadAsync())
            {
                user.Id = reader.GetInt32("id");
                user.Username = reader.GetString("username");
                user.Password = reader.GetString("password");
                //user.RegistrationDate = reader.GetDateTime("registration_date");
                //user.Add(reader.GetString("username"));
            }

            //if (user != null)
                return user;*/
            /*else
            {
                int newId = await DodajIgracaAsync(username);

                user.Id = newId;
                user.Username = username;

                return user;
            }*/
        }

        public async Task<int> DodajIgracaAsync(string username, string password)
        {
            await using MySqlConnection conn = databaseManager.GetConnection();
            await conn.OpenAsync();

            string upit = "INSERT INTO users (username, password) VALUES (@username, @password); " +
                          "SELECT LAST_INSERT_ID();";

            await using var cmd = new MySqlCommand(upit, conn);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password", password);

            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task<List<UserDTO>> GetMostPointsLeaderboard(string level)
        {
            List<UserDTO> MostPoints = new List<UserDTO>();

            await using MySqlConnection conn = databaseManager.GetConnection();
            await conn.OpenAsync();

            string upit = "SELECT u.username, SUM(r.points) AS total_points " +
                "FROM match_results r " +
                "JOIN matches m ON m.id = r.match_id " +
                "JOIN users u ON u.id = r.user_id " +
                "JOIN levels l ON l.id = m.level_id " +
                "WHERE l.`level` = @level " +
                "GROUP BY r.user_id, u.username " +
                "ORDER BY total_points DESC LIMIT 10;";

            await using var cmd = new MySqlCommand(upit, conn);
            cmd.Parameters.AddWithValue("@level", level);

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                MostPoints.Add(new UserDTO
                {
                    Username = reader.GetString("username"),
                    TotalWins = reader.GetInt32("total_points")
                });
            }

            return MostPoints;
        }
        public async Task<List<UserDTO>> GetMostWinsLeaderboard(string level)
        {
            List<UserDTO> MostWins = new List<UserDTO>();

            await using MySqlConnection conn = databaseManager.GetConnection();
            await conn.OpenAsync();

            string upit = "SELECT u.username, COUNT(*) AS wins_num " +
                "FROM match_results r " +
                "JOIN matches m ON m.id = r.match_id " +
                "JOIN users u ON u.id = r.user_id " +
                "JOIN levels l ON l.id = m.level_id " +
                "WHERE r.fl_victory = 1 AND l.`level` = @level " +
                "GROUP BY r.user_id, u.username " +
                "ORDER BY wins_num DESC LIMIT 10;";

            await using var cmd = new MySqlCommand(upit, conn);
            cmd.Parameters.AddWithValue("@level", level);

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                MostWins.Add(new UserDTO
                {
                    Username = reader.GetString("username"),
                    TotalWins = reader.GetInt32("wins_num")
                });
            }

            return MostWins;
        }

        public async Task<StatsResponse> GetUserStats(string username)
        {
            await using MySqlConnection conn = databaseManager.GetConnection();
            await conn.OpenAsync();

            int points = 0, wins = 0, matches = 0;
            
            string query = "SELECT u.username, SUM(m.points) as points_num, SUM(m.fl_victory) as win_num FROM users u, match_results m where u.username = @username and u.id = m.user_id;";

            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@username", username);

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                //response.TotalMatches
                points = reader.GetInt32("points_num");
                wins = reader.GetInt32("win_num");
            }


            StatsResponse response = new(MessageType.STATS, points, matches, wins);

            return response;
        }

        public async Task<int> GetLevelId(string level)
        {
            int levelId = 0;

            await using MySqlConnection conn = databaseManager.GetConnection();
            await conn.OpenAsync();

            string query = "SELECT l.id FROM levels l where l.level = @level;";

            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@level", level);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return reader.GetInt32("id");
            }

            return levelId;
        }

        public async Task InsertStats(MatchesDTO matchesDTO, MatchResultsDTO matchResultsHostDTO, MatchResultsDTO matchResultsGuestDTO)
        {
            await using MySqlConnection conn = databaseManager.GetConnection();
            await conn.OpenAsync();

            await using MySqlTransaction transaction = await conn.BeginTransactionAsync();

            try
            {
                string match = "INSERT INTO matches (level_id, host_user_id, guest_user_id, winner_user_id, match_datetime) " +
                                "VALUES (@level_id, @host_user_id, @guest_user_id, @winner_user_id, @match_datetime);" +
                                "SELECT LAST_INSERT_ID();";

                await using var cmd = new MySqlCommand(match, conn, transaction);
                cmd.Parameters.AddWithValue("@level_id", matchesDTO.LevelId);
                cmd.Parameters.AddWithValue("@host_user_id", matchesDTO.HostUserId);
                cmd.Parameters.AddWithValue("@guest_user_id", matchesDTO.GuestUserId);
                cmd.Parameters.AddWithValue("@winner_user_id", matchesDTO.WinnerUserId);
                cmd.Parameters.AddWithValue("@match_datetime", matchesDTO.MatchDatetime);

                int matchId = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                matchResultsHostDTO.MatchId = matchId;
                matchResultsGuestDTO.MatchId = matchId;

                await InsertMatchResult(conn, transaction, matchResultsHostDTO);
                Console.WriteLine("Host rezultat upisan");

                await InsertMatchResult(conn, transaction, matchResultsGuestDTO);
                Console.WriteLine("Guest rezultat upisan");

                await transaction.CommitAsync();
                Console.WriteLine("COMMIT izvršen");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GREŠKA: {ex.Message}, radim ROLLBACK");
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task InsertMatchResult(MySqlConnection conn, MySqlTransaction transaction, MatchResultsDTO matchResultsDTO)
        {
            string matchResults = "INSERT INTO match_results (match_id, user_id, points, correct_pairs, rounds, fl_victory) VALUES (@match_id, @user_id, @points, @correct_pairs, @rounds, @fl_victory)";
            
            await using var cmd = new MySqlCommand(matchResults, conn, transaction);
            cmd.Parameters.AddWithValue("@match_id", matchResultsDTO.MatchId);
            cmd.Parameters.AddWithValue("@user_id", matchResultsDTO.UserId);
            cmd.Parameters.AddWithValue("@points", matchResultsDTO.Points);
            cmd.Parameters.AddWithValue("@correct_pairs", matchResultsDTO.CorrectPairs);
            cmd.Parameters.AddWithValue("@rounds", matchResultsDTO.Rounds);
            cmd.Parameters.AddWithValue("@fl_victory", matchResultsDTO.FlVictory);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
