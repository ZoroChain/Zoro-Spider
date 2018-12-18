﻿using Newtonsoft.Json.Linq;
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

        public void Save(JToken jToken)
        {
            string contract = jToken["assetid"].ToString();
            Dictionary<string, string> where = new Dictionary<string, string>();
            where.Add("assetid", contract);
            bool exist = MysqlConn.CheckExist(DataTableName, where);
            if (!exist)
            {
                Start(contract);
            }
        }

        public async void Start(string contract)
        {
            if (contract.Length == 40 || contract.Length == 42)
            {
                await getNEP5Asset(UInt160.Parse(contract));
            }
            else {
                await getNEP5Asset(UInt256.Parse(contract));
            }           
        }

        public async Task getNEP5Asset(UInt256 Contract) {

            try
            {
                ScriptBuilder sb = new ScriptBuilder();

                sb.EmitSysCall("Zoro.NativeNEP5.TotalSupply", Contract);
                sb.EmitSysCall("Zoro.NativeNEP5.Name", Contract);
                sb.EmitSysCall("Zoro.NativeNEP5.Symbol", Contract);
                sb.EmitSysCall("Zoro.NativeNEP5.Decimals", Contract);

                string script = Helper.Bytes2HexString(sb.ToArray());

                IO.Json.JObject jObject;

                using (WebClient wc = new WebClient())
                {
                    var url = $"{Settings.Default.RpcUrl}/?jsonrpc=2.0&id=1&method=invokescript&params=['{ChainHash}','{script}']";
                    var result = await wc.DownloadStringTaskAsync(url);
                    jObject = IO.Json.JObject.Parse(result);
                }

                IO.Json.JObject jsonResult = jObject["result"];
                IO.Json.JArray jStack = jsonResult["stack"] as IO.Json.JArray;

                string totalSupply = jStack[0]["type"].AsString() == "ByteArray"?new BigInteger(Helper.HexString2Bytes(jStack[0]["value"].AsString())).ToString():jStack[0]["value"].AsString();
                string name = jStack[1]["type"].AsString() == "ByteArray" ? Encoding.UTF8.GetString(Helper.HexString2Bytes(jStack[1]["value"].AsString())) : jStack[1]["value"].AsString();
                string symbol = jStack[2]["type"].AsString() == "ByteArray" ? Encoding.UTF8.GetString(Helper.HexString2Bytes(jStack[2]["value"].AsString())) : jStack[2]["value"].AsString();
                string decimals = jStack[3]["type"].AsString() == "ByteArray" ? BigInteger.Parse(jStack[3]["value"].AsString()).ToString() : jStack[3]["value"].AsString();

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

                string script = Helper.Bytes2HexString(sb.ToArray());

                IO.Json.JObject jObject;

                using (WebClient wc = new WebClient())
                {
                    var url = $"{Settings.Default.RpcUrl}/?jsonrpc=2.0&id=1&method=invokescript&params=['{ChainHash}','{script}']";
                    var result = await wc.DownloadStringTaskAsync(url);
                    jObject = IO.Json.JObject.Parse(result);
                }
                
                IO.Json.JObject jsonResult = jObject["result"];
                IO.Json.JArray jStack = jsonResult["stack"] as IO.Json.JArray;

                
                string totalSupply = new BigInteger(Helper.HexString2Bytes(jStack[0]["value"].AsString())).ToString();
                string name = Encoding.UTF8.GetString(Helper.HexString2Bytes(jStack[1]["value"].AsString()));
                string symbol = Encoding.UTF8.GetString(Helper.HexString2Bytes(jStack[2]["value"].AsString()));
                string decimals = BigInteger.Parse(jStack[3]["value"].AsString()).ToString();

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
