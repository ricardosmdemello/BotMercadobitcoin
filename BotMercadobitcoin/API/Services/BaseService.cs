using BotMercadobitcoin.Interfaces.Dados.v3;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace BotMercadobitcoin.API.Services
{
    public abstract class BaseService : IBaseService
    {
        protected RestClient Client { get; set; }
        protected Coin Coin { get; set; }

        public BaseService(Coin coin = Coin.BTC)
        {
            this.Client = new RestClient("https://www.mercadobitcoin.net");
            this.Coin = coin;
        }

        protected RestRequest GetRequest(string resource, Method method)
        {
            return new RestRequest(resource, method);
        }

        /// <summary>
        /// https://github.com/wesdrasalves/MercadoBitcoin_API
        /// </summary>
        /// <param name="url"></param>
        /// <param name="tapi_segredo"></param>
        /// <returns></returns>
        protected string CriaTapiMac(string url, string tapi_segredo)
        {
            var tapi_mac = string.Empty;

            try
            {
                var encoding = new ASCIIEncoding();

                var sha512 = new HMACSHA512(encoding.GetBytes(tapi_segredo));
                byte[]  _signByte = sha512.ComputeHash(encoding.GetBytes(url));

                var _sBuilder = new StringBuilder();
                for (int _i = 0; _i < _signByte.Length; _i++)
                    _sBuilder.Append(_signByte[_i].ToString("x2"));

                tapi_mac = _sBuilder.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }

            return tapi_mac;
        }

        public virtual void Dispose()
        {
            // Dispose(true): realizará as operações de liberação de recursos gerenciados
            this.Dispose(true);
        }

        public void Dispose(bool disposing = true)
        {
            // se for true vai liberar os recursos gerenciados e false, os não gerenciados. 
            if (disposing)
            {
                // libera recursos não gerenciados pela CLR, que não estão num 'bloco using ou try-finally '
                if (this.Client != null)
                    this.Client = null;
            }

            GC.SuppressFinalize(this);
        }

        ~BaseService()
        {
            if (this.Client != null)
                this.Client = null;
        }
    }
}
