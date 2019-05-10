using System.Collections.Generic;

namespace Zoro.Spider
{
    class SaveNFTAddress : SaveBase
    {
        public SaveNFTAddress(UInt160 chainHash)
            : base(chainHash)
        {
            InitDataTable(TableType.NFT_Address);
        }

        public override bool CreateTable(string name)
        {
            MysqlConn.CreateTable(TableType.NFT_Address, name);
            return true;
        }

        public string GetInsertSql(string contract, string addr, string nfttoken, string properties)
        {
            string sql = "";
            List<string> slist = new List<string>();
            slist.Add(contract);
            slist.Add(addr);
            slist.Add(nfttoken);
            slist.Add(properties);
            sql = MysqlConn.InsertSqlBuilder(DataTableName, slist);
            return sql;
        }

        public string GetTransferSql(string contract, string addr, string nfttoken)
        {
            Dictionary<string, string> selectWhere = new Dictionary<string, string>();
            selectWhere.Add("contract", contract);
            selectWhere.Add("nfttoken", nfttoken);
            Dictionary<string, string> dirs = new Dictionary<string, string>();
            dirs.Add("addr", addr);
            return MysqlConn.UpdateSqlBuilder(DataTableName, dirs, selectWhere);
        }

        public string GetUpdateSql(string contract, string nfttoken, string properties)
        {
            Dictionary<string, string> selectWhere = new Dictionary<string, string>();
            selectWhere.Add("contract", contract);
            selectWhere.Add("nfttoken", nfttoken);
            Dictionary<string, string> dirs = new Dictionary<string, string>();
            dirs.Add("properties", properties);
            return MysqlConn.UpdateSqlBuilder(DataTableName, dirs, selectWhere);
        }
    }
}
