using System;
using System.Collections.Generic;
using System.Text;

namespace BotMercadobitcoin.API.Retorno.Dados.v3
{
    public class Trade
    {
        /// <summary>
        /// Data e hora da negociação em Era Unix
        /// </summary>
        public int date { get; set; }

        /// <summary>
        /// Preço unitário da negociação.
        /// </summary>
        public decimal price { get; set; }

        /// <summary>
        /// Quantidade da negociação.
        /// </summary>
        public decimal amount { get; set; }

        /// <summary>
        /// Identificador da negociação.
        /// </summary>
        public int tid { get; set; }

        /// <summary>
        /// Indica a ponta executora da negociação 
        /// 
        /// Domínio de dados:
        ///     buy : indica ordem de compra executora
        ///     sell : indica ordem de venda executora
        /// </summary>
        public string type { get; set; }
    }
}
