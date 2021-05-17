using System;
using System.Collections.Generic;
using System.Text;

namespace BotMercadobitcoin.SQLite.Domain
{
    public class Log
    {
        public int Id { get; set; }
        public string Data { get; set; }
        public string LogText { get; set; }
        public int Tipo { get; set; }
    }
}
