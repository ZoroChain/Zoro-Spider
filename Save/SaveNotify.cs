using System;
using System.Net;
using System.Text;
using System.Data;
using System.Collections.Generic;
using System.Numerics;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace Zoro.Spider
{
    class SaveNotify : SaveBase
    {
        private SaveNEP5Asset nep5Asset;
        private SaveNEP5Transfer nep5Transfer;

        public SaveNotify(UInt160 chainHash)
            : base(chainHash)
        {
            InitDataTable(TableType.Notify);

            nep5Asset = new SaveNEP5Asset(chainHash);
            nep5Transfer = new SaveNEP5Transfer(chainHash);
        }

        public override bool CreateTable(string name)
        {
            MysqlConn.CreateTable(TableType.Notify, name);
            return true;
        }

        public async void Save(JToken jToken, uint blockHeight)
        {
            JToken result = null;
            JToken executions = null;
            try
            {
                WebClient wc = new WebClient();
                var getUrl = $"{Settings.Default.RpcUrl}/?jsonrpc=2.0&id=1&method=getapplicationlog&params=['{ChainHash}','{jToken["txid"]}']";
                var info = await wc.DownloadStringTaskAsync(getUrl);
                var json = JObject.Parse(info);
                result = json["result"];
                executions = result["executions"].First as JToken;
            }
            catch (Exception e)
            {
                Program.Log($"error occured when call getapplicationlog, chain:{ChainHash} height:{blockHeight}, reason:{e.ToString()}", Program.LogLevel.Error);
                throw e;
            }
            if (result != null && executions != null)
            {
                //JObject jObject = new JObject();
                //jObject["txid"] = jToken["txid"];
                //jObject["vmstate"] = executions["vmstate"];
                //jObject["gas_consumed"] = executions["gas_consumed"];
                //jObject["stack"] = executions["stack"];
                //jObject["notifications"] = executions["notifications"];
                //jObject["blockindex"] = blockHeight;

                List<string> slist = new List<string>();
                slist.Add(jToken["txid"].ToString());
                slist.Add(executions["vmstate"].ToString());
                slist.Add(executions["gas_consumed"].ToString());
                slist.Add(executions["stack"].ToString());
                slist.Add(executions["notifications"].ToString());
                slist.Add(blockHeight.ToString());

                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary.Add("txid", jToken["txid"].ToString());
                dictionary.Add("blockindex", blockHeight.ToString());
                bool exist = MysqlConn.CheckExist(DataTableName, dictionary);
                if (!exist)
                {
                    MysqlConn.ExecuteDataInsert(DataTableName, slist);
                }

                Program.Log($"SaveNotify {ChainHash} {jToken["txid"]}", Program.LogLevel.Info);
                Program.Log(result.ToString(), Program.LogLevel.Debug);

                //var notifyPath = "notify" + Path.DirectorySeparatorChar + result["txid"] + "_" + result["n"] + ".txt";
                //File.Delete(notifyPath);
                //File.WriteAllText(notifyPath, jObject.ToString(), Encoding.UTF8);

                JToken notifications = executions["notifications"];

                foreach (JObject notify in notifications)
                {
                    JToken values = notify["state"]["value"];

                    if (values[0]["type"].ToString() == "ByteArray")
                    {
                        string transfer = Encoding.UTF8.GetString(Helper.HexString2Bytes(values[0]["value"].ToString()));
                        string contract = notify["contract"].ToString();

                        if (transfer == "transfer")
                        {
                            JObject nep5 = new JObject();
                            nep5["assetid"] = contract;
                            nep5Asset.Save(nep5);

                            //存储Nep5Transfer内容
                            JObject tx = new JObject();
                            tx["blockindex"] = blockHeight;
                            tx["txid"] = jToken["txid"].ToString();
                            tx["n"] = 0;
                            tx["asset"] = contract;
                            tx["from"] = values[1]["value"].ToString();
                            tx["to"] = values[2]["value"].ToString();
                            tx["value"] = BigInteger.Parse(values[3]["value"].ToString(), NumberStyles.AllowHexSpecifier).ToString();

                            nep5Transfer.Save(tx);
                        }
                    }
                }
            }
        }
    }
}
