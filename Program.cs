using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text.RegularExpressions;

namespace Zoro.Spider
{
    class Program
    {
        public enum LogLevel : byte
        {
            Fatal,
            Error,
            Warning,
            Info,
            Debug
        }

        private static LogLevel logLevel = LogLevel.Warning;
        private static object logLock = new object();
        public static string Config = "config.json";

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            using (FileStream fs = new FileStream("error.log", FileMode.Create, FileAccess.Write, FileShare.None))
            using (StreamWriter w = new StreamWriter(fs))
                if (e.ExceptionObject is Exception ex)
                {
                    PrintErrorLogs(w, ex);
                }
                else
                {
                    w.WriteLine(e.ExceptionObject.GetType());
                    w.WriteLine(e.ExceptionObject);
                }
        }

        private static void PrintErrorLogs(StreamWriter writer, Exception ex)
        {
            writer.WriteLine(ex.GetType());
            writer.WriteLine(ex.Message);
            writer.WriteLine(ex.StackTrace);
            if (ex is AggregateException ex2)
            {
                foreach (Exception inner in ex2.InnerExceptions)
                {
                    writer.WriteLine();
                    PrintErrorLogs(writer, inner);
                }
            }
            else if (ex.InnerException != null)
            {
                writer.WriteLine();
                PrintErrorLogs(writer, ex.InnerException);
            }
        }

        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                switch (args[0])
                {
                    case "--test":
                        Config = "config.testnet.json";
                        break;
                    case "--main":
                        Config = "config.mainnet.json";
                        break;
                    default:
                        Config = "config.json";
                        break;
                }
            }
            //C#为了使用并发
            System.Net.ServicePointManager.DefaultConnectionLimit = 512;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            ProjectInfo.head();
            
            MysqlConn.conf = Settings.Default.MysqlConfig;
            MysqlConn.dbname = Settings.Default.DataBaseName;

            // 开始抓取根链的数据
            if (IsMyInterestedChain("Root", "", out int startHeight))
            {
                StartChainSpider(UInt160.Zero, startHeight);
            }

            StartAppChainListSpider();

            ProjectInfo.tail();

            while (true)
            {
                System.Threading.Thread.Sleep(1000);
            }
        }

        public static bool IsMyInterestedChain(string name, string hash, out int startHeight)
        {
            foreach (var chain in Settings.Default.ChainSettings)
            {
                if ((chain.Hash.Length > 0 && chain.Hash == hash) || IsInterestedName(chain.Name, name))
                {
                    startHeight = chain.StartHeight;
                    return true;
                }
            }

            startHeight = 0;
            return false;
        }

        public static bool IsInterestedName(string chainName, string name)
        {
            if (chainName.Length > 0 && chainName == name)
                return true;

            if (chainName.Contains("+"))
            {
                Regex reg = new Regex(@chainName);

                bool IsMatch = reg.IsMatch(name);
                return IsMatch;
            }

            return false;
        }

        public static bool CheckSeedList(string[] seedlist)
        {
            foreach (string hostAndPort in seedlist)
            {
                string[] p = hostAndPort.Split(':');
                if (p.Length < 2)
                    return false;

                IPEndPoint seed;
                try
                {
                    seed = GetIPEndpointFromHostPort(p[0], int.Parse(p[1]));
                }
                catch (AggregateException)
                {
                    return false;
                }
            }

            return true;
        }

        public static IPEndPoint GetIPEndpointFromHostPort(string hostNameOrAddress, int port)
        {
            if (IPAddress.TryParse(hostNameOrAddress, out IPAddress ipAddress))
                return new IPEndPoint(ipAddress, port);
            IPHostEntry entry;
            try
            {
                entry = Dns.GetHostEntry(hostNameOrAddress);
            }
            catch (SocketException)
            {
                return null;
            }
            ipAddress = entry.AddressList.FirstOrDefault(p => p.AddressFamily == AddressFamily.InterNetwork || p.IsIPv6Teredo);
            if (ipAddress == null) return null;
            return new IPEndPoint(ipAddress, port);
        }

        public static void StartChainSpider(UInt160 chainHash, int startHeight)
        {
            ChainSpider spider = new ChainSpider(chainHash);
            spider.Start(startHeight);
        }

        static void StartAppChainListSpider()
        {
            AppChainListSpider listSpider = new AppChainListSpider();
            listSpider.Start();
        }

        public static void SetLogLevel(LogLevel lv)
        {
            logLevel = lv;
        }

        public static void Log(string message, LogLevel lv, string dir = null)
        {
            if (lv <= logLevel)
            {
                DateTime now = DateTime.Now;
                string line = $"[{now.TimeOfDay:hh\\:mm\\:ss\\.fff}] {message}";
                Console.WriteLine(line);
                string log_dictionary = dir != null ? $"Logs/{dir}" : $"Logs/default";
                string path = Path.Combine(log_dictionary, $"{now:yyyy-MM-dd}.log");
                lock (logLock)
                {
                    Directory.CreateDirectory(log_dictionary);
                    File.AppendAllLines(path, new[] { line });
                }
            }
        }
    }

    class ProjectInfo
    {
        static private string appName = "Zoro-Spider";
        public static void head()
        {
            string[] info = new string[] {
                "*** Start to run "+appName,
                "*** Auth:lz",
                "*** Version:0.0.0.1",
                "*** CreateDate:2018-10-25",
                "*** LastModify:2018-11-14"
            };
            foreach (string ss in info)
            {
                log(ss);
            }
            //LogHelper.printHeader(info);
        }
        public static void tail()
        {
            log("Program." + appName + " exit");
        }

        static void log(string ss)
        {
            Console.WriteLine(DateTime.Now + " " + ss);
        }
    }
}
