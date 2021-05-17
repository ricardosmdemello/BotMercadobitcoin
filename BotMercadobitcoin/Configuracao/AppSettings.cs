using System;
using System.Collections.Generic;
using System.Text;

namespace BotMercadobitcoin.Configuracao
{
    public class AppSettings
    {
        public int SleepFromMinutes { get; set; }
        public int SleepFromSeconds { get; set; }
        public int Sleep_tapi_nonce_Seconds { get; set; }
        public int OrdemSleepFromSeconds { get; set; }
        public string tapi_id { get; set; }
        public string tapi_segredo { get; set; }
        public decimal limite_de_transacao { get; set; }
        public int ordemEmProcessamentoCountLimite { get; set; }
        public int mediaMaior { get; set; }
        public int mediaMenor { get; set; }

        /// 3º media
        public int mediaValida { get; set; }
    }
}
