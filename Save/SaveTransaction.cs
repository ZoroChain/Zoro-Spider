using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Zoro.Spider
{
    class SaveTransaction : SaveBase
    {
        //private SaveUTXO utxo;
        private SaveAsset asset;
        private SaveNotify notify;
        private SaveTxScriptMethod txScriptMethod;

        public SaveTransaction(UInt160 chainHash)
            : base(chainHash)
        {
            InitDataTable(TableType.Transaction);

            //utxo = new SaveUTXO(chainHash);
            asset = new SaveAsset(chainHash);
            notify = new SaveNotify(chainHash);
            txScriptMethod = new SaveTxScriptMethod(chainHash);
        }

        public override bool CreateTable(string name)
        {
            MysqlConn.CreateTable(TableType.Transaction, name);
            return true;
        }

        public void Save(JToken jObject, uint blockHeight, uint blockTime)
        {
            List<string> slist = new List<string>();
            slist.Add(jObject["txid"].ToString());
            slist.Add(jObject["size"].ToString());
            slist.Add(jObject["type"].ToString());
            slist.Add(jObject["version"].ToString());
            slist.Add(jObject["attributes"].ToString());
            slist.Add(jObject["sys_fee"].ToString());
            slist.Add(jObject["scripts"].ToString());
            slist.Add(jObject["nonce"].ToString());
            slist.Add(blockHeight.ToString());
            slist.Add(jObject["gas_limit"]?.ToString());
            slist.Add(jObject["gas_price"]?.ToString());
            slist.Add(ZoroHelper.GetAddressFromScriptHash(UInt160.Parse(jObject["account"].ToString())));

            if (jObject["script"] != null)
                txScriptMethod.Save(jObject["script"].ToString(), blockHeight, jObject["txid"].ToString());

            Dictionary<string, string> deleteWhere = new Dictionary<string, string>();
            deleteWhere.Add("txid", jObject["txid"].ToString());
            deleteWhere.Add("blockheight", blockHeight.ToString());

            MysqlConn.ExecuteDataInsertWithCheck(DataTableName, slist, deleteWhere);

            Program.Log($"SaveTransaction {ChainHash} {blockHeight} {jObject["txid"].ToString()}", Program.LogLevel.Info, ChainHash.ToString());

            if (jObject["type"].ToString() == "InvocationTransaction")
            {
                notify.Save(jObject, blockHeight, blockTime, jObject["script"].ToString());
                //Thread.Sleep(20);
            }
        }
        
    }
}
