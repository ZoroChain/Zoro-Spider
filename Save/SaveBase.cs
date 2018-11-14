using System;
using System.Linq;

namespace Zoro.Spider
{
    internal abstract class SaveBase
    {
        public UInt160 ChainHash { get; private set; }

        public string DataTableName { get; private set; }

        public SaveBase(UInt160 chainHash)
        {
            ChainHash = chainHash;
        }

        public bool InitDataTable(string name)
        {
            DataTableName = name;

            if (ChainHash != null)
            {
                DataTableName += "_" + ChainHash.ToArray().Reverse().ToHexString();
            }

            if (!IsTableExisted(DataTableName))
            {
                return CreateTable(DataTableName);
            }
            return true;
        }

        public bool IsTableExisted(string name)
        {
            return false;
        }

        public abstract bool CreateTable(string name);
    }
}
