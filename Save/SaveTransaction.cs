using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Data;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace Zoro.Spider
{
    class SaveTransaction : SaveBase
    {
        private SaveUTXO utxo;
        private SaveAsset asset;
        private SaveNotify notify;

        public SaveTransaction(UInt160 chainHash)
            : base(chainHash)
        {
            InitDataTable(TableType.Transaction);

            utxo = new SaveUTXO(chainHash);
            asset = new SaveAsset(chainHash);
            notify = new SaveNotify(chainHash);
        }

        public override bool CreateTable(string name)
        {
            MysqlConn.CreateTable(TableType.Transaction, name);
            return true;
        }

        public void Save(JToken jObject, uint blockHeight, uint blockTime)
        {
            JObject result = new JObject();
            result["txid"] = jObject["txid"];
            result["size"] = jObject["size"];
            result["type"] = jObject["type"];
            result["version"] = jObject["version"];
            result["attributes"] = jObject["attributes"];
            result["vin"] = jObject["vin"];
            result["vout"] = jObject["vout"];
            result["sys_fee"] = jObject["sys_fee"];
            result["scripts"] = jObject["scripts"];
            result["nonce"] = jObject["nonce"];
            result["blockindex"] = blockHeight;

            List<string> slist = new List<string>();
            slist.Add(result["txid"].ToString());
            slist.Add(result["size"].ToString());
            slist.Add(result["type"].ToString());
            slist.Add(result["version"].ToString());
            slist.Add(result["attributes"].ToString());
            slist.Add(result["sys_fee"].ToString());
            slist.Add(result["scripts"].ToString());
            slist.Add(result["nonce"].ToString());
            slist.Add(blockHeight.ToString());

            //Dictionary<string, string> dictionary = new Dictionary<string, string>();
            //dictionary.Add("txid", jObject["txid"].ToString());
            //dictionary.Add("blockheight", blockHeight.ToString());
            //bool exist = MysqlConn.CheckExist(DataTableName, dictionary);
            //if (!exist)
            if (ChainSpider.checkHeight == int.Parse(blockHeight.ToString()))
            {
                Dictionary<string, string> where = new Dictionary<string, string>();
                where.Add("txid", jObject["txid"].ToString());
                where.Add("blockheight", blockHeight.ToString());
                MysqlConn.Delete(DataTableName, where);
            }
            {
                MysqlConn.ExecuteDataInsert(DataTableName, slist);
            }

            Program.Log($"SaveTransaction {ChainHash} {blockHeight}", Program.LogLevel.Info, ChainHash.ToString());           

            utxo.Save(result, blockHeight);                      

            if (result["type"].ToString() == "InvocationTransaction")
            {
                notify.Save(jObject, blockHeight, blockTime, jObject["script"].ToString());
                Thread.Sleep(50);
            }
        }
    }
}
