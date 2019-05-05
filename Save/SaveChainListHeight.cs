using System.Collections.Generic;
using System.Data;

namespace Zoro.Spider
{
    class SaveChainListHeight : SaveBase
    {

        public SaveChainListHeight(UInt160 chainHash) : base(null)
        {
            InitDataTable(TableType.Chainlistheight);
        }

        public override bool CreateTable(string name)
        {
            MysqlConn.CreateTable(TableType.Chainlistheight, name);
            return true;
        }

        public void Save(string chainHash, uint height)
        {            
            if (height == 0)
            {
                var list = new List<string>();
                list.Add(chainHash);
                list.Add(height.ToString());
                MysqlConn.ExecuteDataInsert(DataTableName, list);
            }
            else
            {
                var dir = new Dictionary<string, string>();
                dir.Add("chainhash", chainHash);
                var set = new Dictionary<string, string>();
                set.Add("chainheight", height.ToString());
                MysqlConn.Update(DataTableName, set, dir);
            }
        }

        public uint getHeight(string chainHash)
        {
            var dir = new Dictionary<string, string>();
            dir.Add("chainhash", chainHash);
            DataTable dt = MysqlConn.ExecuteDataSet(DataTableName, dir).Tables[0];
            if (dt.Rows.Count == 0)
            {
                return 0;
            }
            else
            {
                return uint.Parse(dt.Rows[0]["chainheight"].ToString());
            }
        }

    }
}
