using Newtonsoft.Json.Linq;
using System.Data;

namespace Zoro.Spider
{
    class SaveAppChain : SaveBase
    {
        public SaveAppChain()
            : base(null)
        {
            InitDataTable("appchain");
        }

        public override bool CreateTable(string name)
        {
            return true;
        }

        public void Save(JToken jObject)
        {
        }
    }
}
