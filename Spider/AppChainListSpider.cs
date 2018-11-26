﻿using System;
using System.Net;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Zoro.Spider
{
    class AppChainListSpider : IDisposable
    {
        private Task task;
        private WebClient wc = new WebClient();
        private MysqlConn conn;
        private SaveHashlist hashlist;
        private SaveAppChain appchain;
        private List<UInt160> currentList = new List<UInt160>();

        public void Start()
        {
            conn = new MysqlConn(MysqlConn.conf);
            hashlist = new SaveHashlist(conn);
            appchain = new SaveAppChain(conn);
            task = Task.Factory.StartNew(() =>
            {
                Process();
            });
        }

        public void Dispose()
        {
            task.Dispose();
        }

        private List<UInt160> GetAppChainHashList()
        {
            try
            {
                var gethashlistUrl = $"{Settings.Default.RpcUrl}/?jsonrpc=2.0&id=1&method=getappchainhashlist&params=[]";
                var info = wc.DownloadString(gethashlistUrl);
                var json = JObject.Parse(info);
                JToken result = json["result"];

                if (result != null)
                {
                    JToken hlist = result["hashlist"];
                    if (hlist != null && hlist is JArray jlist)
                    {
                        hashlist.Save(result);

                        List<UInt160> list = new List<UInt160>();
                        foreach (var item in jlist)
                        {
                            list.Add(UInt160.Parse(item.ToString()));
                        }
                        return list;
                    }
                }
            }
            catch (Exception)
            {
                Program.Log("error occured when call getappchainhashlist", Program.LogLevel.Error);
            }

            return null;
        }

        private void GetAppChainState(UInt160 chainHash)
        {
            try
            {
                var getstateUrl = $"{Settings.Default.RpcUrl}/?jsonrpc=2.0&id=1&method=getappchainstate&params=['{chainHash}']";
                var info = wc.DownloadString(getstateUrl);
                var json = JObject.Parse(info);
                JToken result = json["result"];
                if (result != null)
                {
                    appchain.Save(result);

                    if (Program.IsMyInterestedChain(result["name"].ToString(), chainHash.ToString(), out int startHeight))
                    {
                        Program.StartChainSpider(chainHash, startHeight);
                    }
                }
            }
            catch (Exception)
            {
                Program.Log($"error occured when call getappchainstate with chainHash ={chainHash}", Program.LogLevel.Error);
            }
        }

        private void Process()
        {
            while (true)
            {
                List<UInt160> list = GetAppChainHashList();

                if (list != null)
                {
                    UInt160[] array = list.Where(p => !currentList.Contains(p)).ToArray();

                    if (array.Length > 0)
                    {
                        currentList.AddRange(array);

                        foreach (var hash in array)
                        {
                            GetAppChainState(hash);
                        }
                    }
                }

                Thread.Sleep(5000);
            }
        }
    }
}
