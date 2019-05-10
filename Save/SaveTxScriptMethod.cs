using Neo.VM;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Zoro.Spider
{
    class SaveTxScriptMethod : SaveBase
    {
        public SaveTxScriptMethod(UInt160 chainHash)
            : base(chainHash)
        {
            InitDataTable(TableType.Tx_Script_Method);
        }

        public override bool CreateTable(string name)
        {
            MysqlConn.CreateTable(TableType.Tx_Script_Method, name);
            return true;
        }

        public string GetInsertSql(List<string> slist)
        {
            return MysqlConn.InsertSqlBuilder(DataTableName, slist);
        }
    }

}
