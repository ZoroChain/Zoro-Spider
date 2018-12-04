using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data;

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

            //Dictionary<string, string> dictionary = new Dictionary<string, string>();
            //dictionary.Add("txid", jToken["txid"].ToString());
            //dictionary.Add("blockindex", jToken["blockindex"].ToString());
            //bool exist = MysqlConn.CheckExist(DataTableName, dictionary);
            //if (!exist)
            if (ChainSpider.checkHeight == int.Parse(jToken["blockindex"].ToString()))
            {
                Dictionary<string, string> where = new Dictionary<string, string>();
                where.Add("txid", jToken["txid"].ToString());
                where.Add("blockindex", jToken["blockindex"].ToString());
                MysqlConn.Delete(DataTableName, where);
            }
            {
                MysqlConn.ExecuteDataInsert(DataTableName, slist);
            }

            Program.Log($"SaveNEP5Transfer {ChainHash} {jToken["blockindex"]} {jToken["txid"]}", Program.LogLevel.Info, ChainHash.ToString());
        }
    }
}
