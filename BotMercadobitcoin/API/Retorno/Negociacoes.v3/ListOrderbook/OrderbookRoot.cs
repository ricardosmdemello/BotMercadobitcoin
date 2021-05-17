using System;
using System.Collections.Generic;
using System.Text;

namespace BotMercadobitcoin.API.Retorno.Negociacoes.v3.ListOrderbook
{
    public class OrderbookRoot
    {
        public ResponseData response_data { get; set; }
        public int status_code { get; set; }
        public string server_unix_timestamp { get; set; }
    }

    public class Bid
    {
        public int order_id { get; set; }
        public string quantity { get; set; }
        public string limit_price { get; set; }
        public bool is_owner { get; set; }
    }

    public class Ask
    {
        public int order_id { get; set; }
        public string quantity { get; set; }
        public string limit_price { get; set; }
        public bool is_owner { get; set; }
    }

    public class Orderbook
    {
        public List<Bid> bids { get; set; }
        public List<Ask> asks { get; set; }
        public int latest_order_id { get; set; }
    }

    public class ResponseData
    {
        public Orderbook orderbook { get; set; }
    }
}
