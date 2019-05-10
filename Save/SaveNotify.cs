using System;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Numerics;
using System.Globalization;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Data;

namespace Zoro.Spider
{
    class SaveNotify : SaveBase
    {
        private SaveAddressTransaction address_tx;
        private SaveNEP5Transfer nep5Transfer;
        private SaveNFTAddress nftAddress;

        private SaveAddress saveAddress;
        private SaveAddressAsset addressAsset;
        private SaveNEP5Asset nep5Asset;

        public SaveNotify(UInt160 chainHash)
            : base(chainHash)
        {
            InitDataTable(TableType.Notify);

            address_tx = new SaveAddressTransaction(chainHash);
            nep5Transfer = new SaveNEP5Transfer(chainHash);
            nftAddress = new SaveNFTAddress(chainHash);

            saveAddress = new SaveAddress(chainHash);
            addressAsset = new SaveAddressAsset(chainHash);
            nep5Asset = new SaveNEP5Asset(chainHash);
            nep5Asset.InitNep5List();
        }

        public void ListClear()
        {
            saveAddress.AddressTxCountDict.Clear();
            addressAsset.AddressAssetList.Clear();
        }

        public override bool CreateTable(string name)
        {
            MysqlConn.CreateTable(TableType.Notify, name);
            return true;
        }

        public async Task<JToken> GetApplicationlog(WebClient wc, Zoro.UInt160 ChainHash, string txid, uint blockHeight)
        {
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
                Thread.Sleep(3000);
                return await GetApplicationlog(wc, ChainHash, txid, blockHeight);
            }
        }

        public string GetNotifySqlText(JToken jToken, uint blockHeight, uint blockTime, Dictionary<string, string> contractDict)
        {
            string sql = "";
            JToken result = null;
            JToken executions = null;
            string script = jToken["script"].ToString();
            string txid = jToken["txid"].ToString();
            WebClient wc = new WebClient();
            wc.Proxy = null;
            try
            {
                result = GetApplicationlog(wc, ChainHash, txid, blockHeight).Result;
                if (result != null)
                    executions = result["executions"].First as JToken;
            }
            catch (Exception e)
            {
                Program.Log($"error occured when call getapplicationlog, chain:{ChainHash} txid:{txid}, reason:{e.Message}", Program.LogLevel.Error);
                Thread.Sleep(3000);
                result = GetApplicationlog(wc, ChainHash, txid, blockHeight).Result;
            }

            if (result != null && executions != null)
            {
                List<string> slist = new List<string>();
                slist.Add(txid);
                slist.Add(executions["vmstate"].ToString());
                slist.Add(executions["gas_consumed"].ToString());
                slist.Add(executions["stack"].ToString());
                slist.Add(executions["notifications"].ToString().Replace(@"[/n/r]", ""));
                slist.Add(blockHeight.ToString());

                sql += MysqlConn.InsertSqlBuilder(DataTableName, slist);                

                if (executions["vmstate"].ToString().Contains("FAULT"))
                    return sql;

                JToken notifications = executions["notifications"];

                foreach (JObject notify in notifications)
                {
                    JToken values = notify["state"]["value"];

                    if (values[0]["type"].ToString() == "ByteArray")
                    {
                        string method = Encoding.UTF8.GetString(Helper.HexString2Bytes(values[0]["value"].ToString()));
                        string contract = notify["contract"].ToString();

                        if (method == "mintToken" && contractDict[contract].Contains("NEP-10"))
                        {
                            sql += nftAddress.GetInsertSql(contract, UInt160.Parse(values[1]["value"].ToString()).ToAddress(), values[2]["value"].ToString(), values[3] == null ? "" : values[3]["value"].ToString());
                        }
                        if (method == "modifyProperties" && contractDict[contract].Contains("NEP-10"))
                        {
                            sql += nftAddress.GetUpdateSql(contract, values[1]["value"].ToString(), values[2]["value"].ToString());
                        }
                        if (method == "transfer")
                        {
                            //存储 Transfer 内容
                            JObject tx = new JObject();
                            tx["blockindex"] = blockHeight;
                            tx["txid"] = txid;
                            tx["n"] = 0;
                            tx["asset"] = contract;
                            tx["from"] = values[1]["value"].ToString() == "" ? "" : UInt160.Parse(values[1]["value"].ToString()).ToAddress();
                            tx["to"] = UInt160.Parse(values[2]["value"].ToString()).ToAddress();
                            tx["value"] = values[3]["type"].ToString() == "ByteArray" ? new BigInteger(Helper.HexString2Bytes(values[3]["value"].ToString())).ToString() :
                            tx["value"] = BigInteger.Parse(values[3]["value"].ToString(), NumberStyles.AllowHexSpecifier).ToString();

                            sql += address_tx.GetAddressTxSql(tx["to"].ToString(), tx["txid"].ToString(), blockHeight, blockTime);
                            sql += nep5Transfer.GetNep5TransferSql(tx);

                            if (contractDict.ContainsKey(contract) && contractDict[contract].Contains("NEP-10"))
                            {
                                sql += nftAddress.GetTransferSql(contract, UInt160.Parse(values[2]["value"].ToString()).ToAddress(), values[3]["value"].ToString());
                            }

                            sql += GetNep5AssetSql(contract, script);
                            sql += GetAddressSql(tx["to"].ToString(), blockTime);
                            sql += GetAddressAssetSql(tx["to"].ToString(), contract, script);
                        }
                    }
                }
            }

            return sql;
        }

        public string GetNep5AssetSql(string contract, string script)
        {
            if (nep5Asset.Nep5List.Contains(contract))
                return "";

            nep5Asset.Nep5List.Add(contract);

            if (script.EndsWith(Helper.ZoroNativeNep5Call))
                return nep5Asset.GetNativeNEP5Asset(UInt160.Parse(contract));
            else
                return nep5Asset.GetNEP5Asset(UInt160.Parse(contract));
        }

        public string GetAddressSql(string address, uint blockTime)
        {                  
            string sql = "";
            DataTable dt = saveAddress.GetAddressDt(address);

            if (dt.Rows.Count == 0)
            {
                if (!saveAddress.AddressTxCountDict.ContainsKey(address))
                {
                    List<string> slist = new List<string>();
                    slist.Add(address);
                    slist.Add(blockTime.ToString());
                    slist.Add(blockTime.ToString());
                    slist.Add("1");
                    sql = saveAddress.GetInsertSql(slist);

                    saveAddress.AddressTxCountDict[address] = 1;
                }
                else
                {
                    Dictionary<string, string> dirs = new Dictionary<string, string>();
                    dirs.Add("lastuse", blockTime.ToString());
                    dirs.Add("txcount", (saveAddress.AddressTxCountDict[address] + 1).ToString());
                    Dictionary<string, string> where = new Dictionary<string, string>();
                    where.Add("addr", address);
                    sql = saveAddress.GetUpdateSql(dirs, where);

                    saveAddress.AddressTxCountDict[address]++;
                }
            }

            else
            {
                if (!saveAddress.AddressTxCountDict.ContainsKey(address))
                {
                    saveAddress.AddressTxCountDict[address] = int.Parse(dt.Rows[0]["txcount"].ToString());
                }

                Dictionary<string, string> dirs = new Dictionary<string, string>();
                dirs.Add("lastuse", blockTime.ToString());
                dirs.Add("txcount", (saveAddress.AddressTxCountDict[address] + 1).ToString());
                Dictionary<string, string> where = new Dictionary<string, string>();
                where.Add("addr", address);
                sql = saveAddress.GetUpdateSql(dirs, where);

                saveAddress.AddressTxCountDict[address]++;
            }

            return sql;
        }

        public string GetAddressAssetSql(string addr, string asset, string script)
        {
            if (addressAsset.AddressAssetList.Contains(addr + asset))
                return "";

            string sql = "";
            Dictionary<string, string> selectWhere = new Dictionary<string, string>();
            selectWhere.Add("addr", addr);
            selectWhere.Add("asset", asset);

            bool isExisted = addressAsset.AddressAssetIsExisted(selectWhere);

            if (!isExisted)
            {
                string type = "";
                if (script.EndsWith(Helper.ZoroNativeNep5Call))
                    type = "NativeNep5";
                else
                    type = "Nep5";
                List<string> slist = new List<string>();
                slist.Add(addr);
                slist.Add(asset);
                slist.Add(type);
                sql = addressAsset.GetInsertSql(slist);
            }

            addressAsset.AddressAssetList.Add(addr + asset);
            return sql;
        }

    }
}
