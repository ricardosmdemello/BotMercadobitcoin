using System;
using System.Collections.Generic;
using System.Text;

namespace BotMercadobitcoin.SQLite.Domain
{
    public class Ticker
    {
        public int Id { get; set; }
        public string Data { get; set; }

        /// <summary>
        /// Valor do BTC
        /// </summary>
        public decimal Valor { get; set; }

        // Valor da media menor
        public decimal? mediaMenorSoma { get; set; }

        // Valor da media maior
        public decimal? mediaMaiorSoma { get; set; }

        public decimal? mediaValidaSoma { get; set; }

        // Numero de periodos usado para calcular a media maior
        public int mediaMaior { get; set; }

        // Numero de periodos usado para calcular a media Menor
        public int mediaMenor { get; set; }
        public int? mediaValida { get; set; }
    }
}
