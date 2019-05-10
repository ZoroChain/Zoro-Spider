using System;
using System.Collections.Generic;
using System.Data;

namespace Zoro.Spider
{
    class SaveAddressAsset:SaveBase
    {
        public SaveAddressAsset(UInt160 chainHash)
            : base(chainHash)
        {
            InitDataTable(TableType.Address_Asset);
        }

        public List<string> AddressAssetList = new List<string>();

        public override bool CreateTable(string name)
        {
            MysqlConn.CreateTable(TableType.Address_Asset, name);
            return true;
        }       

        public string GetInsertSql(List<string> slist)
        {
            return MysqlConn.InsertSqlBuilder(DataTableName, slist);
        }

        public bool AddressAssetIsExisted(Dictionary<string, string> selectWhere)
        {
            DataTable dt = MysqlConn.ExecuteDataSet(DataTableName, selectWhere).Tables[0];
            if (dt.Rows.Count > 0)
                return true;
            return false;
        }
    }
}
