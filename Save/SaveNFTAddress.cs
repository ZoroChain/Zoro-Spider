using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

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

        public void Save(string contract, string addr, string nfttoken, string properties)
        {
            List<string> slist = new List<string>();
            slist.Add(contract);
            slist.Add(addr);
            slist.Add(nfttoken);
            slist.Add(properties);
            MysqlConn.ExecuteDataInsert(DataTableName, slist);
        }

        public void Save(string contract, string addr, string nfttoken) {
            Dictionary<string, string> selectWhere = new Dictionary<string, string>();
            selectWhere.Add("contract", contract);
            selectWhere.Add("nfttoken", nfttoken);
            Dictionary<string, string> dirs = new Dictionary<string, string>();
            dirs.Add("addr", addr);
            MysqlConn.Update(DataTableName, dirs, selectWhere);
        }

        public void Update(string contract, string nfttoken, string properties) {
            Dictionary<string, string> selectWhere = new Dictionary<string, string>();
            selectWhere.Add("contract", contract);
            selectWhere.Add("nfttoken", nfttoken);
            Dictionary<string, string> dirs = new Dictionary<string, string>();
            dirs.Add("properties", properties);
            MysqlConn.Update(DataTableName, dirs, selectWhere);
        }
    }
}
