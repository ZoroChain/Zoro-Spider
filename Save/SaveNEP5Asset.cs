using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Net;
using System.Numerics;
using System.Globalization;
using System.Threading.Tasks;
using Neo.VM;

namespace Zoro.Spider
{
    class SaveNEP5Asset : SaveBase
    {
        private WebClient wc;

        public SaveNEP5Asset(WebClient wc, UInt160 chainHash)
            : base(chainHash)
        {
            InitDataTable(TableType.NEP5Asset);
            this.wc = wc;
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

            sb.EmitAppCall(Contract, "totalSupply");
            sb.EmitAppCall(Contract, "name");
            sb.EmitAppCall(Contract, "symbol");
            sb.EmitAppCall(Contract, "decimals");

            string script = Helper.Bytes2HexString(sb.ToArray());

            var url = $"{Settings.Default.RpcUrl}/?jsonrpc=2.0&id=1&method=invokescript&params=['{ChainHash}','{script}']";
            var result = await wc.DownloadStringTaskAsync(url);

            IO.Json.JObject jObject = IO.Json.JObject.Parse(result);
            IO.Json.JObject jsonResult = jObject["result"];
            IO.Json.JArray jStack = jsonResult["stack"] as IO.Json.JArray;

            string totalSupply = BigInteger.Parse(jStack[0]["value"].AsString(), NumberStyles.AllowHexSpecifier).ToString();
            string name = Encoding.UTF8.GetString(Helper.HexString2Bytes(jStack[1]["value"].AsString()));
            string symbol = Encoding.UTF8.GetString(Helper.HexString2Bytes(jStack[2]["value"].AsString()));
            string decimals = BigInteger.Parse(jStack[3]["value"].AsString()).ToString();

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
