using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Neo.VM;

namespace Zoro.Spider
{
    class SaveNEP5Asset : SaveBase
    {
        public SaveNEP5Asset(UInt160 chainHash)
            : base(chainHash)
        {
            InitDataTable(TableType.NEP5Asset);
        }

        public override bool CreateTable(string name)
        {
            MysqlConn.CreateTable(TableType.NEP5Asset, name);
            return true;
        }

        public void Save(JToken jToken)
        {
            string contract = jToken["assetid"].ToString();
            Dictionary<string, string> where = new Dictionary<string, string>();
            where.Add("assetid", contract);
            DataTable dt = MysqlConn.ExecuteDataSet(DataTableName, where).Tables[0];
            if (dt.Rows.Count == 0)
            {
                Start(contract);
            }
        }

        public async void Start(string contract)
        {
            await getNEP5Asset(UInt160.Parse(contract));
        }

        public async Task getNEP5Asset(UInt160 Contract)
        {
            ScriptBuilder sb = new ScriptBuilder();

            JArray array = new JArray();
            sb.EmitPush(array);
            sb.EmitPush("totalSupply");
            sb.EmitAppCall(Contract);

            sb.EmitPush(array);
            sb.EmitPush("name");
            sb.EmitAppCall(Contract);

            sb.EmitPush(array);
            sb.EmitPush("symbol");
            sb.EmitAppCall(Contract);

            sb.EmitPush(array);
            sb.EmitPush("decimals");
            sb.EmitAppCall(Contract);

            string scriptPublish = Helper.Bytes2HexString(sb.ToArray());

            byte[] postdata;
            var url = Helper.MakeRpcUrlPost(Helper.url, "invokescript", out postdata, new JObject(scriptPublish));
            var result = await Helper.HttpPost(url, postdata);

            JObject jObject = JObject.Parse(result);
            JArray results = jObject["result"]["stack"] as JArray;
            string totalSupply = Encoding.UTF8.GetString(Helper.HexString2Bytes(results[0]["value"].ToString()));
            string name = Encoding.UTF8.GetString(Helper.HexString2Bytes(results[1]["value"].ToString()));
            string symbol = Encoding.UTF8.GetString(Helper.HexString2Bytes(results[2]["value"].ToString()));
            string decimals = Encoding.UTF8.GetString(Helper.HexString2Bytes(results[3]["value"].ToString()));

            List<string> slist = new List<string>();
            slist.Add(Contract.ToString());
            slist.Add(totalSupply);
            slist.Add(name);
            slist.Add(symbol);
            slist.Add(decimals);
            MysqlConn.ExecuteDataInsert(DataTableName, slist);
        }
    }
}
