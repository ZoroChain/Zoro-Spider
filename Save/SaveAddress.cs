using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;

namespace Zoro.Spider
{
    class SaveAddress : SaveBase
    {
        public SaveAddress(UInt160 chainHash)
            : base(chainHash)
        {
            InitDataTable(TableType.Address);
        }

        public override bool CreateTable(string name)
        {
            MysqlConn.CreateTable(TableType.Address, name);
            return true;
        }

        //public List<string> AddressList = new List<string>();
        public Dictionary<string,int> AddressTxCountDict = new Dictionary<string, int>();        

        public string GetInsertSql(List<string> slist)
        {
            return MysqlConn.InsertSqlBuilder(DataTableName, slist);
        }

        public string GetUpdateSql(Dictionary<string, string> dirs, Dictionary<string, string> where)
        {
            return MysqlConn.UpdateSqlBuilder(DataTableName, dirs, where);
        }        

        public DataTable GetAddressDt(string address)
        {
            string sql = $"select * from {DataTableName} where addr = '{address}'";
            DataTable dt = MysqlConn.ExecuteDataSet(sql).Tables[0];
            return dt;
        }
    }
}
