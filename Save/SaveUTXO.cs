using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Zoro.Spider
{
    class SaveUTXO : SaveBase
    {
        public SaveUTXO(UInt160 chainHash)
            : base(chainHash)
        {
            InitDataTable(TableType.UTXO);
        }

        public override bool CreateTable(string name)
        {
            MysqlConn.CreateTable(TableType.UTXO, name);
            return true;
        }

        public void Save(JToken jObject, uint blockHeight)
        {
            foreach (JObject vout in jObject["vout"])
            {
                JObject result = new JObject();
                result["addr"] = vout["address"];
                result["txid"] = jObject["txid"];
                result["n"] = vout["n"];
                result["asset"] = vout["asset"];
                result["value"] = vout["value"];
                result["createHeight"] = blockHeight;
                result["used"] = 0;
                result["useHeight"] = 0;
                result["claimed"] = "";

                List<string> slist = new List<string>();
                slist.Add(result["addr"].ToString());
                slist.Add(result["txid"].ToString());
                slist.Add(result["n"].ToString());
                slist.Add(result["asset"].ToString());
                slist.Add(result["value"].ToString());
                slist.Add(result["createHeight"].ToString());
                slist.Add(result["used"].ToString());
                slist.Add(result["useHeight"].ToString());
                slist.Add(result["claimed"].ToString());
                MysqlConn.ExecuteDataInsert(DataTableName, slist);

                //var utxoPath = "utxo" + Path.DirectorySeparatorChar + result["txid"] + "_" + result["n"] + "_" + result["addr"] + ".txt";
                //File.Delete(utxoPath);
                //File.WriteAllText(utxoPath, result.ToString(), Encoding.UTF8);
            }
            foreach (JObject vin in jObject["vin"])
            {
                ChangeUTXO(vin["txid"].ToString(), vin["vout"].ToString(), blockHeight);
            }
        }

        public void ChangeUTXO(string txid, string voutNum, uint blockHeight)
        {
            Dictionary<string, string> dirs = new Dictionary<string, string>();
            dirs.Add("used", "1");
            dirs.Add("useHeight", blockHeight.ToString());
            Dictionary<string, string> where = new Dictionary<string, string>();
            where.Add("txid", txid);
            where.Add("n", voutNum);
            MysqlConn.Update(DataTableName, dirs, where);

            //JObject result = JObject.Parse(File.ReadAllText(path, Encoding.UTF8));
            //result["used"] = 1;
            //result["useHeight"] = Helper.blockHeight;
            //File.WriteAllText(path, result.ToString(), Encoding.UTF8);
        }
    }
}
