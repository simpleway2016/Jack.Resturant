using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Jack.Resturant.Impls.Baidu
{
    class BaiduHelper
    {
        public static bool CheckSign(SortedDictionary<string, object> dict,string body)
        {
            StringBuilder content = new StringBuilder();
            foreach (var item in dict)
            {
                if (item.Key == "sign")
                    continue;
                if (content.Length > 0)
                    content.Append('&');
                content.Append(item.Key);
                content.Append('=');
                if (item.Key == "body" && body != null)
                {
                    content.Append( body);
                }
                else
                {
                    content.Append(item.Value);
                }
            }
            using (MD5 md5hash = MD5.Create())
            {
                var bs = md5hash.ComputeHash(Encoding.UTF8.GetBytes(content.ToString()));
                var sb = new StringBuilder();
                foreach (byte b in bs)
                {
                    sb.Append(b.ToString("x2"));
                }
                return dict["sign"].ToString() == sb.ToString().ToUpper();
            }
        }
        public static string Sign(SortedDictionary<string,string> dict)
        {
            StringBuilder content = new StringBuilder();
            foreach (var item in dict)
            {
                if (content.Length > 0)
                    content.Append('&');
                content.Append(item.Key);
                content.Append('=');
                content.Append(item.Value);
            }
            dict.Remove("secret");
            using (MD5 md5hash = MD5.Create())
            {
                var bs = md5hash.ComputeHash(Encoding.UTF8.GetBytes(content.ToString()));
                var sb = new StringBuilder();
                foreach (byte b in bs)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString().ToUpper();
            }
        }
    }
}
