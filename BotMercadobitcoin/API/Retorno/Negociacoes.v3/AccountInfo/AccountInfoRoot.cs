using System;
using System.Collections.Generic;
using System.Text;

namespace BotMercadobitcoin.API.Retorno.Negociacoes.v3.AccountInfo
{
    public class AccountInfoRoot
    {
        public ResponseData response_data { get; set; }
        public int status_code { get; set; }
        public string server_unix_timestamp { get; set; }
    }

    /// <summary>
    /// Real
    /// </summary>
    public class Brl
    {
        /// <summary>
        /// Saldo em real disponivel para operações
        /// </summary>
        public string available { get; set; }

        /// <summary>
        /// Total e real, available mais ordens de compra em aberto
        /// </summary>
        public string total { get; set; }
    }
    
    public class Btc
    {
        public string available { get; set; }
        
        public string total { get; set; }
        public int amount_open_orders { get; set; }
    }

    public class Ltc
    {
        public string available { get; set; }
        public string total { get; set; }
        public int amount_open_orders { get; set; }
    }

    public class Bch
    {
        public string available { get; set; }
        public string total { get; set; }
        public int amount_open_orders { get; set; }
    }

    public class Balance
    {
        public Brl brl { get; set; }
        public Btc btc { get; set; }
        public Ltc ltc { get; set; }
        public Bch bch { get; set; }
    }

    public class Brl2
    {
        public string available { get; set; }
        public string total { get; set; }
    }

    public class Btc2
    {
        public string available { get; set; }
        public string total { get; set; }
    }

    public class Ltc2
    {
        public string available { get; set; }
        public string total { get; set; }
    }

    public class Bch2
    {
        public string available { get; set; }
        public string total { get; set; }
    }

    public class WithdrawalLimits
    {
        public Brl2 brl { get; set; }
        public Btc2 btc { get; set; }
        public Ltc2 ltc { get; set; }
        public Bch2 bch { get; set; }
    }

    public class ResponseData
    {
        public Balance balance { get; set; }
        public WithdrawalLimits withdrawal_limits { get; set; }
    }
}
