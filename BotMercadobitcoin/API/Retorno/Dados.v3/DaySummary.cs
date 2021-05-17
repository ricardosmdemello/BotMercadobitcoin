using System;
using System.Collections.Generic;
using System.Text;

namespace BotMercadobitcoin.API.Retorno.Dados.v3
{
    public class DaySummary
    {
        /// <summary>
        /// Menor preço unitário de negociação no dia.
        /// </summary>
        public decimal lowest { get; set; }

        /// <summary>
        /// Volume de Reais (BRL) negociados no dia.
        /// </summary>
        public decimal volume { get; set; }

        /// <summary>
        /// Número de negociações realizadas no dia.
        /// </summary>
        public int amount { get; set; }

        /// <summary>
        /// Preço unitário médio das negociações no dia.
        /// </summary>
        public decimal avg_price { get; set; }

        /// <summary>
        /// Preço unitário de abertura de negociação no dia.
        /// </summary>
        public decimal opening { get; set; }

        /// <summary>
        /// Data do resumo diário
        /// Formato: AAAA-MM-DD, exemplo: 2013-06-20
        /// </summary>
        public string date { get; set; }

        /// <summary>
        /// Preço unitário de fechamento de negociação no dia.
        /// </summary>
        public decimal closing { get; set; }

        /// <summary>
        /// Maior preço unitário de negociação no dia.
        /// </summary>
        public decimal highest { get; set; }

        /// <summary>
        /// Quantidade da moeda digital negociada no dia.
        /// </summary>
        public decimal quantity { get; set; }
    }
}
