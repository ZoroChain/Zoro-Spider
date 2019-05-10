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

        public string GetAddressTxSql(string address, string txid, uint blockHeight, uint blockTime)
        {
            List<string> slist = new List<string>();
            slist.Add(address);
            slist.Add(txid);
            slist.Add(blockHeight.ToString());
            slist.Add(blockTime.ToString());

            return MysqlConn.InsertSqlBuilder(DataTableName, slist);
        }
    }
}
