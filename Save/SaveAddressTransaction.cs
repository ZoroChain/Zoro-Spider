using Newtonsoft.Json.Linq;
using System.Collections.Generic;

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

            Dictionary<string, string> deleteWhere = new Dictionary<string, string>();
            deleteWhere.Add("addr", jObject["address"].ToString());
            deleteWhere.Add("blockindex", blockHeight.ToString());
            deleteWhere.Add("txid", jObject["txid"].ToString());

            MysqlConn.ExecuteDataInsertWithCheck(DataTableName, slist, deleteWhere);

        }
    }
}
