using Newtonsoft.Json.Linq;
using System;
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

        public void Save(JToken jToken, string script)
        {
            string sql = $"select * from {DataTableName} where assetid = '{jToken["assetid"].ToString()}'";
            
            bool exist = MysqlConn.CheckExist(sql);
            if (!exist)
            {
                Start(jToken["assetid"].ToString(), script);
            }
        }

        public async void Start(string contract, string script)
        {
            if (script.EndsWith(Helper.ZoroNativeNep5Call))
                await getNativeNEP5Asset(UInt160.Parse(contract));
            else
                await getNEP5Asset(UInt160.Parse(contract));        
        }     

        public async Task getNativeNEP5Asset(UInt160 Contract)
        {
            try
            {
                ScriptBuilder sb = new ScriptBuilder();

                sb.EmitSysCall("Zoro.NativeNEP5.Call", "TotalSupply", Contract);
                sb.EmitSysCall("Zoro.NativeNEP5.Call", "Name", Contract);
                sb.EmitSysCall("Zoro.NativeNEP5.Call", "Symbol", Contract);               
                sb.EmitSysCall("Zoro.NativeNEP5.Call", "Decimals", Contract);

                string script = Helper.Bytes2HexString(sb.ToArray());

                IO.Json.JObject jObject;

                using (WebClient wc = new WebClient())
                {
                    wc.Proxy = null;
                    var url = $"{Settings.Default.RpcUrl}/?jsonrpc=2.0&id=1&method=invokescript&params=['{ChainHash}','{script}']";
                    var result = await wc.DownloadStringTaskAsync(url);
                    jObject = IO.Json.JObject.Parse(result);
                }

                IO.Json.JObject jsonResult = jObject["result"];
                IO.Json.JArray jStack = jsonResult["stack"] as IO.Json.JArray;

                string totalSupply = jStack[0]["type"].AsString() == "ByteArray" ? new BigInteger(Helper.HexString2Bytes(jStack[0]["value"].AsString())).ToString() : jStack[0]["value"].AsString();
                string name = jStack[1]["type"].AsString() == "ByteArray" ? Encoding.UTF8.GetString(Helper.HexString2Bytes(jStack[1]["value"].AsString())) : jStack[1]["value"].AsString();
                string symbol = jStack[2]["type"].AsString() == "ByteArray" ? Encoding.UTF8.GetString(Helper.HexString2Bytes(jStack[2]["value"].AsString())) : jStack[2]["value"].AsString();
                string decimals = jStack[3]["type"].AsString() == "ByteArray" ? BigInteger.Parse(jStack[3]["value"].AsString()).ToString() : jStack[3]["value"].AsString();

                //BCT没有限制，可以随意增发
                if (symbol == "BCT") { totalSupply = "0"; }

                List<string> slist = new List<string>();
                slist.Add(Contract.ToString());
                slist.Add(totalSupply);
                slist.Add(name);
                slist.Add(symbol);
                slist.Add(decimals);

                
                //这里有个bug，我们的bcp会因为转账而增长          
                {
                    MysqlConn.ExecuteDataInsert(DataTableName, slist);
                }

                Program.Log($"SaveNEP5Asset {ChainHash} {Contract}", Program.LogLevel.Info, ChainHash.ToString());
            }
            catch (Exception e)
            {
                Program.Log($"error occured when call invokescript, chainhash:{ChainHash}, nep5contract:{Contract.ToString()}, reason:{e.Message}", Program.LogLevel.Error);
                throw e;
            }
        }

        public async Task getNEP5Asset(UInt160 Contract)
        {            
            try
            {
                ScriptBuilder sb = new ScriptBuilder();

                sb.EmitAppCall(Contract, "totalSupply");
                sb.EmitAppCall(Contract, "name");
                sb.EmitAppCall(Contract, "symbol");
                sb.EmitAppCall(Contract, "decimals");

                JObject jObject;

                var result = await ZoroHelper.InvokeScript(sb.ToArray(), ChainHash.ToString());

                jObject = JObject.Parse(result);
                JArray jStack = jObject["result"]["stack"] as JArray;

                if (jStack[1]["value"].ToString() == "" || jStack[2]["value"].ToString() == "" || jStack[3]["value"].ToString() == "")
                    return;                

                string totalSupply = new BigInteger(Helper.HexString2Bytes(jStack[0]["value"].ToString())).ToString();
                string name = Encoding.UTF8.GetString(Helper.HexString2Bytes(jStack[1]["value"].ToString()));
                string symbol = Encoding.UTF8.GetString(Helper.HexString2Bytes(jStack[2]["value"].ToString()));
                string decimals = BigInteger.Parse(jStack[3]["value"].ToString()).ToString();

                List<string> slist = new List<string>();
                slist.Add(Contract.ToString());
                slist.Add(totalSupply);
                slist.Add(name);
                slist.Add(symbol);
                slist.Add(decimals);

                //Dictionary<string, string> dictionary = new Dictionary<string, string>();
                //dictionary.Add("assetid", Contract.ToString());
                //bool exist = MysqlConn.CheckExist(DataTableName, dictionary);
                //if (!exist)
                //这里有个bug，我们的bcp会因为转账而增长          
                {
                    MysqlConn.ExecuteDataInsert(DataTableName, slist);
                }

                Program.Log($"SaveNEP5Asset {ChainHash} {Contract}", Program.LogLevel.Info, ChainHash.ToString());
            }
            catch (Exception e)
            {
                Program.Log($"error occured when call invokescript, chainhash:{ChainHash}, nep5contract:{Contract.ToString()}, reason:{e.Message}", Program.LogLevel.Error);
                throw e;
            }
        }
    }
}
