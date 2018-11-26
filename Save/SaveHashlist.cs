using Newtonsoft.Json.Linq;

namespace Zoro.Spider
{
    class SaveHashlist : SaveBase
    {
        public SaveHashlist()
            : base(null)
        {
            InitDataTable(TableType.Hash_List);
        }

        public override bool CreateTable(string name)
        {
            MysqlConn.CreateTable(TableType.Hash_List, name);
            return true;
        }

        public void Save(JToken jObject)
        {
            MysqlConn.SaveAndUpdataHashList(DataTableName, jObject["hashlist"].ToString());
        }
    }
}
