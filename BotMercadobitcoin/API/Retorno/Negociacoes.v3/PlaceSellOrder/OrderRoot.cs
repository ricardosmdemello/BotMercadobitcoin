using System;
using System.Collections.Generic;
using System.Text;

namespace BotMercadobitcoin.API.Retorno.Negociacoes.v3.PlaceSellOrder
{
    public class OrderRoot
    {
        public ResponseData response_data { get; set; }
        public int status_code { get; set; }
        public string server_unix_timestamp { get; set; }
    }

    public class Operation
    {
        public int operation_id { get; set; }
        public string quantity { get; set; }
        public string price { get; set; }
        public string fee_rate { get; set; }
        public string executed_timestamp { get; set; }
    }

    public class Order
    {
        public int order_id { get; set; }
        public string coin_pair { get; set; }
        public int order_type { get; set; }
        public int status { get; set; }
        public bool has_fills { get; set; }
        public string quantity { get; set; }
        public string limit_price { get; set; }
        public string executed_quantity { get; set; }
        public string executed_price_avg { get; set; }
        public string fee { get; set; }
        public string created_timestamp { get; set; }
        public string updated_timestamp { get; set; }
        public List<Operation> operations { get; set; }
    }

    public class ResponseData
    {
        public Order order { get; set; }
    }
}
