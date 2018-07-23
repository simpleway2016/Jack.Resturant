using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Jack.Resturant.Impls.Ele
{
    class EleHelper
    {
        public static bool CheckSign(Newtonsoft.Json.Linq.JObject jsonObj,Config config)
        {
            SortedDictionary<string, object> dict = new SortedDictionary<string, object>();
            foreach (var jsonItem in jsonObj)
            {
                if (jsonItem.Key != "signature")
                {
                    dict[jsonItem.Key] = jsonObj[jsonItem.Key];
                }
            }
            StringBuilder signContent = new StringBuilder();
            foreach (var keyItem in dict)
            {
                signContent.Append(keyItem.Key);
                signContent.Append('=');
                signContent.Append(keyItem.Value.ToString());
            }
            signContent.Append(config.Secret);

            using (MD5 md5hash = MD5.Create())
            {
                var bs = md5hash.ComputeHash(Encoding.UTF8.GetBytes(signContent.ToString()));
                var sb = new StringBuilder();
                foreach (byte b in bs)
                {
                    sb.Append(b.ToString("x2"));
                }
                //所有字符转为大写
                var myresult = sb.ToString().ToUpper();
                return myresult == jsonObj.Value<string>("signature");
            }
        }
        public static string Sign(Config config , string token,string action , IDictionary<string, object> dict)
        {
            IDictionary<string, object> metas = (IDictionary<string, object>)dict["metas"];
            IDictionary<string, object> _params = (IDictionary<string, object>)dict["params"];
            SortedDictionary<string, object> signDict = new SortedDictionary<string, object>();
            foreach( var item in metas )
            {
                signDict[item.Key] = item.Value;
            }
            foreach (var item in _params)
            {
                signDict[item.Key] = item.Value;
            }
            StringBuilder signContent = new StringBuilder();
            signContent.Append(action);
            signContent.Append(token);
            foreach (var item in signDict)
            {
                signContent.Append(item.Key);
                signContent.Append('=');
                signContent.Append( Newtonsoft.Json.JsonConvert.SerializeObject(item.Value) );
            }
            signContent.Append(config.Secret);

            using (MD5 md5hash = MD5.Create())
            {
                var bs = md5hash.ComputeHash(Encoding.UTF8.GetBytes(signContent.ToString()));
                var sb = new StringBuilder();
                foreach (byte b in bs)
                {
                    sb.Append(b.ToString("x2"));
                }
                //所有字符转为大写
                return sb.ToString().ToUpper();
            }
        }
    }
}
