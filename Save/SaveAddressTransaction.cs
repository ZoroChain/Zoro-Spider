using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Zoro.Spider
{
    class SaveAddressTransaction : SaveBase
    {
        public SaveAddressTransaction(UInt160 chainHash)
            : base(chainHash)
        {
            InitDataTable("address_tx");
        }

        public override bool CreateTable(string name)
        {
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

                MysqlConn.ExecuteDataInsert(DataTableName, slist);
            }

            //File.Delete(path);
            //File.WriteAllText(path, result.ToString(), Encoding.UTF8);
        }
    }
}
