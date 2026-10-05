using Fleck;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using MemoryGame;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text.Json;
using System.Collections.Concurrent;

namespace Memorija
{
    public class Program
    {
        public static class MessageType
        {
            public const string LOGIN = "LOGIN";
            public const string REGISTER = "REGISTER";
            public const string JOIN_LOCAL = "JOIN_LOCAL";
            public const string JOIN = "JOIN";
            public const string SELECT_LEVEL = "SELECT_LEVEL";
            public const string CHAT = "CHAT";
            public const string KLIK_KARTICE = "KLIK_KARTICE";
            public const string KREIRANA_TABLA = "KREIRANA_TABLA";
            public const string CHANGE_TURN = "CHANGE_TURN";
            public const string OKRETANJE_KARTICE = "OKRETANJE_KARTICE";
            public const string UPDATE_GAME_STATUS = "UPDATE_GAME_STATUS";
            public const string KRAJ_IGRE = "GAME_END";
            public const string LEADERBOARD = "LEADERBOARD";
            public const string STATS = "STATS";
            public const string FORCE_MATCH_END = "FORCE_MATCH_END";
        }

        static Matchmaker matchmaker = new Matchmaker();


        static List<IWebSocketConnection> sviIgraci = new List<IWebSocketConnection>();
        internal static List<IgraSoba> aktivneSobe = new List<IgraSoba>();
        //internal static Dictionary<IWebSocketConnection, IgraSoba> mapaSoba = new Dictionary<IWebSocketConnection, IgraSoba>();
        internal static ConcurrentDictionary<IWebSocketConnection, IgraSoba> mapaSoba = new();
        internal static readonly object RoomsLock = new();
        private static void RemoveWaiting(IWebSocketConnection socket)
        {
            lock (Matchmaker.Lock)
            {
                if (Matchmaker.igraciKojiCekaju.Any(player => player.Connection == socket))
                {
                    Matchmaker.igraciKojiCekaju.TryDequeue(out var item);
                    Console.WriteLine("Igrač je uklonjen iz reda cekanja.");
                }
            }
        }

        static async Task Main(string[] args)
        {
            //WebSocketServer server = new WebSocketServer("ws://0.0.0.0:8081");

            var port = Environment.GetEnvironmentVariable("PORT") ?? "8081";
            var server = new WebSocketServer($"ws://0.0.0.0:{port}");

            /* var server = new WebSocketServer("wss://0.0.0.0:8081");
             server.Certificate = new X509Certificate2("cert.pfx", "lozinka");
             server.EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;*/

            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    Console.WriteLine("Igrač se povezao!");
                    sviIgraci.Add(socket);
                };

                socket.OnClose = () =>
                {
                    if (mapaSoba.TryGetValue(socket, out IgraSoba currentRoom))
                    {
                        if (currentRoom.flActive)
                        {
                            IWebSocketConnection remainingPlayer = currentRoom.hostIgracDTO.Connection == socket ? currentRoom.guestIgracDTO.Connection : currentRoom.hostIgracDTO.Connection;

                            currentRoom.ForceMatchEnd(remainingPlayer);

                            /*lock (RoomsLock)
                            {
                                aktivneSobe.Remove(currentRoom);

                                mapaSoba.Remove(currentRoom.guestIgracDTO.Connection);
                                mapaSoba.Remove(currentRoom.hostIgracDTO.Connection);
                            }*/

                            aktivneSobe.Remove(currentRoom);
                            mapaSoba.TryRemove(currentRoom.guestIgracDTO.Connection, out _);
                            mapaSoba.TryRemove(currentRoom.hostIgracDTO.Connection, out _);
                        }
                    }

                    Console.WriteLine("Igrač se diskonektovao.");
                    sviIgraci.Remove(socket);

                    RemoveWaiting(socket);
                    /*
                    if (Matchmaker.igraciKojiCekaju.Any(player => player.Connection == socket))
                    {
                        Matchmaker.igraciKojiCekaju.TryDequeue(out var item);
                        Console.WriteLine("Igrač je uklonjen iz reda cekanja.");
                    }*/
                };

                try
                {
                    socket.OnMessage = async message =>
                    {
                        Console.WriteLine($"Primljena poruka od klijenta: {message}");

                        var poruka = JsonSerializer.Deserialize<SocketRequest>(message);

                        if (poruka.TipPoruke == MessageType.LOGIN)
                        {
                            var playerJoinRequest = JsonSerializer.Deserialize<PlayerJoinRequest>(message);

                            UserRepository userRepository = new();
                            UserDTO userDTO = await userRepository.GetUserByUsername(playerJoinRequest.Username);

                            PlayerJoinRequest response;

                            if (userDTO == null)
                            {
                                string wrongPasswordMessage = "Wrong username or password";
                                response = new PlayerJoinRequest(MessageType.LOGIN, playerJoinRequest.Username, "", playerJoinRequest.Email, wrongPasswordMessage, -1);
                                socket.Send(response.Json);
                                return;

                            }
                            else
                            {
                                if (!BCrypt.Net.BCrypt.Verify(playerJoinRequest.Password, userDTO.Password))
                                {
                                    string wrongPasswordMessage = "Wrong username or password";
                                    response = new PlayerJoinRequest(MessageType.LOGIN, playerJoinRequest.Username, "", playerJoinRequest.Email, wrongPasswordMessage, -1);
                                    socket.Send(response.Json);
                                    return;
                                }

                                Console.WriteLine("Igrač se LOGINovao.");
                                response = new PlayerJoinRequest(MessageType.LOGIN, playerJoinRequest.Username, "", playerJoinRequest.Email, "Login successful", userDTO.Id);
                                socket.Send(response.Json);
                            }
                        }

                        if (poruka.TipPoruke == MessageType.REGISTER)
                        {
                            var playerJoinRequest = JsonSerializer.Deserialize<PlayerJoinRequest>(message);
                            Igrac noviIgrac = new Igrac(socket, playerJoinRequest.Username);

                            UserRepository userRepository = new();

                            UserDTO existingUser = await userRepository.GetUserByUsername(playerJoinRequest.Username);

                            if (existingUser != null)
                            {
                                string unavailableUsernameMessage = "Provided username is unavailable";
                                PlayerJoinRequest response = new PlayerJoinRequest(MessageType.REGISTER, playerJoinRequest.Username, "", playerJoinRequest.Email, unavailableUsernameMessage, -1);
                                
                                socket.Send(response.Json);
                            } else
                            {
                                UserDTO userDTO = new();

                                string hash = BCrypt.Net.BCrypt.HashPassword(playerJoinRequest.Password);

                                int newId = await userRepository.DodajIgracaAsync(noviIgrac.Username, hash);

                                noviIgrac.UserId = newId;
                                noviIgrac.FlLocal = false;

                                PlayerJoinRequest response = new PlayerJoinRequest(MessageType.REGISTER, playerJoinRequest.Username, "", playerJoinRequest.Email, "Login successful", newId);
                                NetworkManager.PosaljiPorukuKlijentu(socket, playerJoinRequest);
                            }                                                        
                        }

                        if (poruka.TipPoruke == MessageType.JOIN_LOCAL)
                        {
                            var playerJoinRequest = JsonSerializer.Deserialize<PlayerJoinRequest>(message);

                            Igrac noviIgrac = new Igrac(socket, playerJoinRequest.Username);

                            noviIgrac.FlLocal = true;
                            noviIgrac.UserId = UserDTO.LOCAL_USER_ID;

                            matchmaker.DodajURed(noviIgrac);
                        }

                        if (poruka.TipPoruke == MessageType.JOIN)
                        {
                            var playerJoinRequest = JsonSerializer.Deserialize<PlayerJoinRequest>(message);

                            Igrac noviIgrac = new Igrac(socket, playerJoinRequest.Username);
                            noviIgrac.FlLocal = false;
                            noviIgrac.UserId = playerJoinRequest.Id;

                            matchmaker.DodajURed(noviIgrac);
                        }

                        if (poruka.TipPoruke == MessageType.SELECT_LEVEL)
                        {
                            var selectLevelRequest = JsonSerializer.Deserialize<SelectLevelRequest>(message);

                            lock (Program.RoomsLock)
                            {
                                foreach (IgraSoba igraSoba in aktivneSobe)
                                {
                                    if (igraSoba.guestIgracDTO.Connection == socket || igraSoba.hostIgracDTO.Connection == socket)
                                        igraSoba.PokreniIgru(selectLevelRequest.Level);
                                }
                            }
                        }

                        if (poruka.TipPoruke == MessageType.KLIK_KARTICE)
                        {
                            var okretanjeKarticeRequest = JsonSerializer.Deserialize<OkretanjeKarticeRequest>(message);

                            lock (Program.RoomsLock)
                            {
                                foreach (IgraSoba igraSoba in aktivneSobe)
                                {
                                    if (igraSoba.guestIgracDTO.Connection == socket || igraSoba.hostIgracDTO.Connection == socket)
                                        /*await*/
                                       igraSoba.ObradiKlik(socket, okretanjeKarticeRequest.Index);
                                }
                            }
                        }

                        if (poruka.TipPoruke == MessageType.STATS)
                        {
                            var statsRequest = JsonSerializer.Deserialize<StatsRequest>(message);

                            await NetworkManager.ShowStats(statsRequest.Username, socket);                         
                        }

                        if (poruka.TipPoruke == MessageType.LEADERBOARD)
                        {
                            var leaderboardRequest = JsonSerializer.Deserialize<SocketRequest>(message);

                            await NetworkManager.GetLeaderboard(socket);
                        }

                        if (poruka.TipPoruke == MessageType.CHAT)
                        {
                            var messageRequest = JsonSerializer.Deserialize<MessageRequest>(message);

                            lock (Program.RoomsLock)
                            {
                                foreach (IgraSoba igraSoba in aktivneSobe)
                                {
                                    if (igraSoba.guestIgracDTO.Connection == socket || igraSoba.hostIgracDTO.Connection == socket)
                                        igraSoba.SendMessage(socket, messageRequest);
                                }
                            }                            
                        }
                    };
                } catch
                {

                }
                
            });

            await Task.Delay(Timeout.Infinite);
            //Console.ReadLine();
        }
    }
}