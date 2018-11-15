using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Zoro.Spider
{
    class SaveNEP5Transfer : SaveBase
    {
        public SaveNEP5Transfer(UInt160 chainHash)
            : base(chainHash)
        {
            InitDataTable(TableType.NEP5Transfer);
        }

        public override bool CreateTable(string name)
        {
            MysqlConn.CreateTable(TableType.NEP5Transfer, name);
            return true;
        }

        public void Save(JToken jToken)
        {
            List<string> slist = new List<string>();
            slist.Add(jToken["blockindex"].ToString());
            slist.Add(jToken["txid"].ToString());
            slist.Add(jToken["n"].ToString());
            slist.Add(jToken["asset"].ToString());
            slist.Add(jToken["from"].ToString());
            slist.Add(jToken["to"].ToString());
            slist.Add(jToken["value"].ToString());
            MysqlConn.ExecuteDataInsert(DataTableName, slist);
        }
    }
}
