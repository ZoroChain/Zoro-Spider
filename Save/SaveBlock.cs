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
            slist.Add((jObject["tx"] as JArray).Count.ToString());

            if (ChainSpider.checkHeight == int.Parse(jObject["index"].ToString())) {
                Dictionary<string, string> where = new Dictionary<string, string>();
                where.Add("indexx", jObject["index"].ToString());
                MysqlConn.Delete(DataTableName, where);
            }
            MysqlConn.ExecuteDataInsert(DataTableName, slist);
            
            uint blockTime = uint.Parse(jObject["time"].ToString());

            int numTx = 0;
            foreach (var tx in jObject["tx"])
            {
                trans.Save(tx, height, blockTime);
                numTx++;
            }

            Program.Log($"BlockSaved {ChainHash} height:{height} tx:{numTx}", Program.LogLevel.Warning, ChainHash.ToString());
        }
    }
}
