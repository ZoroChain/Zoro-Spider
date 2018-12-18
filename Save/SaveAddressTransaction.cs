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

        public void Save(JToken jObject, uint blockHeight, uint blockTime)
        {                      
            List<string> slist = new List<string>();
            slist.Add(jObject["address"].ToString());
            slist.Add(jObject["txid"].ToString());
            slist.Add(blockHeight.ToString());
            slist.Add(blockTime.ToString());

                
            if (ChainSpider.checkHeight == int.Parse(blockHeight.ToString()))
            {
                Dictionary<string, string> where = new Dictionary<string, string>();
                where.Add("addr", jObject["address"].ToString());
                where.Add("blockindex", blockHeight.ToString());
                where.Add("txid", jObject["txid"].ToString());
                MysqlConn.Delete(DataTableName, where);
            }
            {
                MysqlConn.ExecuteDataInsert(DataTableName, slist);
            }
        }
    }
}
