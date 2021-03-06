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
        private SaveHashlist hashlist = new SaveHashlist();
        private SaveAppChain appchain = new SaveAppChain();
        private List<UInt160> currentList = new List<UInt160>();

        public void Start()
        {
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
                wc.Proxy = null;
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
            catch (Exception e)
            {
                Program.Log($"error occured when call getappchainhashlist, reason:{e.Message}", Program.LogLevel.Error);
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
                        JToken seedlist = result["seedlist"];

                        List<string> list = new List<string>();
                        foreach(var seed in seedlist)
                        {
                            list.Add(seed.ToString());
                        }

                        if (Program.CheckSeedList(list.ToArray()))
                        {
                            Program.StartChainSpider(chainHash, startHeight);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Program.Log($"error occured when call getappchainstate with chainHash ={chainHash}, reason:{e.Message}", Program.LogLevel.Error);
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
