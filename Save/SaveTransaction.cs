using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Zoro.Spider
{
    class SaveTransaction : SaveBase
    {        
        private SaveNotify notify;
        private SaveTxScriptMethod txScriptMethod;
        private SaveContractState contractState;

        public SaveTransaction(UInt160 chainHash)
            : base(chainHash)
        {
            InitDataTable(TableType.Transaction);

            contractState = new SaveContractState(chainHash);
            contractState.InitContractDict();

            notify = new SaveNotify(chainHash);
            txScriptMethod = new SaveTxScriptMethod(chainHash);
        }

        public override bool CreateTable(string name)
        {
            MysqlConn.CreateTable(TableType.Transaction, name);
            return true;
        }

        public string GetTranSqlText(JToken jObject, uint blockHeight, uint blockTime)
        {
            string sql = "";
            List<string> slist = new List<string>();
            slist.Add(jObject["txid"].ToString());
            slist.Add(jObject["size"].ToString());
            slist.Add(jObject["type"].ToString());
            slist.Add(jObject["version"].ToString());
            slist.Add(jObject["attributes"].ToString());
            slist.Add(jObject["sys_fee"].ToString());
            slist.Add(jObject["scripts"].ToString());
            slist.Add(jObject["nonce"].ToString());
            slist.Add(blockHeight.ToString());
            slist.Add(jObject["gas_limit"]?.ToString());
            slist.Add(jObject["gas_price"]?.ToString());
            slist.Add(ZoroHelper.GetAddressFromScriptHash(UInt160.Parse(jObject["account"].ToString())));

            if (jObject["script"] != null)
            {
                sql += GetScriptMethodSql(jObject["script"].ToString(), blockHeight, jObject["txid"].ToString());
            } 

            sql += MysqlConn.InsertSqlBuilder(DataTableName, slist);           

            if (jObject["type"].ToString() == "InvocationTransaction")
            {
                sql += notify.GetNotifySqlText(jObject, blockHeight, blockTime, contractState.ContractDict);                
            }

            return sql;
        }

        public string GetScriptMethodSql(string script, uint blockHeight, string txid)
        {
            string sql = "";
            if (script == null) return "";

            Op[] op = Avm2Asm.Trans(script.HexToBytes());
            for (int i = 0; i < op.Length; i++)
            {
                if (op[i].code == OpCode.APPCALL)
                {
                    string method = Encoding.Default.GetString(op[i - 1].paramData);
                    string contract = op[i].paramData.Reverse().ToHexString();
                    sql += AddMethod(txid, "AppCall", method, contract, blockHeight);
                }
                else if (op[i].code == OpCode.SYSCALL)
                {
                    string method = "";
                    string contract = Encoding.Default.GetString(op[i].paramData);
                    if (contract.IndexOf("Create") != -1)
                    {
                        method = "Create";
                    }
                    else
                    {
                        method = Encoding.Default.GetString(op[i - 1].paramData);
                    }
                    sql += AddMethod(txid, "SysCall", method, contract, blockHeight);
                }
            }
            return sql;
        }

        private string AddMethod(string txid, string code, string method, string contract, uint blockHeight)
        {
            string sql = "";
            if (!contract.StartsWith("Zoro.") && contract.Length >= 40)
            {
                contract = "0x" + contract;
                sql += contractState.GetContractInfoSql(contract, ref contractState.ContractDict);
            }

            List<string> slist = new List<string>();
            slist.Add(txid);
            slist.Add(code);
            slist.Add(method);
            slist.Add(contract);
            slist.Add(blockHeight.ToString());            

            sql += txScriptMethod.GetInsertSql(slist);

            return sql;
        }

        public void ListClear()
        {
            notify.ListClear();
        }
        
    }
}
