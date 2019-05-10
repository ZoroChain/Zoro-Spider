using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
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
                wc.Proxy = null;
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
                Thread.Sleep(3000);
                return GetBlockCount();
            }

            return 0;
        }

        private uint GetBlock(uint height)
        {
            try
            {
                WebClient wc = new WebClient();
                wc.Proxy = null;
                var getblockUrl = $"{Settings.Default.RpcUrl}/?jsonrpc=2.0&id=1&method=getblock&params=['{chainHash}',{height},1]";
                var info =  wc.DownloadString(getblockUrl);
                var json = JObject.Parse(info);
                JToken result = json["result"];

                if (result != null)
                {
                    MySqlConnection myConnection = new MySqlConnection(Settings.Default.MysqlConfig);
                    myConnection.Open();

                    MySqlCommand myCommand = myConnection.CreateCommand();
                    MySqlTransaction myTrans = myConnection.BeginTransaction();

                    myCommand.Connection = myConnection;
                    myCommand.Transaction = myTrans;

                    try
                    {
                        string sql = block.GetBlockSqlText(wc, result, height);

                        myCommand.CommandText = sql;
                        myCommand.ExecuteNonQuery();

                        myCommand.CommandText = listHeight.GetUpdateHeightSql(chainHash.ToString(), height);
                        myCommand.ExecuteNonQuery();

                        myTrans.Commit();

                        //Program.Log($"SaveBlock {chainHash} height:{height}", Program.LogLevel.Warning, chainHash.ToString());

                        height = height + 1;
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            myTrans.Rollback();
                        }
                        catch (Exception ex)
                        {
                            if (myTrans.Connection != null)
                            {
                                Console.WriteLine("An exception of type " + ex.GetType() + " when roll back the transaction.");
                                throw ex;
                            }
                        }
                        Console.WriteLine("An exception of type " + e.GetType() + " was encountered while inserting the data.");
                        throw e;
                    }
                    finally
                    {
                        myConnection.Close();
                    }

                }
            }
            catch (Exception e)
            {
                Program.Log($"error occured when call getblock {height} {chainHash}, reason:{e.Message}", Program.LogLevel.Error);
                Thread.Sleep(3000);
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
