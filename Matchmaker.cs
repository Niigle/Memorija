using Fleck;
using Memorija;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Memorija
{
    internal class Matchmaker
    {
        /*private List<IWebSocketConnection> igraciKojiCekaju = new List<IWebSocketConnection>();
        
        private List<Igrac> igraciKojiCekaju = new();

        public void DodajUred(IWebSocketConnection noviIgrac)
        {
            igraciKojiCekaju.Add(noviIgrac);

            // Ako imamo dvojicu, spoji ih 
            if (igraciKojiCekaju.Count >= 2)
            {
                var igrac1 = igraciKojiCekaju[0];
                var igrac2 = igraciKojiCekaju[1];
                igraciKojiCekaju.RemoveRange(0, 2);

                // Kreiramo novu "Sobu" za njih dvojicu i pokrećemo meč! 
                IgraSoba novaSoba = new IgraSoba(igrac1, igrac2);
                novaSoba.PokreniIgru();
            }
        }*/
        private Queue<Igrac> hostoviKojiCekaju = new();
        private Queue<Igrac> guestoviKojiCekaju = new();
        public static readonly object Lock = new();
        public static ConcurrentQueue<Igrac> igraciKojiCekaju = new();
        //ConcurrentDictionary<IWebSocketConnection, Igrac> igraciKojiCekajuD = new ();

        public void DodajURed(Igrac noviIgrac)
        {
            /*igraciKojiCekajuD.TryAdd(noviIgrac.Connection, noviIgrac);
            Console.WriteLine("Igrac je dodat.");

            if (igraciKojiCekajuD.Count > 1)
            {
                if (igraciKojiCekajuD.TryRemove(out Igrac hostIgrac) && igraciKojiCekajuD.TryRemove(out Igrac guestIgrac))
                {
                    Console.WriteLine("Igra se otvara.");
                    hostIgrac.IsHost = true;
                    guestIgrac.IsHost = false;

                    IgraSoba novaSoba = new(hostIgrac, guestIgrac);

                    novaSoba.PrepareGame();

                    Program.aktivneSobe.Add(novaSoba);
                    Program.mapaSoba.Add(hostIgrac.Connection, novaSoba);
                    Program.mapaSoba.Add(guestIgrac.Connection, novaSoba);
                }
            }*/
            Igrac host = null, guest = null;

            lock (Matchmaker.Lock)
            {
                igraciKojiCekaju.Enqueue(noviIgrac);
                Console.WriteLine("Igrac je dodat.");

                if (igraciKojiCekaju.Count > 1)
                {
                    igraciKojiCekaju.TryDequeue(out host);
                    igraciKojiCekaju.TryDequeue(out guest);
                }
            }

            if (host != null && guest != null)
            {
                host.IsHost = true;
                guest.IsHost = false;

                IgraSoba novaSoba = new(host, guest);

                lock (Program.RoomsLock)
                {
                    Program.aktivneSobe.Add(novaSoba);
                    Program.mapaSoba.TryAdd(host.Connection, novaSoba);
                    Program.mapaSoba.TryAdd(guest.Connection, novaSoba);
                }

                novaSoba.PrepareGame();
            }

            //queue
            /*igraciKojiCekaju.Enqueue(noviIgrac);
            Console.WriteLine("Igrac je dodat.");

            if (igraciKojiCekaju.Count > 1)
            {
                if (igraciKojiCekaju.TryDequeue(out Igrac hostIgrac) && igraciKojiCekaju.TryDequeue(out Igrac guestIgrac))
                {
                    Console.WriteLine("Igra se otvara.");
                    hostIgrac.IsHost = true;
                    guestIgrac.IsHost = false;

                    IgraSoba novaSoba = new (hostIgrac, guestIgrac);

                    novaSoba.PrepareGame();

                    Program.aktivneSobe.Add(novaSoba);
                    Program.mapaSoba.Add(hostIgrac.Connection, novaSoba);
                    Program.mapaSoba.Add(guestIgrac.Connection, novaSoba);
                }
            }*/
        }
    }
}
