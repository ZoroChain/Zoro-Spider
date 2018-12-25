using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Zoro.IO.Json;
using Zoro.Cryptography;

namespace Zoro.Spider
{
    static class Helper
    {
        public static string ZoroNativeNep5Call = "5a6f726f2e4e61746976654e4550352e43616c6c";
        public static string Nep5Call = "5a6f726f2e4e61746976654e4550352e43616c6c";
        public static string ZoroGlobalAssetTransfer = "5a6f726f2e476c6f62616c41737365742e5472616e73666572";

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

        public static string ToAddress(this UInt160 scriptHash)
        {
            byte[] data = new byte[21];
            data[0] = 23;
            Buffer.BlockCopy(scriptHash.ToArray(), 0, data, 1, 20);
            return data.Base58CheckEncode();
        }      
    }
}
