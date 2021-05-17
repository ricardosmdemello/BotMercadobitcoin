using System;
using System.Collections.Generic;
using System.Text;

namespace BotMercadobitcoin.API.Retorno.Negociacoes.v3.PlaceBuyOrder
{
    public class OrderRoot
    {
        public ResponseData response_data { get; set; }
        public int status_code { get; set; }
    }

    public class Order
    {
        public int order_id { get; set; }
        public string coin_pair { get; set; }

        /// <summary>
        /// Tipo da ordem a ser filtrado. 
        /// 1 : Ordem de compra
        /// 2 : Ordem de venda
        /// </summary>
        public int order_type { get; set; }

        /// <summary>
        /// Estado da ordem. 
        /// 2 : open : Ordem aberta, disponível no livro de negociações. Estado intermediário.
        /// 3 : canceled : Ordem cancelada, executada parcialmente ou sem execuções.Estado final.
        /// 4 : filled : Ordem concluída, executada em sua totalidade. Estado final.
        /// </summary>
        public int status { get; set; }

        public bool has_fills { get; set; }
        public string quantity { get; set; }
        public string limit_price { get; set; }
        public string executed_quantity { get; set; }
        public string executed_price_avg { get; set; }

        /// <summary>
        /// Comissão da ordem, para ordens de compra os valores são em moeda digital, para ordens de venda os valores são em Reais.
        /// Formato: Ponto como separador decimal, sem separador de milhar
        /// </summary>
        public string fee { get; set; }

        /// <summary>
        /// Data e hora de criação da ordem.
        /// Format: Era Unix
        /// </summary>
        public string created_timestamp { get; set; }

        /// <summary>
        /// Data e hora da última atualização da ordem. Não é alterado caso a ordem esteja em um estado final (ver status).
        /// Format: Era Unix
        /// </summary>
        public string updated_timestamp { get; set; }
        public List<object> operations { get; set; }
    }

    public class ResponseData
    {
        public Order order { get; set; }
    }
}
