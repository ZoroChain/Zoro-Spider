using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
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
            InitDataTable("notify");

            nep5Asset = new SaveNEP5Asset(chainHash);
            nep5Transfer = new SaveNEP5Transfer(chainHash);
        }

        public override bool CreateTable(string name)
        {
            return true;
        }

        public void Save(WebClient wc, JToken jToken, uint blockHeight)
        {
            JToken result = null;
            try
            {
                var getUrl = Helper.url + "?jsonrpc=2.0&id=1&method=getapplicationlog&params=[" + ChainHash.ToString() + "," + jToken["txid"] + "]";
                var info = wc.DownloadString(getUrl);
                var json = JObject.Parse(info);
                result = json["result"];
            }
            catch (Exception)
            {
                Helper.printLog($"error occured when call getapplicationlog with txid ={jToken["txid"]}");
            }
            if (result != null)
            {
                JObject jObject = new JObject();
                jObject["txid"] = jToken["txid"];
                jObject["vmstate"] = result["vmstate"];
                jObject["gas_consumed"] = result["gas_consumed"];
                jObject["stack"] = result["stack"];
                jObject["notifications"] = result["notifications"];
                jObject["blockindex"] = blockHeight;

                List<string> slist = new List<string>();
                slist.Add(jToken["txid"].ToString());
                slist.Add(result["vmstate"].ToString());
                slist.Add(result["gas_consumed"].ToString());
                slist.Add(result["stack"].ToString());
                slist.Add(result["notifications"].ToString());
                slist.Add(blockHeight.ToString());
                MysqlConn.ExecuteDataInsert(DataTableName, slist);

                var notifyPath = "notify" + Path.DirectorySeparatorChar + result["txid"] + "_" + result["n"] + ".txt";
                File.Delete(notifyPath);
                File.WriteAllText(notifyPath, jObject.ToString(), Encoding.UTF8);

                foreach (JObject notify in jObject["notifications"])
                {
                    if (notify["state"]["value"][0]["type"].ToString() == "ByteArray")
                    {
                        string transfer = Encoding.UTF8.GetString(Helper.HexString2Bytes(notify["state"]["value"][0]["value"].ToString()));
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
                            tx["from"] = Encoding.UTF8.GetString(Helper.HexString2Bytes(notify["state"]["value"][1]["value"].ToString()));
                            tx["to"] = Encoding.UTF8.GetString(Helper.HexString2Bytes(notify["state"]["value"][2]["value"].ToString()));
                            tx["value"] = Encoding.UTF8.GetString(Helper.HexString2Bytes(notify["state"]["value"][3]["value"].ToString()));
                            nep5Transfer.Save(tx);
                        }
                    }
                }
            }
        }
    }
}
