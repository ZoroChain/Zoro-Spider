using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;

namespace Zoro.Spider
{
    class MysqlConn
    {
        public static string conf = "";
        public static string dbname = "";

        public static bool Exist(string tableName)
        {
            string cmdStr = $"select t.table_name from information_schema.TABLES t where t.TABLE_SCHEMA = '{dbname}' and t.TABLE_NAME = '{ tableName }' ";
            using (MySqlConnection conn = new MySqlConnection(conf))
            {
                MySqlCommand cmd = new MySqlCommand(cmdStr, conn);
                conn.Open();
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string name = reader.GetString(0);
                    return true;
                }
            }
            return false;
        }

        public static void CreateTable(string type, string tableName)
        {
            string createSql = "";
            switch (type) {
                case TableType.Block:
                    createSql = "create table "+tableName+" (id bigint(20) primary key auto_increment, hash varchar(255), size varchar(255), version tinyint(3)," +
                " previousblockhash varchar(255), merkleroot varchar(255)," +
                " time int(11), indexx int(11), nonce varchar(255), nextconsensus varchar(255), script varchar(2048), tx longtext)";
                    break;
                case TableType.Address:
                    createSql = "create table "+tableName+" (id int(11) primary key auto_increment, addr varchar(255)," +
                " firstuse varchar(255), lastuse varchar(255), txcount int(11))";
                    break;
                case TableType.Address_tx:
                    createSql = "create table "+tableName+" (id int(11) primary key auto_increment, addr varchar(255)," +
                " txid varchar(255), blockindex int(11), blocktime varchar(255))";
                    break;
                case TableType.Transaction:
                    createSql = "create table "+tableName+" (id int(11) primary key auto_increment, txid varchar(255)," +
                " size int(11), type varchar(45), version tinyint(3), attributes varchar(2048), vin varchar(2048), vout varchar(2048)," +
                " sys_fee int(11), net_fee int(11), scripts varchar(2048), nonce varchar(255), blockheight varchar(45))";
                    break;
                case TableType.Notify:
                    createSql = "create table "+tableName+" (id bigint(20) primary key auto_increment, txid varchar(255), vmstate varchar(255), gas_consumed varchar(255)," +
                " stack varchar(2048), notifications varchar(2048), blockindex int(11))";
                    break;
                case TableType.NEP5Asset:                    
                    createSql = "create table " + tableName + " (id int(11) primary key auto_increment, assetid varchar(45), totalsupply varchar(45)," +
                " name varchar(45), symbol varchar(45), decimals varchar(45))";
                    break;
                case TableType.NEP5Transfer:
                    createSql = "create table " + tableName + " (id bigint(20) primary key auto_increment, blockindex int(11), txid varchar(255)," +
                " n int(11), asset varchar(255), fromx varchar(255), tox varchar(255), value varchar(255))";
                    break;
                case TableType.UTXO:
                    createSql = "create table " + tableName + " (id bigint(20) primary key auto_increment, addr varchar(255), txid varchar(255)," +
                " n int(11), asset varchar(255), value varchar(255), createHeight int(11), used varchar(255), useHeight int(11), claimed varchar(255))";
                    break;
                case TableType.Hash_List:
                    createSql = "create table " + tableName + " (id bigint(20) primary key auto_increment, hashlist varchar(255)";
                    break;
                case TableType.Appchainstate:
                    createSql = "create table " + tableName + " (id bigint(20) primary key auto_increment, version varchar(255), hash varchar(255), name varchar(255)," +
                " owner varchar(255), timestamp varchar(255), seedlist varchar(2048), validators varchar(2048))";
                    break;
            }
            using (MySqlConnection conn = new MySqlConnection(conf))
            {
                conn.Open();
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand(createSql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    Program.Log("建表成功 " + tableName, Program.LogLevel.Info);
                }
                catch (Exception)
                {
                    Program.Log("建表失败 " + tableName, Program.LogLevel.Error);
                }
            }
        }

        public static DataSet ExecuteDataSet(string tableName, Dictionary<string, string> where) {
            using (MySqlConnection conn = new MySqlConnection(conf)) {
                conn.Open();
                string select = "select * from " + tableName;
                if (where.Count != 0) {
                    select += " where";
                }               
                foreach (var dir in where)
                {
                    select += " " + dir.Key + "='" + dir.Value + "'";
                    select += " and";
                }
                select = select.Substring(0, select.Length - 4);
                MySqlDataAdapter adapter = new MySqlDataAdapter(select, conf);
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                return ds;
            }
        }

        public static int ExecuteDataInsert(string tableName, List<string> parameter)
        {
            using (MySqlConnection conn = new MySqlConnection(conf))
            {
                conn.Open();
                string mysql = $"insert into " + tableName + " values (null,";
                foreach (string param in parameter) {
                    mysql += "'" + param + "',";
                }               
                mysql = mysql.Substring(0, mysql.Length - 1);
                mysql += ");";
                MySqlCommand mc = new MySqlCommand(mysql, conn);
                int count = mc.ExecuteNonQuery();
                return count;
            }
        }

        /// <summary>
        /// 插入多条数据
        /// </summary>
        public static void InsertCollection(MySqlConnection connection)
        {
            connection.Open();
            MySqlCommand command = new MySqlCommand();
            command.Connection = connection;

            command.CommandText = "INSERT INTO person VALUES ( null,?name, ?birthday)";
            command.Parameters.Add("?name", MySqlDbType.VarChar);
            command.Parameters.Add("?birthday", MySqlDbType.DateTime);

            for (int x = 0; x < 30; x++)
            {
                command.Parameters[0].Value = "name" + x;
                command.Parameters[1].Value = DateTime.Now;
                command.ExecuteNonQuery();
            }

            command.ExecuteNonQuery();
            connection.Close();
        }

        /// <summary>
        /// 修改数据
        /// </summary>
        public static int Update(string tableName, Dictionary<string, string> dirs, Dictionary<string, string> where)
        {
            using (MySqlConnection conn = new MySqlConnection(conf))
            {
                conn.Open();
                string update = $"update " + tableName + " set ";
                foreach (var dir in dirs)
                {
                    update += dir.Key + "='" + dir.Value + "',";
                }
                update = update.Substring(0, update.Length - 1);
                if (where.Count != 0) 
                    update += " where";
                foreach (var dir in where)
                {
                    update += " " + dir.Key + "='" + dir.Value + "'";
                    update += " and";
                }
                if (where.Count != 0)
                    update = update.Substring(0, update.Length - 4);
                update += ";";
                MySqlCommand command = new MySqlCommand(update, conn);
                int count = command.ExecuteNonQuery();
                conn.Close();
                return count;
            }
        }

        public static uint getHeight(string chainHash) {
            var dir = new Dictionary<string, string>();
            dir.Add("chainhash", chainHash);
            DataTable dt = ExecuteDataSet("chainlistheight", dir).Tables[0];
            if (dt.Rows.Count == 0)
            {
                return 0;
            }
            else {
                return uint.Parse(dt.Rows[0]["chainheight"].ToString());
            }
        }

        public static void SaveAndUpdateHeight(string chainHash, string height)
        {
            var dir = new Dictionary<string, string>();
            dir.Add("chainhash", chainHash);
            DataTable dt = ExecuteDataSet("chainlistheight", dir).Tables[0];
            if (dt.Rows.Count == 0)
            {
                var list = new List<string>();
                list.Add(chainHash);
                list.Add(height);
                ExecuteDataInsert("chainlistheight", list);
            }
            else
            {
                var set = new Dictionary<string, string>();
                set.Add("chainheight", height);
                Update("chainlistheight", set, dir);
            }
        }

        public static void SaveAndUpdataHashList(string table, string hashlist) {
            var dir = new Dictionary<string, string>();
            DataTable dt = ExecuteDataSet(table, dir).Tables[0];
            if (dt.Rows.Count == 0)
            {
                var list = new List<string>();
                list.Add(hashlist);
                ExecuteDataInsert(table, list);
            }
            else {
                var set = new Dictionary<string, string>();
                set.Add("hashlist", hashlist);
                Update(table, set, dir);
            }
        }

        public static void SaveAndUpdataAppChainState(string table, List<string> hashlist)
        {
            var dir = new Dictionary<string, string>();
            dir.Add("hash", hashlist[1]);
            DataTable dt = ExecuteDataSet(table, dir).Tables[0];
            if (dt.Rows.Count == 0)
            {
                ExecuteDataInsert(table, hashlist);
            }
            else
            {
                var set = new Dictionary<string, string>();
                set.Add("version", hashlist[0]);
                set.Add("name", hashlist[2]);
                set.Add("owner", hashlist[3]);
                set.Add("timestamp", hashlist[4]);
                set.Add("seedlist", hashlist[5]);
                set.Add("validators", hashlist[6]);
                Update(table, set, dir);
            }
        }
    }

    class TableType {
        public const string Block = "block";
        public const string Address = "address";
        public const string Address_tx = "address_tx";
        public const string Transaction = "tx";
        public const string Notify = "notify";
        public const string NEP5Asset = "nep5asset";
        public const string NEP5Transfer = "nep5transfer";
        public const string UTXO = "utxo";
        public const string Hash_List = "hashlist";
        public const string Appchainstate = "appchainstate";
    }
}
