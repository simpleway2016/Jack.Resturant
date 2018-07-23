using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Jack.Resturant.Impls.Meituan
{
    class MeituanHelper
    {
        public static string Sign(SortedDictionary<string,object> values,string signKey)
        {
            StringBuilder str = new StringBuilder();
            str.Append(signKey);
            foreach (var item in values)
            {
                if (item.Key == "sign")
                    continue;
                str.Append(item.Key);
                str.Append(item.Value);
            }

            using (SHA1 sha1Hash = SHA1.Create())
            {
                var bs = sha1Hash.ComputeHash(Encoding.UTF8.GetBytes(str.ToString()));
                var sb = new StringBuilder();
                foreach (byte b in bs)
                {
                    sb.Append(b.ToString("x2"));
                }
                //所有字符转为大写
                return sb.ToString().ToLower();
            }
        }
        public static string Sign(SortedDictionary<string, string> values, string signKey)
        {
            StringBuilder str = new StringBuilder();
            str.Append(signKey);
            foreach (var item in values)
            {
                if (item.Key == "sign")
                    continue;
                str.Append(item.Key);
                str.Append(item.Value);
            }

            using (SHA1 sha1Hash = SHA1.Create())
            {
                var bs = sha1Hash.ComputeHash(Encoding.UTF8.GetBytes(str.ToString()));
                var sb = new StringBuilder();
                foreach (byte b in bs)
                {
                    sb.Append(b.ToString("x2"));
                }
                //所有字符转为大写
                return sb.ToString().ToLower();
            }
        }
    }
}
