using BotMercadobitcoin.API.Retorno.Dados.v3;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BotMercadobitcoin.Interfaces.Dados.v3
{
    public interface IDadosService
    {
        Task<OrderbookRoot> GetOrderbook();
        Task<TickerRoot> GetTicker();
        Task<List<Trade>> GetTrades();
        Task<DaySummary> GetDaySummary(int year, int month, int day);
    }
}
