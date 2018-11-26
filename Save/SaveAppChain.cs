using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data;

namespace Zoro.Spider
{
    class SaveAppChain : SaveBase
    {
        private MysqlConn conn;
        public SaveAppChain(MysqlConn conn)
            : base(null)
        {
            InitDataTable(TableType.Appchainstate);
            this.conn = conn;
        }

        public override bool CreateTable(string name)
        {
            conn.CreateTable(TableType.Appchainstate, name);
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

            conn.SaveAndUpdataAppChainState(DataTableName, slist);

            Program.Log($"SaveAppChain {jObject["hash"]} {jObject["name"]}", Program.LogLevel.Info);
            Program.Log(slist.ToString(), Program.LogLevel.Debug);
        }
    }
}
