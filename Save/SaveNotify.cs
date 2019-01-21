using System;
using System.Net;
using System.Text;
using System.Data;
using System.Collections.Generic;
using System.Numerics;
using System.Globalization;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Zoro.Wallets;

namespace Zoro.Spider
{
    class SaveNotify : SaveBase
    {
        private SaveAddress address;
        private SaveAddressAsset addressAsset;
        private SaveAddressTransaction address_tx;
        private SaveNEP5Asset nep5Asset;
        private SaveNEP5Transfer nep5Transfer;

        public SaveNotify(UInt160 chainHash)
            : base(chainHash)
        {
            InitDataTable(TableType.Notify);

            address = new SaveAddress(chainHash);
            addressAsset = new SaveAddressAsset(chainHash);
            address_tx = new SaveAddressTransaction(chainHash);
            nep5Asset = new SaveNEP5Asset(chainHash);
            nep5Transfer = new SaveNEP5Transfer(chainHash);
        }

        public override bool CreateTable(string name)
        {
            MysqlConn.CreateTable(TableType.Notify, name);
            return true;
        }

        public async Task<JToken> GetApplicationlog(WebClient wc, Zoro.UInt160 ChainHash, string txid, uint blockHeight) {
            try
            {
                var getUrl = $"{Settings.Default.RpcUrl}/?jsonrpc=2.0&id=1&method=getapplicationlog&params=['{ChainHash}','{txid}']";
                var info = await wc.DownloadStringTaskAsync(getUrl);
                var json = JObject.Parse(info);
                var result = json["result"];
                return result;
            }
            catch (WebException e)
            {
                Program.Log($"error occured when call getapplicationlog, chain:{ChainHash} height:{blockHeight}, reason:{e.Message}", Program.LogLevel.Error);
                //throw e;
                return await GetApplicationlog(wc, ChainHash, txid, blockHeight);
            }
        }

        public async void Save(JToken jToken, uint blockHeight, uint blockTime, string script)
        {
            JToken result = null;
            JToken executions = null;
            try
            {
                WebClient wc = new WebClient();
                wc.Proxy = null;
                result = await GetApplicationlog(wc, ChainHash, jToken["txid"].ToString(), blockHeight);
                if (result != null)
                executions = result["executions"].First as JToken;
            }
            catch (Exception e)
            {
                Program.Log($"error occured when call getapplicationlog, chain:{ChainHash} height:{blockHeight}, reason:{e.Message}", Program.LogLevel.Error);
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

                //Dictionary<string, string> dictionary = new Dictionary<string, string>();
                //dictionary.Add("txid", jToken["txid"].ToString());
                //dictionary.Add("blockindex", blockHeight.ToString());
                //bool exist = MysqlConn.CheckExist(DataTableName, dictionary);
                //if (!exist)
                if (ChainSpider.checkHeight == int.Parse(blockHeight.ToString()))
                {
                    Dictionary<string, string> where = new Dictionary<string, string>();
                    where.Add("txid", jToken["txid"].ToString());
                    where.Add("blockindex", blockHeight.ToString());
                    MysqlConn.Delete(DataTableName, where);
                }
                {
                    MysqlConn.ExecuteDataInsert(DataTableName, slist);
                }

                Program.Log($"SaveNotify {ChainHash} {jToken["txid"]}", Program.LogLevel.Info, ChainHash.ToString());

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
                            nep5Asset.Save(nep5, script);

                            //存储Nep5Transfer内容
                            JObject tx = new JObject();
                            tx["blockindex"] = blockHeight;
                            tx["txid"] = jToken["txid"].ToString();
                            tx["n"] = 0;
                            tx["asset"] = contract;
                            if (values[1]["value"].ToString() == "")
                            {
                                tx["from"] = "";
                            }
                            else {
                                tx["from"] = UInt160.Parse(values[1]["value"].ToString()).ToAddress();
                            }
                            
                            tx["to"] = UInt160.Parse(values[2]["value"].ToString()).ToAddress();
                            if (values[3]["type"].ToString() == "ByteArray")
                            {
                                tx["value"] = new BigInteger(Helper.HexString2Bytes(values[3]["value"].ToString())).ToString();
                            }
                            else {
                                tx["value"] = BigInteger.Parse(values[3]["value"].ToString(), NumberStyles.AllowHexSpecifier).ToString();
                            }                           
                            JObject j = new JObject();
                            j["address"] = tx["to"].ToString();
                            j["txid"] = tx["txid"].ToString();
                            address.Save(j, blockHeight, blockTime);
                            addressAsset.Save(tx["to"].ToString(), contract, script);
                            address_tx.Save(j, blockHeight, blockTime);
                            nep5Transfer.Save(tx);
                        }
                    }
                }
            }
        }
    }
}
