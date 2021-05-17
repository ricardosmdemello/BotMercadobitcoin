using System;
using System.Collections.Generic;
using System.Text;

namespace BotMercadobitcoin.API.Retorno.Dados.v3
{
    public class Ticker
    {
        /// <summary>
        /// high: Maior preço unitário de negociação das últimas 24 horas.
        /// </summary>
        public decimal high { get; set; }

        /// <summary>
        /// Menor preço unitário de negociação das últimas 24 horas.
        /// </summary>
        public decimal low { get; set; }

        /// <summary>
        /// vol: Quantidade negociada nas últimas 24 horas.
        /// </summary>
        public decimal vol { get; set; }

        /// <summary>
        /// last: Preço unitário da última negociação.
        /// </summary>
        public decimal last { get; set; }

        /// <summary>
        /// buy: Maior preço de oferta de compra das últimas 24 horas.
        /// </summary>
        public decimal buy { get; set; }

        /// <summary>
        /// sell: Menor preço de oferta de venda das últimas 24 horas.
        /// </summary>
        public decimal sell { get; set; }

        /// <summary>
        /// date: Data e hora da informação em Era Unix
        /// </summary>
        public int date { get; set; }
    }
}
