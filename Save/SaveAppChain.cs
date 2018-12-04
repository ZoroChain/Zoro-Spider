using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Zoro.Spider
{
    class SaveAppChain : SaveBase
    {
        public SaveAppChain()
            : base(null)
        {
            InitDataTable(TableType.Appchainstate);
        }

        public override bool CreateTable(string name)
        {
            MysqlConn.CreateTable(TableType.Appchainstate, name);
            return true;
        }

        public void Save(JToken jObject)
        {
			JObject hashstateresult = new JObject();

			hashstateresult["result"] = jObject["result"];

			List<string> slist = new List<string>();

			slist.Add(jObject["version"].ToString());
			slist.Add(jObject["hash"].ToString());
			slist.Add(jObject["name"].ToString());
			slist.Add(jObject["owner"].ToString());
			slist.Add(jObject["timestamp"].ToString());
			slist.Add(jObject["seedlist"].ToString());
			slist.Add(jObject["validators"].ToString());
            
            MysqlConn.SaveAndUpdataAppChainState(DataTableName, slist);

            Program.Log($"SaveAppChain {jObject["hash"]} {jObject["name"]}", Program.LogLevel.Info);
        }
    }
}
