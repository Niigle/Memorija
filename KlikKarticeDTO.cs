using System;
using System.Collections.Generic;
using System.Text;

namespace Memorija
{
    internal class KlikKarticeDTO
    {

        int[][] kartica1;
        int[][] kartica2;

        int okrenutaKartica1;
        int okrenutaKartica2;
        internal bool isPogodak { get; set; }

        public KlikKarticeDTO() { }

        public KlikKarticeDTO(int[][] kartica1, int[][] kartica2)
        {
            this.kartica1 = kartica1;
            this.kartica2 = kartica2;
        }

    }
}
