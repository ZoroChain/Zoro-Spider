using System.Net;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Zoro.Spider
{
    class SaveTransaction : SaveBase
    {
        private SaveUTXO utxo;
        private SaveAddress address;
        private SaveAddressTransaction addressTrans;
        private SaveAsset asset;
        private SaveNotify notify;

        public SaveTransaction(WebClient wc, UInt160 chainHash)
            : base(chainHash)
        {
            InitDataTable(TableType.Transaction);

            utxo = new SaveUTXO(chainHash);
            address = new SaveAddress(chainHash);
            addressTrans = new SaveAddressTransaction(chainHash);
            asset = new SaveAsset(chainHash);
            notify = new SaveNotify(wc, chainHash);
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
            result["net_fee"] = jObject["net_fee"];
            result["scripts"] = jObject["scripts"];
            result["nonce"] = jObject["nonce"];
            result["blockindex"] = blockHeight;

            List<string> slist = new List<string>();
            slist.Add(result["txid"].ToString());
            slist.Add(result["size"].ToString());
            slist.Add(result["type"].ToString());
            slist.Add(result["version"].ToString());
            slist.Add(result["attributes"].ToString());
            slist.Add(result["vin"].ToString());
            slist.Add(result["vout"].ToString());
            slist.Add(result["sys_fee"].ToString());
            slist.Add(result["net_fee"].ToString());
            slist.Add(result["scripts"].ToString());
            slist.Add(result["nonce"].ToString());
            slist.Add(blockHeight.ToString());
            MysqlConn.ExecuteDataInsert(DataTableName, slist);

            Program.Log($"SaveTransaction {ChainHash} {blockHeight}", Program.LogLevel.Info);
            Program.Log(result.ToString(), Program.LogLevel.Debug);

            address.Save(result["vout"], blockHeight, blockTime);

            utxo.Save(result, blockHeight);

            var addressTransactionPath = "addressTransaction" + Path.DirectorySeparatorChar + result["txid"] + ".txt";
            addressTrans.Save(result, addressTransactionPath, blockHeight, blockTime);

            //if (result["type"].ToString() == "RegisterTransaction")
            //{
            //    var assetPath = "asset" + Path.DirectorySeparatorChar + result["txid"] + ".txt";
            //    asset.Save(jObject, assetPath);
            //}
            if (result["type"].ToString() == "InvocationTransaction")
            {
                notify.Save(jObject, blockHeight);
            }
        }
    }
}
