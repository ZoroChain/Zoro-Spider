using Neo.VM;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Text;
using System.Threading;
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

        public Dictionary<string, string> ContractDict = new Dictionary<string, string>();

        public void InitContractDict()
        {
            string sql = $"select hash, supportstandard from {DataTableName}";
            DataTable dt = MysqlConn.ExecuteDataSet(sql).Tables[0];
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    ContractDict[dr["hash"].ToString()] = dr["supportstandard"].ToString();
                }
            }
        }

        public string GetContractInfoSql(string hash, ref Dictionary<string, string> contractDict)
        {
            if (contractDict.ContainsKey(hash))
                return "";
            try
            {
                WebClient wc = new WebClient();
                wc.Proxy = null;
                JToken result = GetContractState(wc, ChainHash, hash).Result;

                if (result != null && result["message"] == null)
                {
                    List<string> slist = new List<string>();
                    slist.Add(result["hash"].ToString());
                    slist.Add(result["name"].ToString());
                    slist.Add(result["author"].ToString());
                    slist.Add(result["email"].ToString());
                    slist.Add(result["description"].ToString());

                    string supportedStandard = GetSupportedStandards(UInt160.Parse(hash));

                    slist.Add(supportedStandard);

                    contractDict[hash] = supportedStandard;

                    return MysqlConn.InsertSqlBuilder(DataTableName, slist);
                }

            }
            catch (Exception e)
            {
                Program.Log($"error occured when call getcontractstate, chain:{ChainHash}, reason:{e.Message}", Program.LogLevel.Error);
                Thread.Sleep(3000);
                GetContractInfoSql(hash, ref contractDict);
            }

            return "";
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
                Thread.Sleep(3000);
                return await GetContractState(wc, ChainHash, hash);
            }
        }

        public string GetSupportedStandards(UInt160 Contract)
        {
            try
            {
                ScriptBuilder sb = new ScriptBuilder();

                sb.EmitAppCall(Contract, "supportedStandards");

                JObject jObject;

                var result = ZoroHelper.InvokeScript(sb.ToArray(), ChainHash.ToString()).Result;

                jObject = JObject.Parse(result);

                if (jObject["result"]["stack"] != null)
                {
                    JArray jStack = jObject["result"]["stack"] as JArray;

                    if (jStack.Count > 0 && jStack[0]["value"] != null)
                    {
                        return Encoding.UTF8.GetString(Helper.HexString2Bytes(jStack[0]["value"].ToString()));
                    }
                }
                return "";
            }
            catch (Exception e)
            {
                Program.Log($"error occured when GetSupportedStandards contract: {Contract}, reason:{e.Message}", Program.LogLevel.Error);
                Thread.Sleep(3000);
                return GetSupportedStandards(Contract);
            }
        }

    }
}
