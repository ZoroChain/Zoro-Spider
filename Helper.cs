using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Zoro.Spider
{
    class Helper
    {
        public static string url = "http://127.0.0.1:20332/";
        private static string logfile = "zoro-spider.log";

        public static string Bytes2HexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var d in data)
            {
                sb.Append(d.ToString("x02"));
            }
            return sb.ToString();
        }

        public static byte[] HexString2Bytes(string str)
        {
            if (str.IndexOf("0x") == 0)
                str = str.Substring(2);
            byte[] outd = new byte[str.Length / 2];
            for (var i = 0; i < str.Length / 2; i++)
            {
                outd[i] = byte.Parse(str.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return outd;
        }

        public static async Task<string> HttpPost(string url, byte[] data)
        {
            WebClient wc = new WebClient();
            wc.Headers["content-type"] = "text/plain;charset=UTF-8";
            byte[] retdata = await wc.UploadDataTaskAsync(url, "POST", data);
            return Encoding.UTF8.GetString(retdata);
        }

        public static string MakeRpcUrlPost(string url, string method, out byte[] data, params JObject[] _params)
        {
            var json = new JObject();
            json["id"] = new JObject(1);
            json["jsonrpc"] = new JObject("2.0");
            json["method"] = new JObject(method);
            StringBuilder sb = new StringBuilder();
            var array = new JArray();
            for (var i = 0; i < _params.Length; i++)
            {
                array.Add(_params[i]);
            }
            json["params"] = array;
            data = Encoding.UTF8.GetBytes(json.ToString());
            return url;
        }

        public static void printLog(string ss)
        {
            Console.WriteLine(DateTime.Now + " " + ss);
            using (FileStream fs = new FileStream(logfile, FileMode.Append, FileAccess.Write, FileShare.None))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.WriteLine(DateTime.Now + " " + ss);
            }
        }
    }
}
