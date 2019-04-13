using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Zoro.Spider
{
    class SaveTxScriptMethod : SaveBase
    {
        List<ScriptMethod> scriptMethods = new List<ScriptMethod>();
        private SaveContractState contractState;

        public SaveTxScriptMethod(UInt160 chainHash)
            : base(chainHash)
        {
            InitDataTable(TableType.Tx_Script_Method);
            contractState = new SaveContractState(chainHash);
        }

        public override bool CreateTable(string name)
        {
            MysqlConn.CreateTable(TableType.Tx_Script_Method, name);
            return true;
        }

        public void Save(string script, uint blockHeight, string txid)
        {
            if (script == null) return;
            Dictionary<string, string> selectWhere = new Dictionary<string, string>();
            selectWhere.Add("txid", txid);
            DataTable dt = MysqlConn.ExecuteDataSet(DataTableName, selectWhere).Tables[0];
            if (dt.Rows.Count != 0)
            {
                Dictionary<string, string> where = new Dictionary<string, string>();
                where.Add("txid", txid);
                MysqlConn.Delete(DataTableName, where);
            }
            scriptMethods.Clear();
            Op[] op = Avm2Asm.Trans(script.HexToBytes());
            for (int i = 0; i < op.Length; i++) {
                if (op[i].code == OpCode.APPCALL)
                {
                    string method = Encoding.Default.GetString(op[i - 1].paramData);
                    string contract = op[i].paramData.Reverse().ToHexString();
                    AddMethod(txid, "AppCall", method, contract, blockHeight);
                }
                else if (op[i].code == OpCode.SYSCALL) {
                    string method = Encoding.Default.GetString(op[i - 1].paramData);
                    string contract = Encoding.Default.GetString(op[i].paramData);
                    AddMethod(txid, "SysCall", method, contract, blockHeight);
                }
            }           
        }

        private void AddMethod(string txid, string code, string method, string contract, uint blockHeight) {
            List<string> slist = new List<string>();
            slist.Add(txid);
            slist.Add(code);
            slist.Add(method);
            slist.Add(contract);
            slist.Add(blockHeight.ToString());
            if (contract.Length >= 40)
                contractState.SaveAsync(contract);
            MysqlConn.ExecuteDataInsert(DataTableName, slist);
        }
    }

    class ScriptMethod {
        public string calltype;
        public string method;
        public string contract;
        public ScriptMethod(string calltype, string method, string contract) {
            this.calltype = calltype;
            this.method = method;
            this.contract = contract;
        }
    }
}
