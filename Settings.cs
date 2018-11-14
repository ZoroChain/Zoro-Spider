using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Zoro.Spider
{
    internal class Settings
    {
        public string MysqlConfig { get; }
        public string RpcUrl { get; }

        public static Settings Default { get; }

        static Settings()
        {
            IConfigurationSection section = new ConfigurationBuilder().AddJsonFile("config.json").Build().GetSection("ApplicationConfiguration");
            Default = new Settings(section);
        }

        public Settings(IConfigurationSection section)
        {
            IEnumerable<IConfigurationSection> mysql = section.GetSection("MySql").GetChildren();

            this.MysqlConfig = "";

            foreach (var item in mysql)
            {
                this.MysqlConfig += item.Key + " = " + item.Value;
                this.MysqlConfig += ";";
            }

            RpcUrl = section.GetSection("RPC").GetSection("url").Value;
        }
    }
}
