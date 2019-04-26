using Neo.VM;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Zoro.Spider
{
    class SaveContractState : SaveBase
    {
        public SaveContractState(UInt160 chainHash)
            : base(chainHash)
        {
            InitDataTable(TableType.Contract_State);
        }

        public override bool CreateTable(string name)
        {
            MysqlConn.CreateTable(TableType.Contract_State, name);
            return true;
        }

        public async void SaveAsync(string hash) {
            JToken result = null;
            try
            {
                WebClient wc = new WebClient();
                wc.Proxy = null;
                result = await GetContractState(wc, ChainHash, hash);
                string supportedStandard = await GetNEP5Asset(UInt160.Parse(hash));
                if (result != null && result["message"] == null) {
                    JToken jToken = result;
                    Dictionary<string, string> selectWhere = new Dictionary<string, string>();
                    selectWhere.Add("hash", jToken["hash"].ToString());
                    DataTable dt = MysqlConn.ExecuteDataSet(DataTableName, selectWhere).Tables[0];
                    if (dt.Rows.Count != 0)
                    {
                        return;
                    }

                    List<string> slist = new List<string>();
                    slist.Add(jToken["hash"].ToString());
                    slist.Add(jToken["name"].ToString());
                    slist.Add(jToken["author"].ToString());
                    slist.Add(jToken["email"].ToString());
                    slist.Add(jToken["description"].ToString());
                    slist.Add(supportedStandard);
                    MysqlConn.ExecuteDataInsert(DataTableName, slist);
                }
                    
            }
            catch (Exception e)
            {
                Program.Log($"error occured when call getcontractstate, chain:{ChainHash}, reason:{e.Message}", Program.LogLevel.Error);
                throw e;
            }
            
        }

        public async Task<JToken> GetContractState(WebClient wc, Zoro.UInt160 ChainHash, string hash)
        {
            try
            {
                var getUrl = $"{Settings.Default.RpcUrl}/?jsonrpc=2.0&id=1&method=getcontractstate&params=['{ChainHash}','{hash}']";
                var info = await wc.DownloadStringTaskAsync(getUrl);
                var json = JObject.Parse(info);
                var result = json["result"];
                return result;
            }
            catch (WebException e)
            {
                Program.Log($"error occured when call getcontractstate, chain:{ChainHash}, reason:{e.Message}", Program.LogLevel.Error);
                //throw e;
                return await GetContractState(wc, ChainHash, hash);
            }
        }

        public async Task<string> GetNEP5Asset(UInt160 Contract)
        {
            try
            {
                ScriptBuilder sb = new ScriptBuilder();

                sb.EmitAppCall(Contract, "supportedStandards");

                JObject jObject;

                var result = await ZoroHelper.InvokeScript(sb.ToArray(), ChainHash.ToString());

                jObject = JObject.Parse(result);
                JArray jStack = jObject["result"]["stack"] as JArray;

                if (jStack[0]["value"].ToString() == "") {
                    return "";
                }
                string supportedStandards = Encoding.UTF8.GetString(Helper.HexString2Bytes(jStack[0]["value"].ToString()));
                return supportedStandards;
            }
            catch (Exception e)
            {
                return await GetNEP5Asset(Contract);
            }
        }
    }
}
