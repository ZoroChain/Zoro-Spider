using System;
using System.Net;
using Newtonsoft.Json.Linq;
using Zoro.Ledger;
using Akka.Actor;

namespace Zoro.Spider
{
    public class Spider : IDisposable
    {
        private WebClient wc = new WebClient();

        public Spider()
        {
        }

        public void Dispose()
        {
        }

        public void Start()
        {

        }

        private void SaveAppchain(string hashString)
        {
            var url = $"{Helper.url}?jsonrpc=2.0&id=1&method=getappchainstate&params=[{hashString}]";
            var response = wc.DownloadString(url);
            var json = JObject.Parse(response);
            var result = json["result"];

            Console.WriteLine("Save AppChainState:" + result.ToString());
        }
    }
}
