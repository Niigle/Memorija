using Fleck;
using MemoryGame;
using System.Text.Json;
using System.Threading.Tasks; //TODO kao treba a unused
using System.Timers;
using static Memorija.Program;

namespace Memorija
{
    internal class IgraSoba
    {

        public Igrac hostIgracDTO, guestIgracDTO;

        private readonly object _lock = new object();
        internal bool hostNaPotezu, flActive;
        internal System.Timers.Timer timer;
        private int preostaloVreme, round = 1, pairsHost, pairsGuest, pointMultiplier = 1;
        private const string neotkrivenaKartica = "NEOTKRIVENA";
        internal string?[/*,*/] tabla; // = new string[][];
        internal string?[] otkriveneKartice;

        public IgraSoba(Igrac hostIgrac, Igrac guestIgrac)
        {
            this.hostIgracDTO = hostIgrac;
            this.guestIgracDTO = guestIgrac;
        }

        internal void PrepareGame()
        {
            SelectLevelRequest request = new(MessageType.SELECT_LEVEL, "level");

            PosaljiPorukuKlijentu(hostIgracDTO, request);
        }

        internal void PokreniIgru(string nivo)
        {

            hostIgracDTO.Nivo = nivo;
            guestIgracDTO.Nivo = nivo;
            KreirajTablu(nivo);

            this.hostIgracDTO.IsNaPotezu = true;
            hostNaPotezu = this.hostIgracDTO.IsNaPotezu;
            this.guestIgracDTO.IsNaPotezu = !hostNaPotezu;

            PodesiTimer();

            flActive = true;

            StartGameRequest startGameRequestHost = new StartGameRequest(/*tabla,*/ "KREIRANA_TABLA", this.hostIgracDTO.Username, this.guestIgracDTO.Username, this.hostIgracDTO.IsNaPotezu, this.hostIgracDTO.IsHost, nivo);
            StartGameRequest startGameRequestGuest = new StartGameRequest(/*tabla,*/ "KREIRANA_TABLA", this.hostIgracDTO.Username, this.guestIgracDTO.Username, this.guestIgracDTO.IsNaPotezu, this.guestIgracDTO.IsHost, nivo);

            try {
                PosaljiPorukuKlijentu(hostIgracDTO, startGameRequestHost);
            } catch (Exception e) { }

            try
            {
                PosaljiPorukuKlijentu(guestIgracDTO, startGameRequestGuest);
            }
            catch (Exception e) { }            

        }

        private static void PosaljiPorukuKlijentu<T>(Igrac igrac, T request) where T : SocketRequest //, new()
        {
            //string jsonRequest = JsonSerializer.Serialize(request);

            if (igrac.Connection.IsAvailable)
            {
                igrac.Connection.Send(request.Json);
            }
        }

        private void PromeniIgracaNaPotezu(Igrac igracBioNaPotezu, Igrac igracDaBudeNaPotezu)
        {
            igracBioNaPotezu.IsNaPotezu = false;
            igracDaBudeNaPotezu.IsNaPotezu = true;

            round++;
                                    
            try
            {
                ChangeTurnRequest changeTurnRequest = new ChangeTurnRequest("CHANGE_TURN", igracBioNaPotezu.IsNaPotezu);
                PosaljiPorukuKlijentu(igracBioNaPotezu, changeTurnRequest);
            }
            catch (Exception e) { }

            try
            {
                ChangeTurnRequest changeTurnRequest = new ChangeTurnRequest("CHANGE_TURN", igracDaBudeNaPotezu.IsNaPotezu);
                PosaljiPorukuKlijentu(igracDaBudeNaPotezu, changeTurnRequest);
            }
            catch (Exception e) { }
        }

        private void PodesiTimer()
        {
            preostaloVreme = 30;

            timer?.Stop();
            timer?.Dispose();

            timer = new System.Timers.Timer(1000);

            timer.Elapsed += SvakeSekunde;
            timer.Start();
        }

        private void SvakeSekunde(object sender, ElapsedEventArgs e)
        {
            lock (_lock)
            {
                preostaloVreme--;                                
            }

            if (preostaloVreme <= 0)
            {
                timer.Stop();
                //timer.Dispose(); //claude stavio, gemine da ne treba

                Console.WriteLine("VREME JE ISTEKLO! Automatska promena igrača.");//okreni random kartice

                if (hostIgracDTO.IsNaPotezu)
                {
                    PromeniIgracaNaPotezu(guestIgracDTO, hostIgracDTO);
                    PodesiTimer();
                }
                else
                {
                    PromeniIgracaNaPotezu(hostIgracDTO, guestIgracDTO);
                    PodesiTimer();
                }
            }
        }

        public async void PauzirajTimerNaDveSekunde()
        {
            
            timer?.Stop();
            timer?.Dispose();

            
            await Task.Delay(2000);

            
            PodesiTimer();
        }

        private void KreirajTablu(string nivo)
        {

            int dimenzija, brojJedinstvenihKartica;
            switch (nivo)//8,18,32,50
            {
                case "easy":
                    dimenzija = 4;
                    brojJedinstvenihKartica = 8;
                    break;
                case "medium":
                    dimenzija = 6;
                    brojJedinstvenihKartica = 18;
                    break;
                case "hard":
                    dimenzija = 8;
                    brojJedinstvenihKartica = 32;
                    break;
                case "expert":
                    dimenzija = 10;
                    brojJedinstvenihKartica = 50;
                    break;
                default: throw new ArgumentOutOfRangeException("Nije izabran ispravan broj kombinacija");
            }

            tabla = new string[dimenzija * dimenzija];
            otkriveneKartice = new string[tabla.Length];

            int ukupnoMesta = tabla.Length;
            int index = 0;

            string[] kartice = new string[ukupnoMesta];
            for (int i = 0; i < brojJedinstvenihKartica; i++)
            {                
                kartice[index++] = KarticeDTO.listaKartica[i];
                kartice[index++] = KarticeDTO.listaKartica[i];
            }

            //TODO >= da li je dovoljno sve popuniti NEOT...
            Random random = new Random();
            for (int i = ukupnoMesta - 1; i >= 0; i--)
            {
                int j = random.Next(i + 1);
                string tmp = kartice[i];
                kartice[i] = kartice[j];
                kartice[j] = tmp;

                otkriveneKartice[i] = neotkrivenaKartica;
            }

            string[,] randomizovanaTabla = new string[dimenzija, dimenzija];
            for (int k = 0; k < ukupnoMesta; k++)
            {
                int red = k / dimenzija;
                int kolona = k % dimenzija;
                randomizovanaTabla[red, kolona] = kartice[k];
            }

            tabla = PoravnjajMatricu(randomizovanaTabla);
        }

        private string[] PoravnjajMatricu(string[,] tabla)
        {
            string[] poravnanaTabla = new string[tabla.Length];
            int brojac = 0;

            foreach (string str in tabla)
            {
                poravnanaTabla[brojac] = str;
                brojac++;
            }
           
            return poravnanaTabla;
        }

        internal async Task ObradiKlik(IWebSocketConnection socket, int index)
        {
            //bool isPogodak = otvoreneKartice[0].naziv?.Equals(otvoreneKartice[1].naziv) ?? false;

            bool naPotezu = (socket == hostIgracDTO.Connection) ? hostIgracDTO.IsNaPotezu : guestIgracDTO.IsNaPotezu;
            if (!naPotezu)
                return;

            Igrac igracNaPotezu = (socket == hostIgracDTO.Connection) ? hostIgracDTO : guestIgracDTO;
            Igrac igracNaCekanju = (socket == hostIgracDTO.Connection) ? guestIgracDTO : hostIgracDTO;

            if (igracNaPotezu.kartica1 == -1)
            {
                igracNaPotezu.kartica1 = index;

                OkretanjeKarticeResponse response = new OkretanjeKarticeResponse("OKRETANJE_KARTICE", igracNaPotezu.kartica1, igracNaPotezu.kartica2, tabla[index], "", false);

                PosaljiPorukuKlijentu(hostIgracDTO, response);
                PosaljiPorukuKlijentu(guestIgracDTO, response);

            } else
            {
                igracNaPotezu.kartica2 = index;

                bool isPogodak = tabla[igracNaPotezu.kartica1] == tabla[igracNaPotezu.kartica2];

                if (isPogodak)
                {
                    /*otkriveneKartice[kartica1] = poravnanaTabla[kartica1];
                    otkriveneKartice[kartica2] = poravnanaTabla[kartica2];*/
                    otkriveneKartice[igracNaPotezu.kartica1] = tabla[igracNaPotezu.kartica1];
                    otkriveneKartice[igracNaPotezu.kartica2] = tabla[igracNaPotezu.kartica2];

                    igracNaPotezu.Poeni++;
                    //pointMultiplier;

                    UpdateGameStatusRequest updateGameStatusRequest = new UpdateGameStatusRequest("UPDATE_GAME_STATUS", 
                                                                                                  igracNaPotezu.Poeni, 
                                                                                                  igracNaPotezu.kartica1, 
                                                                                                  igracNaPotezu.kartica2, 
                                                                                                  tabla[igracNaPotezu.kartica1], 
                                                                                                  tabla[igracNaPotezu.kartica2],
                                                                                                  isPogodak);
                    PosaljiPorukuKlijentu(hostIgracDTO, updateGameStatusRequest);
                    PosaljiPorukuKlijentu(guestIgracDTO, updateGameStatusRequest);

                    igracNaPotezu.kartica1 = -1;
                    igracNaPotezu.kartica2 = -1;

                    if (!otkriveneKartice.Contains<string>(neotkrivenaKartica))
                        await ZavrsiMec();
                    else
                        PodesiTimer();
                }
                else
                {
                    OkretanjeKarticeResponse response = new OkretanjeKarticeResponse("OKRETANJE_KARTICE", igracNaPotezu.kartica1, igracNaPotezu.kartica2, tabla[igracNaPotezu.kartica1], tabla[igracNaPotezu.kartica2], isPogodak);

                    PosaljiPorukuKlijentu(hostIgracDTO, response);
                    PosaljiPorukuKlijentu(guestIgracDTO, response);

                    igracNaPotezu.IsNaPotezu = false;
                    PauzirajTimerNaDveSekunde();

                    await Task.Delay(2000);

                    UpdateGameStatusRequest updateGameStatusRequest = new ("UPDATE_GAME_STATUS",
                                                                            igracNaPotezu.Poeni,
                                                                            igracNaPotezu.kartica1,
                                                                            igracNaPotezu.kartica2,
                                                                            "",
                                                                            "",
                                                                            isPogodak);

                    PosaljiPorukuKlijentu(hostIgracDTO, updateGameStatusRequest);
                    PosaljiPorukuKlijentu(guestIgracDTO, updateGameStatusRequest);

                    igracNaPotezu.kartica1 = -1;
                    igracNaPotezu.kartica2 = -1;

                    PromeniIgracaNaPotezu(igracNaPotezu, igracNaCekanju);
                }                
            }            
        }

        private async Task ZavrsiMec()
        {
            this.guestIgracDTO.IsNaPotezu = false;
            this.hostIgracDTO.IsNaPotezu = false;

            timer?.Stop();

            Console.WriteLine("Igra je zavrsena.");

            bool isHostWinner;
            int winnerPoints;

            if (hostIgracDTO.Poeni > guestIgracDTO.Poeni)
            {
                isHostWinner = true;
                winnerPoints = hostIgracDTO.Poeni;
            }
            else
            {
                isHostWinner = false;

                if (hostIgracDTO.Poeni < guestIgracDTO.Poeni)                
                    winnerPoints = hostIgracDTO.Poeni;
                else
                    winnerPoints = -1;
            }

            EndMatchRequest endGameRequest = new(MessageType.KRAJ_IGRE, winnerPoints, isHostWinner);

            PosaljiPorukuKlijentu(hostIgracDTO, endGameRequest);
            PosaljiPorukuKlijentu(guestIgracDTO, endGameRequest);

            await UpdateDatabase();
        }

        private async Task UpdateDatabase()
        {
            UserRepository userRepository = new();
            int winnerId, hostVictory = 0, guestVictory = 0;

            if (hostIgracDTO.Poeni > guestIgracDTO.Poeni)
            {
                hostVictory = 1;
                winnerId = hostIgracDTO.UserId;
            }
            else
            {
                if (hostIgracDTO.Poeni < guestIgracDTO.Poeni)
                {
                    guestVictory = 1;
                    winnerId = guestIgracDTO.UserId;
                }
                else
                    winnerId = -1;
            }

            int levelId = await userRepository.GetLevelId(this.hostIgracDTO.Nivo);

            MatchesDTO matchesDTO = new MatchesDTO(levelId, hostIgracDTO.UserId, guestIgracDTO.UserId, winnerId, DateTime.Now);

            MatchResultsDTO matchResultsHostDTO = new MatchResultsDTO(hostIgracDTO.UserId, hostIgracDTO.Poeni, hostIgracDTO.Poeni, this.round, hostVictory);
            MatchResultsDTO matchResultsGuestDTO = new MatchResultsDTO(guestIgracDTO.UserId, guestIgracDTO.Poeni, guestIgracDTO.Poeni, this.round, guestVictory);

            await userRepository.InsertStats(matchesDTO, matchResultsHostDTO, matchResultsGuestDTO);
        }

        internal async Task ShowStats(string username)
        {
            UserRepository userRepository = new();
            StatsResponse statsResponse = await userRepository.GetUserStats(username);

            if (hostIgracDTO.Username == username)
                PosaljiPorukuKlijentu(hostIgracDTO, statsResponse);
            else
                PosaljiPorukuKlijentu(guestIgracDTO, statsResponse);
        }

        internal async Task GetLeaderboard(IWebSocketConnection Connection)
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

            LeaderboardResponse leaderboardResponse = new(MessageType.LEADERBOARD, mostWins, mostPoints);

            if (hostIgracDTO.Connection == Connection)
                PosaljiPorukuKlijentu(hostIgracDTO, leaderboardResponse);
            else
                PosaljiPorukuKlijentu(guestIgracDTO, leaderboardResponse);
        }

        private void PauzirajPotez(Igrac igracNaPotezu)
        {
            if (igracNaPotezu.IsNaPotezu)
                igracNaPotezu.IsNaPotezu = false;
            else
                igracNaPotezu.IsNaPotezu = true;
        }

        internal void SendMessage(/*Igrac sender, Igrac receiver, */IWebSocketConnection senderConnection, MessageRequest messageRequest)
        {
            if (senderConnection == hostIgracDTO.Connection)
            {
                PosaljiPorukuKlijentu(guestIgracDTO, messageRequest);
            } else
            {
                PosaljiPorukuKlijentu(hostIgracDTO, messageRequest);
            }
        }

        internal void ForceMatchEnd(IWebSocketConnection connection)
        {
            flActive = false;

            ForceMatchEndRequest forceMatchEndRequest = new ForceMatchEndRequest(MessageType.FORCE_MATCH_END, "");

            NetworkManager.PosaljiPorukuKlijentu(connection, forceMatchEndRequest);
        }

        //TODO ne koristi se?
        public IWebSocketConnection hostIgrac, guestIgrac;

        public IgraSoba(IWebSocketConnection hostIgrac, IWebSocketConnection guestIgrac)
        {
            this.hostIgrac = hostIgrac;
            this.guestIgrac = guestIgrac;
        }
    }
}
