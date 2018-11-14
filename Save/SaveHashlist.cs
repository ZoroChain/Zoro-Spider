using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Zoro.Spider
{
    class SaveHashlist : SaveBase
    {
        public SaveHashlist()
            : base(null)
        {
            InitDataTable("hashlist");
        }

        public override bool CreateTable(string name)
        {
            return true;
        }

        public void Save(JToken jObject)
        {
            JObject hashresult = new JObject();

            hashresult["result"] = jObject["result"];

            List<string> slist = new List<string>();

            slist.Add(jObject["hashlist"].ToString());
            MysqlConn.ExecuteDataInsert(DataTableName, slist);
        }
    }
}
