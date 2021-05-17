using System;
using System.Collections.Generic;
using System.Text;

namespace BotMercadobitcoin.SQLite.Domain
{
    public class Trade
    {
        public int Id { get; set; }
        public string Data { get; set; }
        public int Tipo { get; set; }
        public decimal Valor { get; set; }
        public string Json { get; set; }
    }
}
