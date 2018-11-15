using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Zoro.Spider
{
    internal class Settings
    {
        public string MysqlConfig { get; }
        public string DataBaseName { get; }
        public string RpcUrl { get; }
        public int Restart { get; }

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

            DataBaseName = section.GetSection("MySql").GetSection("database").Value;
            RpcUrl = section.GetSection("RPC").GetSection("url").Value;
            Restart = GetValueOrDefault(section.GetSection("Restart"), 0, p => int.Parse(p));
        }

        public T GetValueOrDefault<T>(IConfigurationSection section, T defaultValue, Func<string, T> selector)
        {
            if (section.Value == null) return defaultValue;
            return selector(section.Value);
        }
    }
}
