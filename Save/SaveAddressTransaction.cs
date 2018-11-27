using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data;

namespace Zoro.Spider
{
    class SaveAddressTransaction : SaveBase
    {
        public SaveAddressTransaction(UInt160 chainHash)
            : base(chainHash)
        {
            InitDataTable(TableType.Address_tx);
        }

        public override bool CreateTable(string name)
        {
            MysqlConn.CreateTable(TableType.Address_tx, name);
            return true;
        }

        public void Save(JToken jObject, string path, uint blockHeight, uint blockTime)
        {
            //JObject result = new JObject();
            //result["txid"] = jObject["txid"];
            //result["blockindex"] = Helper.blockHeight;
            //result["blocktime"] = Helper.blockTime;

            foreach (JObject vout in jObject["vout"])
            {
                List<string> slist = new List<string>();
                slist.Add(vout["address"].ToString());
                slist.Add(jObject["txid"].ToString());
                slist.Add(blockHeight.ToString());
                slist.Add(blockTime.ToString());

                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary.Add("txid", jObject["txid"].ToString());
                dictionary.Add("blockindex", blockHeight.ToString());
                bool exist = MysqlConn.CheckExist(DataTableName, dictionary);
                if (!exist)
                {
                    MysqlConn.ExecuteDataInsert(DataTableName, slist);
                }
            }

            //File.Delete(path);
            //File.WriteAllText(path, result.ToString(), Encoding.UTF8);
        }
    }
}
