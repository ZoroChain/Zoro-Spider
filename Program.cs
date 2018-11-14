using System;

namespace Zoro.Spider
{
    class Program
    {
        static void Main(string[] args)
        {
            ProjectInfo.head();

            MysqlConn.conf = Settings.Default.MysqlConfig;

            // 开始抓取根链的数据
            StartChainSpider(UInt160.Zero);
            StartAppChainListSpider();

            ProjectInfo.tail();

            while (true)
            {
                System.Threading.Thread.Sleep(1000);
            }
        }

        static void StartChainSpider(UInt160 chainHash)
        {
            ChainSpider spider = new ChainSpider(chainHash);
            spider.Start();
        }

        static void StartAppChainListSpider()
        {
            AppChainListSpider listSpider = new AppChainListSpider();
            listSpider.Start();
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
