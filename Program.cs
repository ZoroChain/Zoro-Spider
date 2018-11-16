using System;
using System.IO;

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

        private static LogLevel logLevel = LogLevel.Info;
        private static object logLock = new object();

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
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            ProjectInfo.head();

            MysqlConn.conf = Settings.Default.MysqlConfig;
            MysqlConn.dbname = Settings.Default.DataBaseName;

            // 开始抓取根链的数据
            StartChainSpider(UInt160.Zero);
            StartAppChainListSpider();

            ProjectInfo.tail();

            while (true)
            {
                System.Threading.Thread.Sleep(1000);
            }
        }

        public static void StartChainSpider(UInt160 chainHash)
        {
            Log($"Starting chain spider {chainHash}", LogLevel.Info);

            ChainSpider spider = new ChainSpider(chainHash);
            spider.Start();
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

        public static void Log(string message, LogLevel lv)
        {
            if (lv <= logLevel)
            {
                DateTime now = DateTime.Now;
                string line = $"[{now.TimeOfDay:hh\\:mm\\:ss\\.fff}] {message}";
                Console.WriteLine(line);
                string path = $"zoro-spider_{now:yyyy-MM-dd}.log";
                lock(logLock)
                {
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
