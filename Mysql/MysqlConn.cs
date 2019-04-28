﻿using System;
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
            bool result = false;
            string cmdStr = $"select t.table_name from information_schema.TABLES t where t.TABLE_SCHEMA = '{dbname}' and t.TABLE_NAME = '{ tableName }' ";
            using (MySqlConnection conn = new MySqlConnection(conf))
            {
                MySqlCommand cmd = new MySqlCommand(cmdStr, conn);
                conn.Open();
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    string name = reader.GetString(0);
                    result = true;
                }
                conn.Close();
            }
            return result;
        }

        public static void CreateTable(string type, string tableName)
        {
            string createSql = "";
            switch (type) {
                case TableType.Block:
                    createSql = "create table "+tableName+" (id bigint(20) primary key auto_increment, hash varchar(255), size varchar(255), version tinyint(3)," +
                " previousblockhash varchar(255), merkleroot varchar(255)," +
                " time int(11), indexx int(11), nonce varchar(255), nextconsensus varchar(255), script varchar(2048), tx longtext, txcount varchar(45))";
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
                " size int(11), type varchar(45), version tinyint(3), attributes varchar(2048)," +
                " sys_fee int(11), scripts varchar(2048), nonce varchar(255), blockheight varchar(45), gas_limit varchar(45), gas_price varchar(45), account varchar(255))";
                    break;
                case TableType.Notify:
                    createSql = "create table "+tableName+" (id bigint(20) primary key auto_increment, txid varchar(255), vmstate varchar(255), gas_consumed varchar(255)," +
                " stack varchar(2048), notifications longtext, blockindex int(11))";
                    break;
                case TableType.NEP5Asset:                    
                    createSql = "create table " + tableName + " (id int(11) primary key auto_increment, assetid varchar(150), totalsupply varchar(45)," +
                " name varchar(150), symbol varchar(150), decimals varchar(45))";
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
                    createSql = "create table " + tableName + " (id bigint(20) primary key auto_increment, hashlist longtext)";
                    break;
                case TableType.Appchainstate:
                    createSql = "create table " + tableName + " (id bigint(20) primary key auto_increment, version varchar(255), hash varchar(255), name varchar(255)," +
                " owner varchar(255), timestamp varchar(255), seedlist varchar(2048), validators varchar(2048))";
                    break;
                case TableType.Chainlistheight:
                    createSql = "create table " + tableName + " (id bigint(20) primary key auto_increment, chainhash varchar(255), chainheight varchar(255))";
                    break;
                case TableType.Address_Asset:
                    createSql = "create table " + tableName + " (id bigint(20) primary key auto_increment, addr varchar(255), asset varchar(255), type varchar(255))";
                    break;
                case TableType.Tx_Script_Method:
                    createSql = "create table " + tableName + " (id bigint(20) primary key auto_increment, txid varchar(255), calltype varchar(255), method varchar(255), contract varchar(255), blockheight varchar(255))";
                    break;
                case TableType.Contract_State:
                    createSql = "create table " + tableName + " (id bigint(20) primary key auto_increment, hash varchar(255), name varchar(255), " +
                        "author varchar(255), email varchar(255), description varchar(255), supportstandard varchar(255))";
                    break;
                case TableType.NFT_Address:
                    createSql = "create table " + tableName + " (id bigint(20) primary key auto_increment, contract varchar(255), addr varchar(255), nfttoken varchar(255), properties longtext)";
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
                catch (Exception e)
                {
                    Program.Log($"建表失败 {tableName}, reason:{e.Message}", Program.LogLevel.Fatal);
                    throw e;
                }
                finally
                {                   
                    conn.Close();
                    AlterTable(type, tableName);
                }
            }
        }

        public static void AlterTable(string type, string tableName) {
            string alterSql = "";
            switch (type)
            {
                case TableType.Block:
                    alterSql = "alter table " + tableName + " add index index_name (indexx)";
                    break;
                case TableType.Address:
                    alterSql = "alter table " + tableName + " add index index_name (addr)";
                    break;
                case TableType.Address_tx:
                    alterSql = "alter table " + tableName + " add index index_name (addr)";
                    break;
                case TableType.Transaction:
                    alterSql = "alter table " + tableName + " add index index_name (txid)";
                    break;
                case TableType.Notify:
                    alterSql = "alter table " + tableName + " add index index_name (txid)";
                    break;
                case TableType.NEP5Transfer:
                    alterSql = "alter table " + tableName + " add index index_name (txid)";
                    break;
                case TableType.UTXO:
                    alterSql = "alter table " + tableName + " add index index_name (addr,used)";
                    break;
                case TableType.Address_Asset:
                    alterSql = "alter table " + tableName + " add index index_name (addr,asset)";
                    break;
                case TableType.Tx_Script_Method:
                    alterSql = "alter table " + tableName + " add index index_name (txid,blockheight)";
                    break;
                case TableType.Contract_State:
                    alterSql = "alter table " + tableName + " add index index_name (hash)";
                    break;
                case TableType.NFT_Address:
                    alterSql = "alter table " + tableName + " add index index_name (addr)";
                    break;
                default:
                    return;
            }
            using (MySqlConnection conn = new MySqlConnection(conf))
            {
                conn.Open();
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand(alterSql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    Program.Log("插入索引成功 " + tableName, Program.LogLevel.Info);
                }
                catch (Exception e)
                {
                    Program.Log($"插入索引失败 {tableName}, reason:{e.Message}", Program.LogLevel.Fatal);
                    throw e;
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        public static DataSet ExecuteDataSet(string tableName, Dictionary<string, string> where)
        {
            MySqlConnection conn = new MySqlConnection(conf);

            try
            {
                conn.Open();
                string select = "select * from " + tableName;
                if (where.Count != 0)
                {
                    select += " where";
                }
                foreach (var dir in where)
                {
                    select += " " + dir.Key + "='" + dir.Value + "'";
                    select += " and";
                }
                if (where.Count > 0)
                    select = select.Substring(0, select.Length - 4);
                MySqlDataAdapter adapter = new MySqlDataAdapter(select, conf);
                DataSet ds = new DataSet();
                adapter.Fill(ds);

                return ds;
            }
            catch (Exception e)
            {
                Program.Log($"Error when execute select {tableName}, reason:{e.Message}", Program.LogLevel.Error);
                throw e;
            }
            finally
            {
                conn.Close();
            }
        }

        public static DataSet ExecuteDataSet(string sql)
        {
            MySqlConnection conn = new MySqlConnection(conf);

            try
            {
                conn.Open();
                
                MySqlDataAdapter adapter = new MySqlDataAdapter(sql, conf);
                DataSet ds = new DataSet();
                adapter.Fill(ds);

                return ds;
            }
            catch (Exception e)
            {
                Program.Log($"Error when execute: {sql}, reason:{e.Message}", Program.LogLevel.Error);
                throw e;
            }
            finally
            {
                conn.Close();
            }
        }

        public static bool CheckExist(string sql)
        {
            MySqlConnection conn = new MySqlConnection(conf);
            try
            {
                conn.Open();                

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader reader = cmd.ExecuteReader();
                bool result = reader.Read();                

                return result;
            }
            catch (Exception e)
            {
                Program.Log($"Error when execute: {sql}, reason:{e.Message}", Program.LogLevel.Error);
                throw e;
            }
            finally
            {
                conn.Close();
            }
        }

        public static int ExecuteDataInsert(string tableName, List<string> parameter)
        {
            MySqlConnection conn = new MySqlConnection(conf);

            try
            {
                //DateTime dt = DateTime.Now;

                conn.Open();
                string mysql = $"insert into " + tableName + " values (null,";
                foreach (string param in parameter)
                {
                    mysql += "'" + param + "',";
                }
                mysql = mysql.Substring(0, mysql.Length - 1);
                mysql += ");";
                MySqlCommand mc = new MySqlCommand(mysql, conn);
                int count = mc.ExecuteNonQuery();

                //TimeSpan span = DateTime.Now - dt;
                //Program.Log($"Execute Insert {tableName} time:{span:hh\\:mm\\:ss\\.fff}", Program.LogLevel.Warning);

                return count;
            }
            catch (MySqlException e) {
                Program.Log($"Error when execute insert with {tableName}, reason: {e.Message}", Program.LogLevel.Error);
                conn.Close();
                return ExecuteDataInsert(tableName, parameter);
            }
            catch (Exception e)
            {
                Program.Log($"Error when execute insert with {tableName}, reason: {e.Message}", Program.LogLevel.Error);
                throw e;
            }
            finally
            {
                conn.Close();
            }
        }

        public static int ExecuteDataInsertWithCheck(string tableName, List<string> parameter, Dictionary<string, string> deleteKeys)
        {
            MySqlConnection conn = new MySqlConnection(conf);

            try
            {
                conn.Open();

                string sql = $"delete from " + tableName + "";
                if (deleteKeys.Count != 0)
                    sql += " where";
                foreach (var dir in deleteKeys)
                {
                    sql += " " + dir.Key + "='" + dir.Value + "'";
                    sql += " and";
                }
                if (deleteKeys.Count != 0)
                    sql = sql.Substring(0, sql.Length - 4);
                sql += ";";

                sql += $"insert into " + tableName + " values (null,";
                foreach (string param in parameter)
                {
                    sql += "'" + param + "',";
                }
                sql = sql.Substring(0, sql.Length - 1);
                sql += ");";
                MySqlCommand mc = new MySqlCommand(sql, conn);
                int count = mc.ExecuteNonQuery();

                return count;
            }
            catch (MySqlException e)
            {
                Program.Log($"Error when execute insert with {tableName}, reason: {e.Message}", Program.LogLevel.Error);
                conn.Close();
                return ExecuteDataInsertWithCheck(tableName, parameter, deleteKeys);
            }
            catch (Exception e)
            {
                Program.Log($"Error when execute insert with {tableName}, reason: {e.Message}", Program.LogLevel.Error);
                throw e;
            }
            finally
            {
                conn.Close();
            }
        }

        /// <summary>
        /// 修改数据
        /// </summary>
        public static int Update(string tableName, Dictionary<string, string> dirs, Dictionary<string, string> where)
        {
            MySqlConnection conn = new MySqlConnection(conf);

            try
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
                return count;
            }
            catch(Exception e)
            {
                Program.Log($"Error when execute update with {tableName}, reason: {e.Message}", Program.LogLevel.Error);
                throw e;
            }
            finally
            {
                conn.Close();
            }
        }

        public static void Delete(string tableName, Dictionary<string, string> where)
        {
            MySqlConnection conn = new MySqlConnection(conf);
            try
            {
                conn.Open();
                string delete = $"delete from " + tableName + "";
                if (where.Count != 0)
                    delete += " where";
                foreach (var dir in where)
                {
                    delete += " " + dir.Key + "='" + dir.Value + "'";
                    delete += " and";
                }
                if (where.Count != 0)
                    delete = delete.Substring(0, delete.Length - 4);
                delete += ";";
                MySqlCommand command = new MySqlCommand(delete, conn);
                int count = command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Program.Log($"Error when execute update with {tableName}, reason: {e.Message}", Program.LogLevel.Error);
                throw e;
            }
            finally
            {
                conn.Close();
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
        public const string Chainlistheight = "chainlistheight";
        public const string Address_Asset = "address_asset";
        public const string Tx_Script_Method = "tx_script_method";
        public const string Contract_State = "contract_state";
        public const string NFT_Address = "nft_address";
    }
}
