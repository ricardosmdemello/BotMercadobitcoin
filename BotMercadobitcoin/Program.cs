using BotMercadobitcoin.API.Retorno.Negociacoes.v3.AccountInfo;
using BotMercadobitcoin.API.Services.Dados.v3;
using BotMercadobitcoin.API.Services.Negociacoes.v3;
using BotMercadobitcoin.Configuracao;
using BotMercadobitcoin.Interfaces.Dados.v3;
using BotMercadobitcoin.SQLite;
using BotMercadobitcoin.SQLite.Domain;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BotMercadobitcoin
{
    public enum Status : int
    {
        NadaFeito = 0,
        CompraFeita = 1,
        VendaFeita =2
    }

    class Program
    {
        /// <summary>
        /// https://medium.com/@rsdsantos/configurando-appsettings-json-em-aplica%C3%A7%C3%B5es-net-core-2-94eb4e035660
        /// </summary>
        public static IConfigurationRoot Configuration { get; set; }
        private static AppSettings AppSettings { get; set; }

        static void Main(string[] args)
        {
            Console.WriteLine("Inicio.");
            Console.WriteLine("Lendo as configurações.");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
            
            var connectionString = Configuration.GetConnectionString("DefaultConnection");

            AppSettings = new AppSettings
            {
                SleepFromMinutes = int.Parse(Configuration.GetSection("AppSettings:SleepFromMinutes").Value),
                SleepFromSeconds = int.Parse(Configuration.GetSection("AppSettings:SleepFromSeconds").Value),
                Sleep_tapi_nonce_Seconds = int.Parse(Configuration.GetSection("AppSettings:Sleep_tapi_nonce_Seconds").Value),
                OrdemSleepFromSeconds = int.Parse(Configuration.GetSection("AppSettings:OrdemSleepFromSeconds").Value),
                tapi_id = Configuration.GetSection("AppSettings:tapi_id").Value,
                tapi_segredo = Configuration.GetSection("AppSettings:tapi_segredo").Value,
                limite_de_transacao = decimal.Parse(Configuration.GetSection("AppSettings:limite_de_transacao").Value),
                ordemEmProcessamentoCountLimite = int.Parse(Configuration.GetSection("AppSettings:ordemEmProcessamentoCountLimite").Value),
                mediaMaior = int.Parse(Configuration.GetSection("AppSettings:mediaMaior").Value),
                mediaMenor = int.Parse(Configuration.GetSection("AppSettings:mediaMenor").Value),
                mediaValida = int.Parse(Configuration.GetSection("AppSettings:mediaValida").Value)
            };

            #region Teste SQLLite

            //decimal.TryParse("0.04434151", out decimal vlrREALPrimeiraVezBot);

            /*
            var strData = new DateTimeOffset(DateTime.Now).UtcDateTime.ToString();

            using (var context = new BotDBContext())
            {
                //var trade = new Trade
                //{
                //    Data = strData,
                //    Tipo = 1,
                //    Json = "Json",
                //    Valor = 0.001m
                //};

                //context.Trades.Add(trade);
                //context.SaveChanges();

                var medias9 = context.Tickers.OrderByDescending(_ => _.Id).Take(9).ToList();
                var medias21 = context.Tickers.OrderByDescending(_ => _.Id).Take(21).ToList();

                // Quando a media 9 passar a 21 "para cima", compra "automatico"
                var medias9Resultado = medias9.Sum(_ => _.Valor) / 9;

                // Quando a media 9 passar a 21 "para baixo", vende "automatico"
                var medias21Resultado = medias21.Sum(_ => _.Valor) / 21;

                // ----------------------------

                var logs = context.Logs.ToList();

                var log = new Log
                {
                    Data = strData,
                    LogText = "Teste - LogText",
                    Tipo = 1
                };

                context.Logs.Add(log);
                context.SaveChanges();
            }
            */
            #endregion

            MainAsync(args).GetAwaiter().GetResult();

            Console.ReadKey();
        }

        // dotnet publish -c Release -r win10-x64
        public static async Task MainAsync(string[] args)
        {
            try
            {
                var mediaMenorCount = 0;
                var mediaMaiorCount = 0;
                var mediaValidaCount = 0;

                using (var dadosService = new DadosService())
                {
                    //**
                    //// int year, int month, int day, int hour, int minute, int second
                    //var from = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 10, 0, 0);
                    //var to = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 11, 0, 0);

                    //var resultados = dadosService.GetTradesFromTo(from, to);
                    //**

                    //var orderbookroot = await dadosservice.GetOrderbook();
                    //var traderoot = await dadosservice.GetTrades();
                    //var daysummary = await dadosservice.GetDaySummary(2017, 10, 1);

                    var status = Status.NadaFeito;

                    // Indica que o bot esta sendo iniciado pela primeira vez, 
                    // verifica se tem REAL e so COMPRA quando a "mediaMenor FOR >  mediaMaior", quando passar a linha da media mediaMaior.
                    var primeiraVezBot = true;

                    do // Laço PRINCIPAL
                    {
                        if (primeiraVezBot)
                        {
                            try
                            {
                                // º - obtem os dados das minha conta
                                AccountInfoRoot accountInfoPrimeiraVezBot = null;
                                using (var negociacoesService = new NegociacoesService(AppSettings))
                                    accountInfoPrimeiraVezBot = await negociacoesService.GetAccountInfo();

                                //decimal.TryParse(accountInfoPrimeiraVezBot.response_data.balance.brl.available.Replace(".", ","), out decimal vlrREALPrimeiraVezBot);
                                //decimal.TryParse(accountInfoPrimeiraVezBot.response_data.balance.btc.available.Replace(".", ","), out decimal vlrBTCAvPrimeiraVezBot);

                                decimal.TryParse(accountInfoPrimeiraVezBot.response_data.balance.brl.available, out decimal vlrREALPrimeiraVezBot);
                                decimal.TryParse(accountInfoPrimeiraVezBot.response_data.balance.btc.available, out decimal vlrBTCAvPrimeiraVezBot);

                                Console.WriteLine($"BOT - REAL - brl.available = {accountInfoPrimeiraVezBot.response_data.balance.brl.available}");
                                Console.WriteLine($"BOT - BTC  - btc.available  = {accountInfoPrimeiraVezBot.response_data.balance.btc.available}");

                                Console.WriteLine($"BOT - REAL = {vlrREALPrimeiraVezBot}");
                                Console.WriteLine($"BOT - BTC  = {vlrBTCAvPrimeiraVezBot}");

                                // Obtem o valor atual do BTC ** //
                                decimal vlrBTCAtualPrimeiraVezBot = 0m;
                                var tickerrootPrimeiraVezBot = await dadosService.GetTicker();

                                if (tickerrootPrimeiraVezBot != null)
                                    vlrBTCAtualPrimeiraVezBot = tickerrootPrimeiraVezBot.ticker.last;
                                // *******************************//

                                decimal vlrBTCAvailableSite = ConvertRealParaBTC(vlrREALPrimeiraVezBot, vlrBTCAtualPrimeiraVezBot);

                                Console.WriteLine($"BOT - vlrBTCAvailableSite = vlrRealAvailable / vlrBTCAtual : {vlrBTCAvailableSite}"); 

                                // Se eu tenho REAl na minha conta eu COMPRA = status = Status.VendaFeita
                                // if (status != Status.NadaFeito && status != Status.CompraFeita) // compra "automatico"
                                if (vlrBTCAvailableSite > 0.001m) // 1º REAL
                                {
                                    Console.WriteLine($"BOT - vlrBTCAvailableSite > 0.001m = {vlrBTCAvailableSite}");
                                    status = Status.VendaFeita; // O BOT vai começar COMPRADO
                                    Console.WriteLine($"BOT - Começar COMPRADO!");
                                }
                                else // 2º
                                {        
                                    // Se eu tenho BTC na minha conta eu VENDO = status = Status.CompraFeita
                                    // if (status != Status.NadaFeito && status != Status.VendaFeita) // vende "automatico"
                                    if (vlrBTCAvPrimeiraVezBot > 0.001m) // Se o valor em BTC e maior que o minimo
                                    {
                                        Console.WriteLine($"BOT - vlrBTCAvPrimeiraVezBot > 0.001m = {vlrBTCAvPrimeiraVezBot}");
                                        status = Status.CompraFeita; // O BOT vai começar VENDENDO
                                        Console.WriteLine($"BOT - Começar VENDENDO!");
                                    }
                                }

                                primeiraVezBot = false;
                            }
                            catch (Exception ex)
                            {
                                throw new Exception($"primeiraVezBot", ex);
                            }
                        }
                        else
                        {
                            // º - obtem os dados das minha conta
                            AccountInfoRoot accountInfoPRINT = null;
                            using (var negociacoesService = new NegociacoesService(AppSettings))
                                accountInfoPRINT = await negociacoesService.GetAccountInfo();

                            if (accountInfoPRINT != null && accountInfoPRINT.response_data != null && accountInfoPRINT.response_data.balance != null &&
                                accountInfoPRINT.response_data.balance.brl != null && accountInfoPRINT.response_data.balance.btc != null &&
                                !string.IsNullOrWhiteSpace(accountInfoPRINT.response_data.balance.brl.available) &&
                                !string.IsNullOrWhiteSpace(accountInfoPRINT.response_data.balance.btc.available))
                            {
                                // Exibe o status da CONTA = BTC e REAL DISPONIVEL
                                Console.WriteLine($"{status} - REAL Disponivel: {accountInfoPRINT.response_data.balance.brl.available} ");
                                Console.WriteLine($"{status} - BTC Disponível: {accountInfoPRINT.response_data.balance.btc.available} ");
                            }

                            if (accountInfoPRINT != null)
                                accountInfoPRINT = null;
                        }

                        decimal vlrBTCAtual = 0m;
                        //decimal vlrAskBTC = 0m;
                        bool isSleepThread = true;

                        // 1º - Obtem o valor atual do BTC
                        var tickerroot = await dadosService.GetTicker();

                        if (tickerroot != null && tickerroot.ticker != null)
                        {
                            vlrBTCAtual = tickerroot.ticker.last;

                            // ***************************************************************** //

                            using (var context = new BotDBContext())
                            {
                                Ticker ticker;

                                try
                                {
                                    if (mediaMenorCount <= AppSettings.mediaMenor && mediaMaiorCount <= AppSettings.mediaMaior)
                                    {
                                        // - Grava o valor atual do BTC, o valor atual de compra "valor mais baixo no Orderbook" e a data atual *** //
                                        ticker = new Ticker
                                        {
                                            Data = DateTime.Now.ToString(), // new DateTimeOffset(DateTime.Now).UtcDateTime.ToString(),
                                            Valor = vlrBTCAtual,
                                            mediaMaior = AppSettings.mediaMaior,
                                            mediaMenor = AppSettings.mediaMenor,
                                            mediaValida = AppSettings.mediaValida
                                        };

                                        context.Tickers.Add(ticker);
                                        context.SaveChanges();
                                        // ************************************** //
                                    }
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception($"Salva vlrBTCAtual", ex);
                                }

                                // 4º - incrementa os contadores das medias
                                mediaMenorCount++;
                                mediaMaiorCount++;
                                mediaValidaCount++;

                                Console.WriteLine($"{status} - Ticker - Data: {DateTime.Now.ToString()}, Valor BTC Atual: {vlrBTCAtual} ");
                                
                                // 5º - Se os contadores das medias 9 e 21, estiverem com seus valores minímos "9 e 21"
                                if (mediaMenorCount >= AppSettings.mediaMenor && mediaMaiorCount >= AppSettings.mediaMaior) // Faz a compra ou venda
                                {
                                    decimal mediaMenorSoma;
                                    decimal mediaMaiorSoma;
                                    decimal? mediaValidaSoma = null;

                                    try
                                    {
                                        // 6º - calcula as medias 9 e 21 ************************************************ //
                                        var mediaMenor = context.Tickers.OrderByDescending(_ => _.Id).Take(AppSettings.mediaMenor - 1).ToList(); // 3
                                        var mediaMaior = context.Tickers.OrderByDescending(_ => _.Id).Take(AppSettings.mediaMaior - 1).ToList(); // 7

                                        // Quando a media 9 passar a 21 "para cima", compra "automatico"
                                        mediaMenorSoma = (mediaMenor.Sum(_ => _.Valor) + vlrBTCAtual) / AppSettings.mediaMenor;
                                        
                                        // Quando a media 9 passar a 21 "para baixo", vende "automatico"
                                        mediaMaiorSoma = (mediaMaior.Sum(_ => _.Valor) + vlrBTCAtual) / AppSettings.mediaMaior;

                                        if (mediaValidaCount >= AppSettings.mediaValida)
                                        {
                                            var mediaValida = context.Tickers.OrderByDescending(_ => _.Id).Take(AppSettings.mediaValida - 1).ToList(); // 9
                                            mediaValidaSoma = (mediaValida.Sum(_ => _.Valor) + vlrBTCAtual) / AppSettings.mediaValida;
                                        }
                                        // ****************************************************************************** //

                                        // - Grava o valor atual do BTC, o valor atual de compra "valor mais baixo no Orderbook" e a data atual *** //
                                        ticker = new Ticker
                                        {
                                            Data = DateTime.Now.ToString(), // new DateTimeOffset(DateTime.Now).UtcDateTime.ToString(),
                                            Valor = vlrBTCAtual,
                                            mediaMaior = AppSettings.mediaMaior,
                                            mediaMenor = AppSettings.mediaMenor,
                                            mediaValida = AppSettings.mediaValida,
                                            mediaMaiorSoma = mediaMaiorSoma,
                                            mediaMenorSoma = mediaMenorSoma,
                                            mediaValidaSoma = mediaValidaSoma
                                        };

                                        context.Tickers.Add(ticker);
                                        context.SaveChanges();
                                        // ************************************** //
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new Exception($"calcula Medias", ex);
                                    }

                                    var isCompraValida = false;
                                    var isVendaValida = false;

                                    try
                                    {
                                        // 7º - 'Compra' Ou 'Vende'
                                        var tickersComMedias = context.Tickers.OrderByDescending(_ => _.Id).Take(2).ToList();

                                        if (tickersComMedias.Where(_ => _.mediaMaiorSoma.HasValue && _.mediaMenorSoma.HasValue && !_.mediaValidaSoma.HasValue).Count() > 1)
                                        {
                                            Console.WriteLine($"{status} - _.mediaValidaSoma == NULL");

                                            // ** COMPRA ** //
                                            // A media ANTERIOR 'mediaMenorSoma' tem que ser MENOR que a media ANTERIOR 'mediaMaiorSoma'
                                            // EFETUA A COMPRA
                                            // quando a media ATUAL 'mediaMenorSoma' for  MAIOR que a media ATUAL 'mediaMaiorSoma'
                                            var isMediaAnteriorCompra = tickersComMedias[1].mediaMenorSoma < tickersComMedias[1].mediaMaiorSoma;
                                            var isMediaAtualCompra = tickersComMedias[0].mediaMenorSoma > tickersComMedias[0].mediaMaiorSoma;

                                            isCompraValida = isMediaAnteriorCompra && isMediaAtualCompra;
                                            Console.WriteLine($"{status} - isCompraValida '{isCompraValida}' = isMediaAnteriorCompra '{isMediaAnteriorCompra}' && isMediaAtualCompra '{isMediaAtualCompra}' ");
                                            // ************ //

                                            // ** VENDA ** //
                                            // A media ANTERIOR 'mediaMenorSoma' tem que ser MAIOR que a media ANTERIOR 'mediaMaiorSoma'
                                            // EFETUA A VENDA
                                            // quando a media ATUAL 'mediaMenorSoma' for  MENOR que a media ATUAL 'mediaMaiorSoma' 
                                            var isMediaAnteriorVenda = tickersComMedias[1].mediaMenorSoma > tickersComMedias[1].mediaMaiorSoma;
                                            var isMediaAtualVenda = tickersComMedias[0].mediaMenorSoma < tickersComMedias[0].mediaMaiorSoma;

                                            isVendaValida = isMediaAnteriorVenda && isMediaAtualVenda;
                                            Console.WriteLine($"{status} - isVendaValida '{isVendaValida}' = isMediaAnteriorVenda '{isMediaAnteriorVenda}' && isMediaAtualVenda '{isMediaAtualVenda}' ");
                                            // ************ //
                                        }
                                        else if (tickersComMedias.Where(_ => _.mediaMaiorSoma.HasValue && _.mediaMenorSoma.HasValue && _.mediaValidaSoma.HasValue).Count() > 1)
                                        {
                                            Console.WriteLine($"{status} - _.mediaValidaSoma != NULL");

                                            // ** COMPRA ** //
                                            // A media ANTERIOR 'mediaMenorSoma' tem que ser MENOR que a media ANTERIOR 'mediaMaiorSoma'
                                            // EFETUA A COMPRA
                                            // quando 'mediaMenorSoma' for MAIOR que a 'mediaMaiorSoma' e a 'mediaMaiorSoma' for maior que a 'mediaValidaSoma'
                                            var isMediaAnteriorCompra = !(tickersComMedias[1].mediaMenorSoma > tickersComMedias[1].mediaMaiorSoma &&
                                                                          tickersComMedias[1].mediaMaiorSoma > tickersComMedias[1].mediaValidaSoma);

                                            var isMediaAtualCompra = tickersComMedias[0].mediaMenorSoma > tickersComMedias[0].mediaMaiorSoma &&
                                                                     tickersComMedias[0].mediaMaiorSoma > tickersComMedias[0].mediaValidaSoma;

                                            isCompraValida = isMediaAnteriorCompra && isMediaAtualCompra;
                                            Console.WriteLine($"{status} - isCompraValida '{isCompraValida}' = isMediaAnteriorCompra '{isMediaAnteriorCompra}' && isMediaAtualCompra '{isMediaAtualCompra}' ");
                                            // ************ //

                                            // ** VENDA ** //
                                            // A media ANTERIOR 'mediaMenorSoma' tem que ser MAIOR que a media ANTERIOR 'mediaMaiorSoma'
                                            // EFETUA A VENDA
                                            // quando a media ATUAL 'mediaMenorSoma' for  MENOR que a media ATUAL 'mediaMaiorSoma' 
                                            var isMediaAnteriorVenda = !(tickersComMedias[1].mediaMenorSoma < tickersComMedias[1].mediaMaiorSoma &&
                                                                         tickersComMedias[1].mediaMaiorSoma < tickersComMedias[1].mediaValidaSoma);

                                            var isMediaAtualVenda = tickersComMedias[0].mediaMenorSoma < tickersComMedias[0].mediaMaiorSoma &&
                                                                    tickersComMedias[0].mediaMaiorSoma < tickersComMedias[0].mediaValidaSoma;

                                            isVendaValida = isMediaAnteriorVenda && isMediaAtualVenda;
                                            Console.WriteLine($"{status} - isVendaValida '{isVendaValida}' = isMediaAnteriorVenda '{isMediaAnteriorVenda}' && isMediaAtualVenda '{isMediaAtualVenda}' ");
                                            // ************ //
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new Exception($"Verifica = isCompraValida\\isVendaValida", ex);
                                    }

                                    if (isCompraValida && mediaMenorSoma > mediaMaiorSoma && 
                                        status != Status.NadaFeito && status != Status.CompraFeita) // compra "automatico"
                                    { 
                                        #region COMPRA
                                        if (!mediaValidaSoma.HasValue)
                                            Console.WriteLine($"{status} - COMPRA = mediaMenorSoma > mediaMaiorSoma | {mediaMenorSoma} > {mediaMaiorSoma}");
                                        else
                                            Console.WriteLine($"{status} - COMPRA = mediaMenorSoma > mediaMaiorSoma | {mediaMenorSoma} > {mediaMaiorSoma} && {mediaMaiorSoma} && > {mediaValidaSoma}");

                                        var tentaComprarNovamente = true;

                                        do
                                        {
                                            try
                                            {
                                                // 1º - Obtem o valor atual do BTC
                                                tickerroot = await dadosService.GetTicker();

                                                if (tickerroot != null)
                                                    vlrBTCAtual = tickerroot.ticker.last;

                                                // 7.1º - Obtem a ordem de venda "mais barata" na Orderbook, e tento comprar ela *** //
                                                var ask = await GetValorDeVenda();

                                                // 7.2º - obtem os dados das minha conta
                                                AccountInfoRoot accountInfo = null;
                                                using (var negociacoesService = new NegociacoesService(AppSettings))
                                                    accountInfo = await negociacoesService.GetAccountInfo();

                                                // 7.3º -  Valor em real disponivel para compra
                                                #region Converte o valor em real da conta para BTC
                                                decimal vlrRealBTCAvailableSite = 0;
                                                decimal vlrRealConta = 0;

                                                if (accountInfo != null && accountInfo.response_data != null && accountInfo.response_data.balance.brl != null &&
                                                    accountInfo.response_data.balance != null &&
                                                    !string.IsNullOrWhiteSpace(accountInfo.response_data.balance.brl.available))
                                                {
                                                    //if (decimal.TryParse(accountInfo.response_data.balance.brl.available.Replace(".", ","), out decimal vlrRealAvailable))
                                                    if (decimal.TryParse(accountInfo.response_data.balance.brl.available, out decimal vlrRealAvailable))
                                                    {
                                                        vlrRealConta = vlrRealAvailable;
                                                        vlrRealBTCAvailableSite = ConvertRealParaBTC(vlrRealAvailable, vlrBTCAtual);
                                                    }
                                                }
                                                #endregion

                                                // TODO: Se for uma order "executada parcialmente", pegar o resto dos Reais "Sobrou" e comprar novamente
                                                // TODO: Se a order foi executada "concluída", pegar o resto dos Reais "Sobrou" e comprar novamente 
                                                // 7.3º - Verifica se o valor disponivel e maior que o minimo valor de compra 
                                                if (vlrRealBTCAvailableSite < 0.001m)
                                                {
                                                    Console.WriteLine($"{status} - COMPRA = vlrRealBTCAvailableSite < 0.001m | {vlrRealBTCAvailableSite}");

                                                    tentaComprarNovamente = false;
                                                    status = Status.CompraFeita;

                                                    // // 7.3.1º - Verifica se o BTC da conta e maior que o minimo BTC 0.001m, e marca como compra feita
                                                    // if (accountInfo != null && accountInfo.response_data != null && accountInfo.response_data.balance.brl != null
                                                    //     && accountInfo.response_data.balance != null &&
                                                    //     !string.IsNullOrWhiteSpace(accountInfo.response_data.balance.btc.available))
                                                    // {
                                                    //     if (decimal.TryParse(accountInfo.response_data.balance.btc.available, out decimal vlrBTCConta) &&
                                                    //         vlrBTCConta > 0.001m)
                                                    //     {
                                                    //         status = Status.CompraFeita;
                                                    //     }
                                                    // }
                                                }
                                                else
                                                {
                                                    Console.WriteLine($"{status} - COMPRA = vlrRealBTCAvailableSite > 0.001m | {vlrRealBTCAvailableSite}");

                                                    // 7.2º - Efetua a compra de acordo com o limite, caso tenha um limite
                                                    API.Retorno.Negociacoes.v3.PlaceBuyOrder.OrderRoot buyOrderRoot = null;
                                                    
                                                    //if (decimal.TryParse(ask.quantity.Replace(".", ","), out decimal quantity) &&
                                                    //    decimal.TryParse(ask.limit_price.Replace(".", ","), out decimal limit_price))
                                                    if (decimal.TryParse(ask.quantity, out decimal quantity) &&
                                                        decimal.TryParse(ask.limit_price, out decimal limit_price))
                                                    {
                                                        try
                                                        {
                                                            using (var negociacoesService = new NegociacoesService(AppSettings))
                                                            {
                                                                // limit_price  = valor em REAL do vendedor
                                                                // quantity     = quantidade de BTC do vendedor
                                                                // vlrRealConta =  valor em REAL da minha conta disponivel
                                                                // Quantos BTC eu consigo comprar do vendedor com o meu saldo em REAL
                                                                var minhaQuantityEmBTC = (vlrRealConta * 1) / limit_price;

                                                                // Se a quantidade de REAL do vendedor e maior que da minha conta, compro usando TOTAL de reais disponivel da minha conta
                                                                if (quantity >= minhaQuantityEmBTC) 
                                                                {
                                                                    Console.WriteLine($"{status} - COMPRA quantity >= minhaQuantityEmBTC = minhaQuantityEmBTC ");
                                                                    buyOrderRoot = await negociacoesService.PlaceBuyOrder(ask, minhaQuantityEmBTC);
                                                                    Console.WriteLine($"{status} - COMPRA EFETUADA: QTD BTC's: {minhaQuantityEmBTC}");
                                                                }
                                                                else // Se a minha quantidade de REAL e maior que a do vendedor, compro usando o TOTAL de real do vendedor
                                                                {
                                                                    Console.WriteLine($"{status} - COMPRA quantity < minhaQuantityEmBTC = quantity ");
                                                                    buyOrderRoot = await negociacoesService.PlaceBuyOrder(ask, quantity);
                                                                    Console.WriteLine($"{status} - COMPRA EFETUADA: QTD BTC's: {quantity}");
                                                                }

                                                                if (accountInfo != null && accountInfo.response_data != null && accountInfo.response_data.balance != null &&
                                                                    accountInfo.response_data.balance.brl != null && accountInfo.response_data.balance.btc != null &&
                                                                    !string.IsNullOrWhiteSpace(accountInfo.response_data.balance.brl.available) &&
                                                                    !string.IsNullOrWhiteSpace(accountInfo.response_data.balance.btc.available))
                                                                {
                                                                    // Exibe o status da CONTA = BTC e REAL DISPONIVEL
                                                                    Console.WriteLine($"{status} - REAL Disponivel: {accountInfo.response_data.balance.brl.available} ");
                                                                    Console.WriteLine($"{status} - BTC Disponível: {accountInfo.response_data.balance.btc.available} ");
                                                                }
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            throw new Exception($"PlaceBuyOrder", ex);
                                                        }
                                                    }

                                                    // 7.3º - Verifica o status da COMPRA
                                                    if (buyOrderRoot != null && buyOrderRoot.response_data != null && buyOrderRoot.response_data.order != null)
                                                    {
                                                        // Se a ordem continua em aberta
                                                        if (buyOrderRoot.response_data.order.status != 4 && buyOrderRoot.response_data.order.status != 3)
                                                        {
                                                            // 2 : open: Ordem aberta
                                                            await VerificarOrder(buyOrderRoot.response_data.order.order_id);
                                                        }
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                throw new Exception($"COMPRA - while", ex);
                                            }
                                        } while (tentaComprarNovamente);
                                        #endregion
                                    }
                                    else if (isVendaValida && mediaMenorSoma < mediaMaiorSoma &&
                                        status != Status.NadaFeito && status != Status.VendaFeita) // vende "automatico"
                                    {
                                        #region VENDA
                                        if (!mediaValidaSoma.HasValue)
                                            Console.WriteLine($"{status} - VENDA = mediaMenorSoma < mediaMaiorSoma | {mediaMenorSoma} <>> {mediaMaiorSoma}");
                                        else
                                            Console.WriteLine($"{status} - VENDA = mediaMenorSoma < mediaMaiorSoma | {mediaMenorSoma} <>> {mediaMaiorSoma} && {mediaMaiorSoma} && < {mediaValidaSoma}");
                                        
                                        var tentaVenderNovamente = true;

                                        do
                                        {
                                            try
                                            {
                                                // 1º - Obtem o valor atual do BTC
                                                tickerroot = await dadosService.GetTicker();

                                                if (tickerroot != null)
                                                    vlrBTCAtual = tickerroot.ticker.last;

                                                // 8.1º - Obtem a ordem mais cara "o cara que quer pagar mais" na Orderbook, e tento vender para ele *** //
                                                var bid = await GetValorDeCompra();

                                                // 8.2º - obtem os dados das minha conta
                                                AccountInfoRoot accountInfo = null;
                                                using (var negociacoesService = new NegociacoesService(AppSettings))
                                                    accountInfo = await negociacoesService.GetAccountInfo();

                                                // 8.3º - Valor em BTC disponivel para compra
                                                decimal vlrBTCAvailableConta = 0;

                                                if (accountInfo != null && accountInfo.response_data != null && accountInfo.response_data.balance.btc != null &&
                                                    accountInfo.response_data.balance != null &&
                                                    !string.IsNullOrWhiteSpace(accountInfo.response_data.balance.btc.available))
                                                {
                                                    decimal.TryParse(accountInfo.response_data.balance.btc.available, out vlrBTCAvailableConta);
                                                }

                                                if (vlrBTCAvailableConta < 0.001m)
                                                {
                                                    Console.WriteLine($"{status} - VENDA = vlrBTCAvailableConta < 0.001m | {vlrBTCAvailableConta}");

                                                    tentaVenderNovamente = false;
                                                    status = Status.VendaFeita;

                                                    // // 7.3.1º - Verifica se o BTC da conta e maior que o minimo BTC 0.001m, e marca como compra feita
                                                    // if (accountInfo != null && accountInfo.response_data != null && accountInfo.response_data.balance != null &&
                                                    //     accountInfo.response_data.balance.brl != null &&
                                                    //     !string.IsNullOrWhiteSpace(accountInfo.response_data.balance.btc.available))
                                                    // {
                                                    //     if (decimal.TryParse(accountInfo.response_data.balance.brl.available, out decimal vlrREALConta))
                                                    //     {
                                                    //         var vlrBTCConta = ConvertRealParaBTC(vlrREALConta, vlrBTCAtual);

                                                    //         if (vlrBTCConta > 0.001m)
                                                    //             status = Status.VendaFeita;
                                                    //     }
                                                    // }
                                                }
                                                else
                                                {
                                                    Console.WriteLine($"{status} - VENDA = vlrBTCAvailableConta > 0.001m | {vlrBTCAvailableConta}");

                                                    API.Retorno.Negociacoes.v3.PlaceSellOrder.OrderRoot sellOrderRoot = null;
                                                    
                                                    //if (decimal.TryParse(bid.quantity.Replace(".", ","), out decimal bidQuantityBTC))
                                                    if (decimal.TryParse(bid.quantity, out decimal bidQuantityBTC))
                                                    {
                                                        try
                                                        {
                                                            using (var negociacoesService = new NegociacoesService(AppSettings))
                                                            {
                                                                // limit_price  = valor em REAL do comprador
                                                                // quantity     = quantidade de BTC do comprador
                                                                // vlrBTCAvailableConta =  valor de BTCs da minha conta disponivel
                                                                // Se a quantidade de BTC do COMPRADOR e maior que da minha conta, compro usando TOTAL de BTC disponivel da minha conta
                                                                if (bidQuantityBTC >= vlrBTCAvailableConta)
                                                                {
                                                                    Console.WriteLine($"{status} - VENDA bidQuantityBTC >= vlrBTCAvailableConta = vlrBTCAvailableConta ");
                                                                    sellOrderRoot = await negociacoesService.PlaceSellOrder(vlrBTCAvailableConta, bid.limit_price);
                                                                    Console.WriteLine($"{status} - VENDA EFETUADA: QTD BTC's: {vlrBTCAvailableConta}");
                                                                }
                                                                else // Se a minha quantidade de BTC e maior que a do COMPRADOR, compro usando o TOTAL de BTC do COMPRADOR
                                                                {
                                                                    Console.WriteLine($"{status} - VENDA bidQuantityBTC < vlrBTCAvailableConta = bidQuantityBTC ");
                                                                    sellOrderRoot = await negociacoesService.PlaceSellOrder(bidQuantityBTC, bid.limit_price);
                                                                    Console.WriteLine($"{status} - VENDA EFETUADA: QTD BTC's: {bidQuantityBTC}");
                                                                }

                                                                if (accountInfo != null && accountInfo.response_data != null && accountInfo.response_data.balance != null &&
                                                                    accountInfo.response_data.balance.brl != null && accountInfo.response_data.balance.btc != null &&
                                                                    !string.IsNullOrWhiteSpace(accountInfo.response_data.balance.brl.available) &&
                                                                    !string.IsNullOrWhiteSpace(accountInfo.response_data.balance.btc.available))
                                                                {
                                                                    // Exibe o status da CONTA = BTC e REAL DISPONIVEL
                                                                    Console.WriteLine($"{status} - REAL Disponivel: {accountInfo.response_data.balance.brl.available} ");
                                                                    Console.WriteLine($"{status} - BTC Disponível: {accountInfo.response_data.balance.btc.available} ");
                                                                }
                                                            }
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            throw new Exception($"PlaceSellOrder", ex);
                                                        }
                                                    }

                                                    if (sellOrderRoot != null && sellOrderRoot.response_data != null && sellOrderRoot.response_data.order != null)
                                                    {
                                                        // Se a ordem continua em aberta
                                                        if (sellOrderRoot.response_data.order.status != 4 && sellOrderRoot.response_data.order.status != 3)
                                                        {
                                                            // 2 : open: Ordem aberta
                                                            await VerificarOrder(sellOrderRoot.response_data.order.order_id);
                                                        }
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                throw new Exception($"VENDA - while", ex);
                                            }
                                        } while (tentaVenderNovamente);
                                        #endregion
                                    }
                                }
                            }

                            // 8º - Faz o bot esperar X tempo "valor configurado na appSettings.json"
                            if (isSleepThread)
                                Thread.Sleep(TimeSpan.FromMinutes(AppSettings.SleepFromMinutes));
                            else
                            {
                                Thread.Sleep(TimeSpan.FromSeconds(AppSettings.SleepFromMinutes));
                                isSleepThread = true;
                            }
                        }
                    } while (true);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            Console.ReadKey();
        }

        private static async Task VerificarOrder(int order_id)
        {
            Console.WriteLine($"VerificarOrder - order_id: {order_id}");
            Thread.Sleep(TimeSpan.FromSeconds(AppSettings.OrdemSleepFromSeconds));

            var ordemCancelada = false;
            var ordemEmProcessamentoCount = 0; // Número de tentativas de execução da orderm
            var ordemEmProcessamentoCountLimite = AppSettings.ordemEmProcessamentoCountLimite; //Número com o limite maximo de tentativas, caso seje igual "cancelar a ordem"

            try
            {
                do // Verifica o status da ordem e cancela a mesma de acordo com o limite
                {
                    if (ordemEmProcessamentoCount <= ordemEmProcessamentoCountLimite) // Verificar status da ordem
                    {
                        if (order_id != 0)
                        {
                            try
                            {
                                using (var negociacoesService = new NegociacoesService(AppSettings))
                                {
                                    var order = await negociacoesService.GetOrder(order_id);

                                    // 2 : open : Ordem aberta, disponível no livro de negociações. Estado intermediário.
                                    // 3 : canceled: Ordem cancelada, executada parcialmente ou sem execuções. Estado final.
                                    // 4 : filled: Ordem concluída, executada em sua totalidade.Estado final.

                                    // has_fills: Indica se a ordem tem uma ou mais execuções. Auxilia na identificação de ordens parcilamente executadas.
                                    // false : Sem execuções.
                                    // true : Com uma ou mais execuções.

                                    // Verifica se a ordem foi "concluida" ou "concluida parcialmente"
                                    if (order != null && order.response_data != null && order.response_data.order != null &&
                                        ((order.response_data.order.status == 3 && order.response_data.order.has_fills) || // 3 : canceled: executada parcialmente
                                        order.response_data.order.status == 4)) // Se a ordem e cancelada ou concluída
                                    {
                                        ordemCancelada = true;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                throw new Exception($"GetOrder - order_id: {order_id}", ex);
                            }

                            ordemEmProcessamentoCount++;
                        }
                    }
                    else
                    {
                        if (order_id != 0) // Cancelar a ordem
                        {
                            try
                            {
                                using (var negociacoesService = new NegociacoesService(AppSettings))
                                {
                                    var order = await negociacoesService.CancelOrder(order_id);
                                    
                                    // 3 : canceled : Ordem cancelada, executada parcialmente ou sem execuções. Estado final.
                                    if (order != null && order.response_data != null && order.response_data.order != null &&
                                        order.response_data.order.status == 3) 
                                    {
                                        ordemCancelada = true;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                throw new Exception($"CancelOrder - order_id: {order_id}", ex);
                            }

                            ordemEmProcessamentoCount = 0;
                        }
                    }

                    if (ordemCancelada == false)
                        Thread.Sleep(TimeSpan.FromSeconds(AppSettings.OrdemSleepFromSeconds));
                } while (!ordemCancelada);
            }
            catch (Exception ex)
            {
                throw new Exception($"VerificarOrder - while - order_id: {order_id}", ex);
            }
        }
        
        /// <summary>
        /// bids: Lista de ofertas de compras, ordenadas do maior para o menor preço.
        /// Tipo: Array
        ///     [0]: Preço unitário da oferta de compra.
        ///     Tipo: Decimal
        ///
        ///     [1]: Quantidade da oferta de compra.
        ///     Tipo: Decimal
        /// </summary>
        private static async Task<API.Retorno.Negociacoes.v3.ListOrderbook.Bid> GetValorDeCompra()
        {
            API.Retorno.Negociacoes.v3.ListOrderbook.Bid bid = null;

            try
            {
                using (var negociacoesService = new NegociacoesService(AppSettings))
                {
                    var listOrderbook = await negociacoesService.ListOrderbook();

                    var bids = new List<API.Retorno.Negociacoes.v3.ListOrderbook.Bid>();
                    if (listOrderbook != null && listOrderbook.response_data != null && listOrderbook.response_data.orderbook != null &&
                        listOrderbook.response_data.orderbook.bids != null && listOrderbook.response_data.orderbook.bids.Any())
                    {
                        bids = listOrderbook.response_data.orderbook.bids.OrderBy(_ => _.limit_price).ToList();
                    }

                    if (bids != null && bids.Any())
                        bid = bids.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"GetValorDeCompra", ex);
            }

            return bid;
        }

        /// <summary>
        /// asks: Lista de ofertas de venda, ordenadas do menor para o maior preço.
        /// Tipo: Array
        ///     [0]: Preço unitário da oferta de venda.
        ///     Tipo: Decimal
        ///
        ///     [1]: Quantidade da oferta de venda.
        ///     Tipo: Decimal
        /// </summary>
        private static async Task<API.Retorno.Negociacoes.v3.ListOrderbook.Ask> GetValorDeVenda()
        {
            API.Retorno.Negociacoes.v3.ListOrderbook.Ask ask = null;

            try
            {
                using (var negociacoesService = new NegociacoesService(AppSettings))
                {
                    var listOrderbook = await negociacoesService.ListOrderbook();

                    var asks = new List<API.Retorno.Negociacoes.v3.ListOrderbook.Ask>();
                    if (listOrderbook != null && listOrderbook.response_data != null && listOrderbook.response_data.orderbook != null &&
                        listOrderbook.response_data.orderbook.asks != null && listOrderbook.response_data.orderbook.asks.Any())
                    {
                        asks = listOrderbook.response_data.orderbook.asks.OrderByDescending(_ => _.limit_price).ToList();
                    }

                    if (asks != null && asks.Any())
                        ask = asks.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"GetValorDeVenda", ex);
            }

            return ask;
        }

        /// <summary>
        /// vlrRealAvailable = valor em real
        /// vlrBTCAtual = valor em BTC
        /// </summary>
        private static decimal ConvertRealParaBTC(decimal vlrRealAvailable, decimal vlrBTCAtual)
        {
            // decimal vlr = decimal.Parse(vlrBTC.Replace(".", ","));

            return vlrRealAvailable / vlrBTCAtual;
        }
    }
}