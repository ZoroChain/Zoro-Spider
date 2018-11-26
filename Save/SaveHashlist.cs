using Newtonsoft.Json.Linq;

namespace Zoro.Spider
{
    class SaveHashlist : SaveBase
    {
        private MysqlConn conn;
        public SaveHashlist(MysqlConn conn)
            : base(null)
        {
            InitDataTable(TableType.Hash_List);
            this.conn = conn;
        }

        public override bool CreateTable(string name)
        {
            conn.CreateTable(TableType.Hash_List, name);
            return true;
        }

        public void Save(JToken jObject)
        {
            conn.SaveAndUpdataHashList(DataTableName, jObject["hashlist"].ToString());
        }
    }
}
