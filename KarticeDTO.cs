using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Linq;

namespace Memorija
{
    internal class KarticeDTO
    {

        internal static readonly string[] listaKartica = Enumerable.Range(1, 50)
                                                                   .Select(i => $"sprite{i}")
                                                                   .ToArray();
        public string naziv;

        public KarticeDTO(string naziv)
        {
            this.naziv = naziv;
        }

    }
}
