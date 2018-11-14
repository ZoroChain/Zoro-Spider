using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;

namespace Zoro.Spider
{
    class MysqlConn
    {
        public static string conf = "";

        public static void CreateTable(string createSql)
        {
            //string createTableSql_block = "create table block (id bigint(20) primary key auto_increment, hash varchar(255), size varchar(255), version tinyint(3)," +
            //    " previousblockhash varchar(255), merkleroot varchar(255)," +
            //    " time int(11), indexx int(11), nonce varchar(255), nextconsensus varchar(255), script varchar(2048), tx longtext)";
            //string createTableSql_address = "create table address (id int(11) primary key auto_increment, addr varchar(255)," +
            //    " firstuse varchar(255), lastuse varchar(255), txcount int(11))";
            string createTableSql_address_tx = "";
            using (MySqlConnection conn = new MySqlConnection(conf))
            {
                conn.Open();
                try
                {
                    using (MySqlCommand cmd = new MySqlCommand(createSql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    Console.WriteLine("建表成功");
                }
                catch (Exception)
                {
                    Console.WriteLine("建表失败");
                }
            }
        }

        public static DataSet ExecuteDataSet(string tableName, Dictionary<string, string> where) {
            using (MySqlConnection conn = new MySqlConnection(conf)) {
                conn.Open();
                string select = "select * from " + tableName + " where";
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
                update += " where";
                foreach (var dir in where)
                {
                    update += " " + dir.Key + "='" + dir.Value + "'";
                    update += " and";
                }
                update = update.Substring(0, update.Length - 4);
                update += ";";
                MySqlCommand command = new MySqlCommand(update, conn);
                int count = command.ExecuteNonQuery();
                conn.Close();
                return count;
            }
        }
    }
}
