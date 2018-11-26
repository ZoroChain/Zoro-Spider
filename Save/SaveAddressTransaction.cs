using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data;

namespace Zoro.Spider
{
    class SaveAddressTransaction : SaveBase
    {
        private MysqlConn conn = null;
        public SaveAddressTransaction(MysqlConn conn, UInt160 chainHash)
            : base(chainHash)
        {
            InitDataTable(TableType.Address_tx);
            this.conn = conn;
        }

        public override bool CreateTable(string name)
        {
            conn.CreateTable(TableType.Address_tx, name);
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
                DataSet ds = conn.ExecuteDataSet(DataTableName, dictionary);
                if (ds.Tables[0].Rows.Count == 0)
                {
                    conn.ExecuteDataInsert(DataTableName, slist);
                }
            }

            //File.Delete(path);
            //File.WriteAllText(path, result.ToString(), Encoding.UTF8);
        }
    }
}
