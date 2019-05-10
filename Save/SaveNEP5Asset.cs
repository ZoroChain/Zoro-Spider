using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Numerics;
using Neo.VM;
using System.Data;
using System.Threading;

namespace Zoro.Spider
{
    class SaveNEP5Asset : SaveBase
    {
        public SaveNEP5Asset(UInt160 chainHash)
            : base(chainHash)
        {
            InitDataTable(TableType.NEP5Asset);
        }

        public List<string> Nep5List = new List<string>();

        public override bool CreateTable(string name)
        {
            MysqlConn.CreateTable(TableType.NEP5Asset, name);
            return true;
        }

        public void InitNep5List()
        {
            string sql = $"select assetid from {DataTableName}";
            DataTable dt = MysqlConn.ExecuteDataSet(sql).Tables[0];
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    Nep5List.Add(dr["assetid"].ToString());
                }
            }
        }

        public string GetNativeNEP5Asset(UInt160 contract)
        {
            try
            {
                ScriptBuilder sb = new ScriptBuilder();

                sb.EmitSysCall("Zoro.NativeNEP5.Call", "TotalSupply", contract);
                sb.EmitSysCall("Zoro.NativeNEP5.Call", "Name", contract);
                sb.EmitSysCall("Zoro.NativeNEP5.Call", "Symbol", contract);               
                sb.EmitSysCall("Zoro.NativeNEP5.Call", "Decimals", contract);

                string script = Helper.Bytes2HexString(sb.ToArray());

                IO.Json.JObject jObject;

                using (WebClient wc = new WebClient())
                {
                    wc.Proxy = null;
                    var url = $"{Settings.Default.RpcUrl}/?jsonrpc=2.0&id=1&method=invokescript&params=['{ChainHash}','{script}']";
                    var result = wc.DownloadStringTaskAsync(url).Result;
                    jObject = IO.Json.JObject.Parse(result);
                }

                IO.Json.JObject jsonResult = jObject["result"];
                IO.Json.JArray jStack = jsonResult["stack"] as IO.Json.JArray;

                string totalSupply = jStack[0]["type"].AsString() == "ByteArray" ? new BigInteger(Helper.HexString2Bytes(jStack[0]["value"].AsString())).ToString() : jStack[0]["value"].AsString();
                string name = jStack[1]["type"].AsString() == "ByteArray" ? Encoding.UTF8.GetString(Helper.HexString2Bytes(jStack[1]["value"].AsString())) : jStack[1]["value"].AsString();
                string symbol = jStack[2]["type"].AsString() == "ByteArray" ? Encoding.UTF8.GetString(Helper.HexString2Bytes(jStack[2]["value"].AsString())) : jStack[2]["value"].AsString();
                string decimals = jStack[3]["type"].AsString() == "ByteArray" ? BigInteger.Parse(jStack[3]["value"].AsString()).ToString() : jStack[3]["value"].AsString();

                //BCT 和法币锚定 不限制总量
                if (symbol == "BCT") { totalSupply = "0"; }

                List<string> slist = new List<string>();
                slist.Add(contract.ToString());
                slist.Add(totalSupply);
                slist.Add(name);
                slist.Add(symbol);
                slist.Add(decimals);

                return MysqlConn.InsertSqlBuilder(DataTableName, slist);

            }
            catch (Exception e)
            {
                Program.Log($"error occured when call invokescript, chainhash:{ChainHash}, nep5contract:{contract.ToString()}, reason:{e.Message}", Program.LogLevel.Error);
                Thread.Sleep(3000);
                return GetNativeNEP5Asset(contract);
            }
        }

        public string GetNEP5Asset(UInt160 contract)
        {            
            try
            {
                ScriptBuilder sb = new ScriptBuilder();

                sb.EmitAppCall(contract, "totalSupply");
                sb.EmitAppCall(contract, "name");
                sb.EmitAppCall(contract, "symbol");
                sb.EmitAppCall(contract, "decimals");

                JObject jObject;

                var result = ZoroHelper.InvokeScript(sb.ToArray(), ChainHash.ToString()).Result;

                jObject = JObject.Parse(result);
                JArray jStack = jObject["result"]["stack"] as JArray;

                if (jStack[1]["value"].ToString() == "" || jStack[2]["value"].ToString() == "" || jStack[3]["value"].ToString() == "")
                    return "";

                string totalSupply = new BigInteger(Helper.HexString2Bytes(jStack[0]["value"].ToString())).ToString();
                string name = Encoding.UTF8.GetString(Helper.HexString2Bytes(jStack[1]["value"].ToString()));
                string symbol = Encoding.UTF8.GetString(Helper.HexString2Bytes(jStack[2]["value"].ToString()));
                string decimals = BigInteger.Parse(jStack[3]["value"].ToString()).ToString();

                List<string> slist = new List<string>();
                slist.Add(contract.ToString());
                slist.Add(totalSupply);
                slist.Add(name);
                slist.Add(symbol);
                slist.Add(decimals);

                return MysqlConn.InsertSqlBuilder(DataTableName, slist);
            }
            catch (Exception e)
            {
                Program.Log($"error occured when call invokescript, chainhash:{ChainHash}, nep5contract:{contract.ToString()}, reason:{e.Message}", Program.LogLevel.Error);
                Thread.Sleep(3000);
                return GetNEP5Asset(contract);
            }
        }        

    }
}
