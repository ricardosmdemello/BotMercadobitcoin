using System;
using System.Collections.Generic;
using System.Text;

namespace BotMercadobitcoin.API.Retorno.Dados.v3
{
    public class OrderbookRoot
    {
        /// <summary>
        /// Lista de ofertas de venda, ordenadas do menor para o maior preço.
        /// [0]: Preço unitário da oferta de venda.
        /// [1]: Quantidade da oferta de venda.
        /// </summary>
        public List<List<decimal>> asks { get; set; }

        /// <summary>
        /// Lista de ofertas de compras, ordenadas do maior para o menor preço.
        /// [0]: Preço unitário da oferta de compra.
        /// [1]: Quantidade da oferta de compra.
        /// </summary>
        public List<List<decimal>> bids { get; set; }
    }
}
