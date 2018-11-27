using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data;

namespace Zoro.Spider
{
    class SaveAsset : SaveBase
    {
        public SaveAsset(UInt160 chainHash)
            : base(chainHash)
        {
            InitDataTable("asset");
        }

        public override bool CreateTable(string name)
        {
            return true;
        }

        public void Save(JToken jObject, string path)
        {
            JObject result = new JObject();
            result["version"] = jObject["version"];
            result["id"] = jObject["txid"];
            result["type"] = jObject["asset"]["type"];
            result["name"] = jObject["asset"]["name"];
            result["amount"] = jObject["asset"]["amount"];
            result["available"] = 1;
            result["precision"] = jObject["asset"]["precision"];
            result["owner"] = jObject["asset"]["owner"];
            result["admin"] = jObject["asset"]["admin"];
            result["issuer"] = 1;
            result["expiration"] = 0;
            result["frozen"] = 0;

            List<string> slist = new List<string>();
            slist.Add(result["version"].ToString());
            slist.Add(result["id"].ToString());
            slist.Add(result["type"].ToString());
            slist.Add(result["name"].ToString());
            slist.Add(result["amount"].ToString());
            slist.Add(result["available"].ToString());
            slist.Add(result["precision"].ToString());
            slist.Add(result["owner"].ToString());
            slist.Add(result["admin"].ToString());
            slist.Add(result["issuer"].ToString());
            slist.Add(result["expiration"].ToString());
            slist.Add(result["frozen"].ToString());

            //Dictionary<string, string> dictionary = new Dictionary<string, string>();
            //dictionary.Add("id", result["id"].ToString());
            //bool exist = MysqlConn.CheckExist(DataTableName, dictionary);
            //if (!exist)
            {
                MysqlConn.ExecuteDataInsert(DataTableName, slist);
            }

            Program.Log($"SaveAsset {ChainHash} {result["name"]}", Program.LogLevel.Info);
            Program.Log(slist.ToString(), Program.LogLevel.Debug);

            //File.Delete(path);
            //File.WriteAllText(path, result.ToString(), Encoding.UTF8);
        }
    }
}
