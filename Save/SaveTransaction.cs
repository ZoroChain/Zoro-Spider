﻿using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Data;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Linq;

namespace Zoro.Spider
{
    class SaveTransaction : SaveBase
    {
        private SaveUTXO utxo;
        private SaveAsset asset;
        private SaveNotify notify;
        private SaveTxScriptMethod txScriptMethod;

        public SaveTransaction(UInt160 chainHash)
            : base(chainHash)
        {
            InitDataTable(TableType.Transaction);

            utxo = new SaveUTXO(chainHash);
            asset = new SaveAsset(chainHash);
            notify = new SaveNotify(chainHash);
            txScriptMethod = new SaveTxScriptMethod(chainHash);
        }

        public override bool CreateTable(string name)
        {
            MysqlConn.CreateTable(TableType.Transaction, name);
            return true;
        }

        public void Save(JToken jObject, uint blockHeight, uint blockTime)
        {
            JObject result = new JObject();
            result["txid"] = jObject["txid"];
            result["size"] = jObject["size"];
            result["type"] = jObject["type"];
            result["version"] = jObject["version"];
            result["attributes"] = jObject["attributes"];
            result["sys_fee"] = jObject["sys_fee"];
            result["scripts"] = jObject["scripts"];
            result["nonce"] = jObject["nonce"];
            result["blockindex"] = blockHeight;
            result["gas_limit"] = jObject["gas_limit"];
            result["gas_price"] = jObject["gas_price"];
            result["account"] = jObject["account"];

            List<string> slist = new List<string>();
            slist.Add(result["txid"].ToString());
            slist.Add(result["size"].ToString());
            slist.Add(result["type"].ToString());
            slist.Add(result["version"].ToString());
            slist.Add(result["attributes"].ToString());
            slist.Add(result["sys_fee"].ToString());
            slist.Add(result["scripts"].ToString());
            slist.Add(result["nonce"].ToString());
            slist.Add(blockHeight.ToString());
            slist.Add(result["gas_limit"].ToString());
            slist.Add(result["gas_price"].ToString());
            slist.Add(UInt160.Parse(StringRemoveZoro(result["account"].ToString()).HexToBytes().Reverse().ToHexString()).ToAddress());

            if (jObject["script"] != null)
                txScriptMethod.Save(jObject["script"].ToString(), blockHeight, jObject["txid"].ToString());
            //Dictionary<string, string> dictionary = new Dictionary<string, string>();
            //dictionary.Add("txid", jObject["txid"].ToString());
            //dictionary.Add("blockheight", blockHeight.ToString());
            //bool exist = MysqlConn.CheckExist(DataTableName, dictionary);
            //if (!exist)
            if (ChainSpider.checkHeight == int.Parse(blockHeight.ToString()))
            {
                Dictionary<string, string> where = new Dictionary<string, string>();
                where.Add("txid", jObject["txid"].ToString());
                where.Add("blockheight", blockHeight.ToString());
                MysqlConn.Delete(DataTableName, where);
            }
            {
                MysqlConn.ExecuteDataInsert(DataTableName, slist);
            }

            Program.Log($"SaveTransaction {ChainHash} {blockHeight}", Program.LogLevel.Info, ChainHash.ToString());

            utxo.Save(result, blockHeight);

            if (result["type"].ToString() == "InvocationTransaction")
            {
                notify.Save(jObject, blockHeight, blockTime, jObject["script"].ToString());
                Thread.Sleep(20);
            }
        }

        private string StringRemoveZoro(string hex) {
            string s = hex;
            if (s.StartsWith("0x")) {
                s = s.Substring(2);
            }
            return s;
        }
    }
}
