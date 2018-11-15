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
            MysqlConn.SaveAndUpdataHashList(DataTableName, jObject["hashlist"].ToString());
        }
    }
}
