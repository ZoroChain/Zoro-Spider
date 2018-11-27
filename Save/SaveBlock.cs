﻿using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System;

namespace Zoro.Spider
{
    class SaveBlock : SaveBase
    {
        private SaveTransaction trans;

        public SaveBlock(UInt160 chainHash)
            : base(chainHash)
        {
            InitDataTable(TableType.Block);

            trans = new SaveTransaction(chainHash);
        }

        public override bool CreateTable(string name)
        {
            MysqlConn.CreateTable(TableType.Block, name);
            return true;
        }

        public void Save(WebClient wc, JToken jObject, uint height)
        {
            JObject result = new JObject();
            result["hash"] = jObject["hash"];
            result["size"] = jObject["size"];
            result["version"] = jObject["version"];
            result["previousblockhash"] = jObject["previousblockhash"];
            result["merkleroot"] = jObject["merkleroot"];
            result["time"] = jObject["time"];
            result["index"] = jObject["index"];
            result["nonce"] = jObject["nonce"];
            result["nextconsensus"] = jObject["nextconsensus"];
            result["script"] = jObject["script"];

            List<string> slist = new List<string>();
            slist.Add(jObject["hash"].ToString());
            slist.Add(jObject["size"].ToString());
            slist.Add(jObject["version"].ToString());
            slist.Add(jObject["previousblockhash"].ToString());
            slist.Add(jObject["merkleroot"].ToString());
            slist.Add(jObject["time"].ToString());
            slist.Add(jObject["index"].ToString());
            slist.Add(jObject["nonce"].ToString());
            slist.Add(jObject["nextconsensus"].ToString());
            slist.Add(jObject["script"].ToString());
            slist.Add(jObject["tx"].ToString());

            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("hash", jObject["hash"].ToString());

            DateTime dt = DateTime.Now;

            bool exist = MysqlConn.CheckExist(DataTableName, dictionary);
            if (!exist) {
                MysqlConn.ExecuteDataInsert(DataTableName, slist);
            }
            
            uint blockTime = uint.Parse(result["time"].ToString());

            Program.Log($"SaveBlock {ChainHash} {height}", Program.LogLevel.Debug);
            Program.Log(result.ToString(), Program.LogLevel.Debug);

            int numTx = 0;
            foreach (var tx in jObject["tx"])
            {
                trans.Save(tx, height, blockTime);
                numTx++;
            }

            TimeSpan span = DateTime.Now - dt;

            Program.Log($"BlockSaved {ChainHash} height:{height} tx:{numTx} time:{span:hh\\:mm\\:ss\\.fff}", Program.LogLevel.Warning);
        }
    }
}
