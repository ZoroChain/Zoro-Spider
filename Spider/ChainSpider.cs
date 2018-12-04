using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Zoro.Spider
{
    class ChainSpider : IDisposable
    {
        private Task task;
        private SaveBlock block;
        private SaveChainListHeight listHeight;

        private UInt160 chainHash;
        private uint currentHeight = 0;

        public static uint checkHeight = 0;

        public ChainSpider(UInt160 chainHash)
        {
            this.chainHash = chainHash;
            block = new SaveBlock(chainHash);
            listHeight = new SaveChainListHeight(chainHash);
        }

        public void Start(int startHeight)
        {
            this.currentHeight = startHeight >= 0 ? (uint)startHeight : listHeight.getHeight(chainHash.ToString());
            checkHeight = currentHeight;

            Program.Log($"Starting chain spider {chainHash} {currentHeight}", Program.LogLevel.Warning);

            task = Task.Factory.StartNew(() =>
            {
                Process();
            });
        }

        public void Dispose()
        {
            task.Dispose();
        }

        private uint GetBlockCount()
        {
            try
            {
                WebClient wc = new WebClient();
                var getcountUrl = $"{Settings.Default.RpcUrl}/?jsonrpc=2.0&id=1&method=getblockcount&params=['{chainHash}']";
                var info = wc.DownloadString(getcountUrl);
                var json = JObject.Parse(info);
                JToken result = json["result"];

                if (result != null)
                {
                    uint height = uint.Parse(result.ToString());
                    return height;
                }
            }
            catch (Exception e)
            {
                Program.Log($"error occured when call getblockcount {chainHash}, reason:{e.Message}", Program.LogLevel.Error);
                return GetBlockCount();
            }

            return 0;
        }

        private uint GetBlock(uint height)
        {
            try
            {
                WebClient wc = new WebClient();
                var getblockUrl = $"{Settings.Default.RpcUrl}/?jsonrpc=2.0&id=1&method=getblock&params=['{chainHash}',{height},1]";
                var info =  wc.DownloadString(getblockUrl);
                var json = JObject.Parse(info);
                JToken result = json["result"];

                if (result != null)
                {
                    block.Save(wc, result, height);
                    //每获取一个块做一次高度记录，方便下次启动时做开始高度
                    listHeight.Save(chainHash.ToString(), height.ToString());
                    return height + 1;
                }
            }
            catch (Exception e)
            {
                Program.Log($"error occured when call getblock {height} {chainHash}, reason:{e.Message}", Program.LogLevel.Error);
                //throw e;
                return GetBlock(height);
            }

            return height;
        }

        private void Process()
        {
            while (true)
            {
                uint blockCount = GetBlockCount();

                while (currentHeight < blockCount)
                {
                    currentHeight = GetBlock(currentHeight);
                }

                Thread.Sleep(1000);
            }
        }
    }
}
