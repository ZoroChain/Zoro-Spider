using Newtonsoft.Json.Linq;
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

        public void Save(JToken jObject, uint blockHeight, uint blockTime)
        {
            foreach (JObject j in jObject)
            {
                Dictionary<string, string> selectWhere = new Dictionary<string, string>();
                selectWhere.Add("addr", j["address"].ToString());
                DataTable dt = MysqlConn.ExecuteDataSet(DataTableName, selectWhere).Tables[0];
                if (dt.Rows.Count != 0)
                {
                    Dictionary<string, string> dirs = new Dictionary<string, string>();
                    dirs.Add("lastuse", blockTime.ToString());
                    dirs.Add("txcount", (int.Parse(dt.Rows[0]["txcount"].ToString()) + 1) + "");
                    Dictionary<string, string> where = new Dictionary<string, string>();
                    where.Add("addr", dt.Rows[0]["addr"].ToString());
                    MysqlConn.Update(DataTableName, dirs, where);
                }
                else
                {
                    JObject result = new JObject();
                    result["addr"] = j["address"];
                    result["firstuse"] = blockHeight;
                    result["lastuse"] = blockHeight;
                    result["txcount"] = 1;

                    List<string> slist = new List<string>();
                    slist.Add(j["address"].ToString());
                    slist.Add(blockHeight.ToString());
                    slist.Add(blockHeight.ToString());
                    slist.Add("1");
                    MysqlConn.ExecuteDataInsert(DataTableName, slist);
                }
            }
        }
    }
}
