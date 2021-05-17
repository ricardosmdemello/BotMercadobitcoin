using BotMercadobitcoin.API.Retorno.Dados.v3;
using BotMercadobitcoin.Interfaces.Dados.v3;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BotMercadobitcoin.API.Services.Dados.v3
{
    public class DadosService : BaseService, IDadosService
    {
        /// <summary>
        /// Livro de ofertas é composto por duas listas:
        ///     (1) uma lista com as ofertas de compras ordenadas pelo maior valor; 
        ///     (2) uma lista com as ofertas de venda ordenadas pelo menor valor. O livro mostra até 1000 ofertas de compra e até 1000 ofertas de venda.
        ///     
        ///  Uma oferta é constituída por uma ou mais ordens, sendo assim, a quantidade da oferta é o resultado da soma
        ///  das quantidades das ordens de mesmo preço unitário.Caso uma oferta represente mais de uma ordem, 
        ///  a prioridade de execução se dá com base na data de criação da
        ///  
        /// Url: https://www.mercadobitcoin.net/api/coin/orderbook/
        /// </summary>
        /// <returns></returns>
        public async Task<OrderbookRoot> GetOrderbook()
        {
            var request = this.GetRequest($"api/{this.Coin.ToString()}/orderbook/", Method.GET);

            return await this.Client.GetTaskAsync<OrderbookRoot>(request);
        }

        /// <summary>
        /// Retorna informações com o resumo das últimas 24 horas de negociações.
        /// Url: https://www.mercadobitcoin.net/api/coin/ticker/
        /// </summary>
        /// <returns></returns>
        public async Task<TickerRoot> GetTicker()
        {
            var request = this.GetRequest($"api/{this.Coin.ToString()}/ticker/", Method.GET);

            return await this.Client.GetTaskAsync<TickerRoot>(request);
        }

        #region Trades
        /// <summary>
        /// Histórico de negociações realizadas.
        /// 
        /// A chamada tradicional do histórico irá retornar as últimas 1000 negociações. 
        /// Para obter dados anteriores, é necessário utilizar outros parâmetros conforme descrito e exemplificado abaixo:
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<Trade>> GetTrades()
        {
            var request = this.GetRequest($"api/{this.Coin.ToString()}/trades/", Method.GET);

            return await this.Client.GetTaskAsync<List<Trade>>(request);
        }

        /// <summary>
        /// Histórico de negociações realizadas.
        /// 
        /// tid: Retorna até 1000 negociações a partir do identificador da negociação informado.
        /// https://www.mercadobitcoin.net/api/coin/trades/?tid=<tid>
        /// </summary>
        /// <param name="tid"></param>
        /// <returns></returns>
        public async Task<List<Trade>> GetTradesTid(int tid)
        {
            var request = this.GetRequest($"api/{this.Coin.ToString()}/trades/?tid={tid}", Method.GET);

            return await this.Client.GetTaskAsync<List<Trade>>(request);
        }

        /// <summary>
        /// Histórico de negociações realizadas.
        /// 
        /// since: Retorna até 1000 negociações a partir do identificador da negociação informado.
        /// https://www.mercadobitcoin.net/api/coin/trades/?since=<since>
        /// </summary>
        /// <param name="since"></param>
        /// <returns></returns>
        public async Task<List<Trade>> GetTradesSince(int since)
        {
            var request = this.GetRequest($"api/{this.Coin.ToString()}/trades/?since={since}", Method.GET);

            return await this.Client.GetTaskAsync<List<Trade>>(request);
        }

        /// <summary>
        /// Histórico de negociações realizadas.
        /// 
        /// from: Retorna até 1000 negociações a partir da data informada.
        /// https://www.mercadobitcoin.net/api/coin/trades/<from>/
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public async Task<List<Trade>> GetTradesFrom(DateTime from)
        {
            var urlFrom = (DateTime.UtcNow - new DateTime(from.Year, from.Month, from.Day, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

            var request = this.GetRequest($"api/{this.Coin.ToString()}/trades/{urlFrom}/", Method.GET);

            return await this.Client.GetTaskAsync<List<Trade>>(request);
        }

        /// <summary>
        /// Histórico de negociações realizadas.
        /// 
        /// from-to: Retorna até 1000 negociações entre o intervalo de timestamp informado.
        /// https://www.mercadobitcoin.net/api/coin/trades/<from>/<to>/
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public async Task<List<Trade>> GetTradesFromTo(DateTime from, DateTime to)
        {
            // Exemplo
            //var epoch = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            //var urlFrom = (DateTime.UtcNow - new DateTime(from.Year, from.Month, from.Day, from.Hour, from.Minute, from.Second, DateTimeKind.Utc)).TotalSeconds.ToString();
            //var urlTo = (DateTime.UtcNow - new DateTime(to.Year, to.Month, from.Day, to.Hour, to.Minute, to.Second, DateTimeKind.Utc)).TotalSeconds.ToString();

            TimeSpan _t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
            var _time = Convert.ToString((int)_t.TotalSeconds);

            var urlFrom1 = (DateTime.UtcNow - new DateTime(2017, 12, 1, 0, 0, 0, DateTimeKind.Local));
            var urlFrom2 = (DateTime.UtcNow - new DateTime(2017, 12, 1, 0, 0, 0, DateTimeKind.Utc));

            // Start of month: 	1512093600	  Sexta-feira, 1 de dezembro de 2017 às 00:00:00 GMT-02:00
            // End of month:    1514771999    Domingo, 31 de dezembro de 2017 às 23:59:59 GMT - 02:00
            var urlFrom = Convert.ToString((int)(DateTime.UtcNow - new DateTime(from.Year, from.Month, 1, 0, 0, 0)).TotalSeconds);
            var urlTo = Convert.ToString((int)(DateTime.UtcNow - new DateTime(to.Year, to.Month, 31, 23, 59, 59)).TotalSeconds);

            //var urlFrom = new DateTime(from.Year, from.Month, from.Day, from.Hour, from.Minute, from.Second, DateTimeKind.Utc).ToFileTimeUtc();
            //var urlTo = new DateTime(to.Year, to.Month, from.Day, to.Hour, to.Minute, to.Second, DateTimeKind.Utc).ToFileTimeUtc();

            var request = this.GetRequest($"api/{this.Coin.ToString()}/trades/{urlFrom}/{urlTo}/", Method.GET);

            IRestResponse response =  this.Client.Execute(request);
            //IRestResponse response = await this.Client.ExecuteTaskAsync(request);

            //return JsonConvert.DeserializeObject<AccountInfoRoot>(response.Content);

            return await this.Client.GetTaskAsync<List<Trade>>(request);
        }
        #endregion

        /// <summary>
        /// Retorna resumo diário de negociações realizadas.
        /// https://www.mercadobitcoin.net/api/coin/day-summary/<year>/<month>/<day>/
        /// year, month, day: Respectivamente ano, mês e dia referente ao dia do ano requisitado.
        /// </summary>
        /// <returns></returns>
        public async Task<DaySummary> GetDaySummary(int year, int month, int day)
        {
            var request = this.GetRequest($"api/{this.Coin.ToString()}/day-summary/{year}/{month}/{day}/", Method.GET);

            return await this.Client.GetTaskAsync<DaySummary>(request);
        }

        public override void Dispose()
        {
            base.Dispose(true);
        }
    }
}
