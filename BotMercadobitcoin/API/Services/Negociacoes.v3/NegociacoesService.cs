using BotMercadobitcoin.API.Retorno.Dados.v3;
using BotMercadobitcoin.API.Retorno.Negociacoes.v3;
using BotMercadobitcoin.API.Retorno.Negociacoes.v3.AccountInfo;
using BotMercadobitcoin.Configuracao;
using BotMercadobitcoin.Interfaces.Dados.v3;
using BotMercadobitcoin.Interfaces.Negociacoes.v3;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BotMercadobitcoin.API.Services.Negociacoes.v3
{
    public class NegociacoesService : BaseService, INegociacoesService
    {
        private AppSettings AppSettings { get; set; }

        public NegociacoesService(AppSettings appSettings)
        {
            this.AppSettings = appSettings;
            
            Thread.Sleep(TimeSpan.FromSeconds(AppSettings.Sleep_tapi_nonce_Seconds));
        }

        /*
            tapi-nonce
            IMPORTANTE: devido às características da arquitetura web, 
            ocorrem casos em que as requisições são processadas em ordem diferente da ordem de envio.
                Isso pode causar problemas com a validação do tapi-nonce e bloquear requisições. 
                Por esse motivo serão aceitos valores ainda não utilizados, 
                desde que atendam o limite de até 30 números menores e estejam em um intervalo 
                de tempo de até 10 segundos após o último processado.
        */
        private int GetTapiNonce()
        {
            var random = new System.Random();
            int value = random.Next(0, 100000);

            var _t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
            return ((int)_t.TotalSeconds); // + value;
        }

        /// <summary>
        /// Retorna dados da conta, como saldos das moedas (Real, Bitcoin, Litecoin e BCash), 
        /// saldos considerando retenção em ordens abertas, quantidades de ordens abertas por moeda digital, 
        /// limites de saque/transferências das moedas.
        /// </summary>
        /// <returns></returns>
        public async Task<AccountInfoRoot> GetAccountInfo()
        {
            var tapiNonce = GetTapiNonce();
            var request = this.GetRequest($"tapi/v3/", Method.POST);

            request.AddParameter("tapi_method", "get_account_info"); // adds to POST
            request.AddParameter("tapi_nonce", $"{tapiNonce}");

            // TAPI-ID: Identificador da TAPI, utilizado para referenciar a conta e chave TAPI do usuário.
            var tapi_id = this.AppSettings.tapi_id; // Meu Identificador do mercadobitcoin
            var tapi_segredo = this.AppSettings.tapi_segredo;
            var url = $"/tapi/v3/?tapi_method=get_account_info&tapi_nonce={tapiNonce}";

            var tapi_mac = CriaTapiMac(url, tapi_segredo);

            request.AddHeader("tapi-id", tapi_id);
            request.AddHeader("tapi-mac", tapi_mac);

            IRestResponse response = await this.Client.ExecuteTaskAsync(request);

            return JsonConvert.DeserializeObject<AccountInfoRoot>(response.Content);
        }

        /// <summary>
        /// Abre uma ordem de compra (buy ou bid) do par de moedas, quantidade de moeda digital e preço unitário limite informados. 
        /// A criação contempla o processo de confrontamento da ordem com o livro de negociações. 
        /// Assim, a resposta pode informar se a ordem foi executada (parcialmente ou não) 
        /// imediatamente após sua criação e, assim, se segue ou não aberta e ativa no livro.
        /// Valor Mínimo de compra BTC: 0,001
        /// 
        ///  vlrBTCBuy = valor em BTC da compra
        /// </summary>
        /// <returns></returns>
        public async Task<Retorno.Negociacoes.v3.PlaceBuyOrder.OrderRoot> PlaceBuyOrder(/*decimal vlrBTC,*/ Retorno.Negociacoes.v3.ListOrderbook.Ask ask, decimal vlrBTCBuy)
        {
            //decimal qtBTC = (vlrReal * 1) / (vlrBTC + 165);
            //decimal ask_limit_price = decimal.Parse(ask.limit_price.Replace(".", ","));

            //decimal qtBTC = (vlrBTCBuy * 1) / ask_limit_price;
            string strVlrBTCBuy = vlrBTCBuy.ToString("n8").Replace(",", ".");

            // if (vlrBTCBuy < 0.001m)
            //     throw new Exception($"Valor {vlrBTCBuy} e menor que o limite de 0.001 BTC");

            //string strVlrCompraBTC = vlrReal.ToString("n2").Replace(",", ".");

            var tapiNonce = GetTapiNonce();
            var request = this.GetRequest($"tapi/v3/", Method.POST);

            request.AddParameter("tapi_method", "place_buy_order"); // adds to POST
            request.AddParameter("tapi_nonce", $"{tapiNonce}");
            request.AddParameter("coin_pair", "BRLBTC");
            request.AddParameter("quantity", strVlrBTCBuy);
            request.AddParameter("limit_price", ask.limit_price);

            // TAPI-ID: Identificador da TAPI, utilizado para referenciar a conta e chave TAPI do usuário.
            var tapi_id = this.AppSettings.tapi_id; // Meu Identificador do mercadobitcoin
            var tapi_segredo = this.AppSettings.tapi_segredo;
            var url = $"/tapi/v3/?tapi_method=place_buy_order&tapi_nonce={tapiNonce}&coin_pair=BRLBTC&quantity={strVlrBTCBuy}&limit_price={ask.limit_price}";

            var tapi_mac = CriaTapiMac(url, tapi_segredo);

            request.AddHeader("tapi-id", tapi_id);
            request.AddHeader("tapi-mac", tapi_mac);

            IRestResponse response = await this.Client.ExecuteTaskAsync(request);

            return JsonConvert.DeserializeObject<Retorno.Negociacoes.v3.PlaceBuyOrder.OrderRoot>(response.Content);
        }


        /// <summary>
        /// Abre uma ordem de venda (sell ou ask) do par de moedas, quantidade de moeda digital e preço unitário limite informados. 
        /// A criação contempla o processo de confrontamento da ordem com o livro de negociações. Assim, 
        /// a resposta pode informar se a ordem foi executada (parcialmente ou não) 
        /// imediatamente após sua criação e, assim, se segue ou não aberta e ativa no livro.
        /// </summary>
        /// <param name="vlrBTCAtual">Valor atual de COMPRA do BTC em REAL - DO COMPRADOR</param>
        /// <param name="qntBTCVenda">Valor de BTCs que eu quero VENDER</param>
        /// <returns></returns>
        public async Task<Retorno.Negociacoes.v3.PlaceSellOrder.OrderRoot> PlaceSellOrder(decimal qntBTCVenda, string vlrVendaBTC)
        {
            string strQtBTC = qntBTCVenda.ToString("n8").Replace(",", ".");

            //string strVlrVendaBTC = vlrVendaBTC.ToString(); // .ToString("n2").Replace(",", ".");

            var tapiNonce = GetTapiNonce();
            var request = this.GetRequest($"tapi/v3/", Method.POST);

            request.AddParameter("tapi_method", "place_sell_order");
            request.AddParameter("tapi_nonce", $"{tapiNonce}");
            request.AddParameter("coin_pair", "BRLBTC");

            // Quantidade da moeda digital a comprar/vender ao preço de limit_price.
            // Formato: Ponto como separador decimal, sem separador de milhar, até 8 casas decimais
            // Valor Mínimo: 0,001 BTC
            request.AddParameter("quantity", strQtBTC);

            // Preço unitário máximo de compra ou mínimo de venda.
            // Formato: Ponto como separador decimal, sem separador de milhar, até 5 casas decimais
            // Valor Mínimo: R$ 0,01
            request.AddParameter("limit_price", vlrVendaBTC);

            // TAPI-ID: Identificador da TAPI, utilizado para referenciar a conta e chave TAPI do usuário.
            var tapi_id = this.AppSettings.tapi_id; // Meu Identificador do mercadobitcoin
            var tapi_segredo = this.AppSettings.tapi_segredo;
            var url = $"/tapi/v3/?tapi_method=place_sell_order&tapi_nonce={tapiNonce}&coin_pair=BRLBTC&quantity={strQtBTC}&limit_price={vlrVendaBTC}";

            var tapi_mac = CriaTapiMac(url, tapi_segredo);

            request.AddHeader("tapi-id", tapi_id);
            request.AddHeader("tapi-mac", tapi_mac);

            IRestResponse response = await this.Client.ExecuteTaskAsync(request);
            //var content = response.Content;

            return JsonConvert.DeserializeObject<Retorno.Negociacoes.v3.PlaceSellOrder.OrderRoot>(response.Content);
        }

        /// <summary>
        /// Abre uma ordem de venda (sell ou ask) do par de moedas, quantidade de moeda digital e preço unitário limite informados. 
        /// A criação contempla o processo de confrontamento da ordem com o livro de negociações.
        /// Assim, a resposta pode informar se a ordem foi executada (parcialmente ou não) imediatamente após sua criação e, 
        /// assim, se segue ou não aberta e ativa no livro.
        /// </summary>
        /// <returns></returns>
        public async Task<BotMercadobitcoin.API.Retorno.Negociacoes.v3.ListOrderbook.OrderbookRoot> ListOrderbook()
        {
            var tapiNonce = GetTapiNonce();
            var request = this.GetRequest($"tapi/v3/", Method.POST);

            request.AddParameter("tapi_method", "list_orderbook");
            request.AddParameter("tapi_nonce", $"{tapiNonce}");
            request.AddParameter("coin_pair", "BRLBTC");

            // TAPI-ID: Identificador da TAPI, utilizado para referenciar a conta e chave TAPI do usuário.
            var tapi_id = this.AppSettings.tapi_id; // Meu Identificador do mercadobitcoin
            var tapi_segredo = this.AppSettings.tapi_segredo;
            var url = $"/tapi/v3/?tapi_method=list_orderbook&tapi_nonce={tapiNonce}&coin_pair=BRLBTC";

            var tapi_mac = CriaTapiMac(url, tapi_segredo);

            request.AddHeader("tapi-id", tapi_id);
            request.AddHeader("tapi-mac", tapi_mac);

            IRestResponse response = await this.Client.ExecuteTaskAsync(request);
            //var content = response.Content;

            return JsonConvert.DeserializeObject<BotMercadobitcoin.API.Retorno.Negociacoes.v3.ListOrderbook.OrderbookRoot>(response.Content);
        }

        /// <summary>
        /// ancela uma ordem, de venda ou compra, de acordo com o ID e par de moedas informado. 
        /// O retorno contempla o sucesso ou não do cancelamento, bem como os dados e status atuais da ordem. 
        /// Somente ordens pertencentes ao próprio usuário podem ser canceladas.
        /// </summary>
        /// <param name="order_id"></param>
        /// <returns></returns>
        public async Task<Retorno.Negociacoes.v3.PlaceSellOrder.OrderRoot> CancelOrder(int order_id)
        {
            var tapiNonce = GetTapiNonce();
            var request = this.GetRequest($"tapi/v3/", Method.POST);

            request.AddParameter("tapi_method", "cancel_order");
            request.AddParameter("tapi_nonce", $"{tapiNonce}");
            request.AddParameter("coin_pair", "BRLBTC");

            request.AddParameter("order_id", order_id);

            // TAPI-ID: Identificador da TAPI, utilizado para referenciar a conta e chave TAPI do usuário.
            var tapi_id = this.AppSettings.tapi_id; // Meu Identificador do mercadobitcoin
            var tapi_segredo = this.AppSettings.tapi_segredo;
            var url = $"/tapi/v3/?tapi_method=cancel_order&tapi_nonce={tapiNonce}&coin_pair=BRLBTC";

            var tapi_mac = CriaTapiMac(url, tapi_segredo);

            request.AddHeader("tapi-id", tapi_id);
            request.AddHeader("tapi-mac", tapi_mac);

            IRestResponse response = await this.Client.ExecuteTaskAsync(request);
            //var content = response.Content;

            return JsonConvert.DeserializeObject<Retorno.Negociacoes.v3.PlaceSellOrder.OrderRoot>(response.Content);
        }
        
        public async Task<Retorno.Negociacoes.v3.PlaceSellOrder.OrderRoot> GetOrder(int order_id)
        {
            var tapiNonce = GetTapiNonce();
            var request = this.GetRequest($"tapi/v3/", Method.POST);

            request.AddParameter("tapi_method", "get_order");
            request.AddParameter("tapi_nonce", $"{tapiNonce}");
            request.AddParameter("coin_pair", "BRLBTC");

            request.AddParameter("order_id", order_id);

            // TAPI-ID: Identificador da TAPI, utilizado para referenciar a conta e chave TAPI do usuário.
            var tapi_id = this.AppSettings.tapi_id; // Meu Identificador do mercadobitcoin
            var tapi_segredo = this.AppSettings.tapi_segredo;
            var url = $"/tapi/v3/?tapi_method=get_order&tapi_nonce={tapiNonce}&coin_pair=BRLBTC";

            var tapi_mac = CriaTapiMac(url, tapi_segredo);

            request.AddHeader("tapi-id", tapi_id);
            request.AddHeader("tapi-mac", tapi_mac);

            IRestResponse response = await this.Client.ExecuteTaskAsync(request);
            //var content = response.Content;

            return JsonConvert.DeserializeObject<Retorno.Negociacoes.v3.PlaceSellOrder.OrderRoot>(response.Content);
        }
    }
}

