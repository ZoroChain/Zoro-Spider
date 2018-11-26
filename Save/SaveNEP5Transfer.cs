using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data;

namespace Zoro.Spider
{
    class SaveNEP5Transfer : SaveBase
    {
        private MysqlConn conn;
        public SaveNEP5Transfer(MysqlConn conn, UInt160 chainHash)
            : base(chainHash)
        {
            InitDataTable(TableType.NEP5Transfer);
            this.conn = conn;
        }

        public override bool CreateTable(string name)
        {
            conn.CreateTable(TableType.NEP5Transfer, name);
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

            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("txid", jToken["txid"].ToString());
            dictionary.Add("blockindex", jToken["blockindex"].ToString());
            DataSet ds = conn.ExecuteDataSet(DataTableName, dictionary);
            if (ds.Tables[0].Rows.Count == 0)
            {
                conn.ExecuteDataInsert(DataTableName, slist);
            }

            Program.Log($"SaveNEP5Transfer {ChainHash} {jToken["blockindex"]} {jToken["txid"]}", Program.LogLevel.Info);
            Program.Log(jToken.ToString(), Program.LogLevel.Debug);
        }
    }
}
