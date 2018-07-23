using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Security.Cryptography;
using Way.Lib;
using System.Threading;

#if NET46
#else
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Internal;
#endif
using Jack.HttpRequestHandlers;

namespace Jack.Resturant.Impls.Meituan.Callbacks
{
    /// <summary>
    /// 新订单
    /// </summary>
    [CallbackDesc("已完成订单推送回调URL")]
    class OrderFinishCallback:IRequestHandler
    {
        public const string NotifyPageName = "/Jack_Resturant_Callback_OrderFinish";

        public string UrlPageName => NotifyPageName;


        public TaskStatus Handle(IHttpProxy httpHandler)
        {
            httpHandler.ResponseContentType = "application/json";
            var forms = httpHandler.Form.ToObject<SortedDictionary<string, string>>();
            try
            {
                if (forms.Count > 0)
                {
                    //验证sign
                    var config = new Config(ResturantFactory.ResturantListener.OnGetPlatformConfigXml(ResturantPlatformType.Meituan));
                    if (forms["sign"] != MeituanHelper.Sign(forms, config.SignKey))
                    {
                        throw new Exception("签名验证失败");
                    }

                    Newtonsoft.Json.Linq.JObject orderJSONObj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(forms["order"]);
                    string orderid = orderJSONObj.Value<string>("orderId");

                    if (ResturantFactory.ResturantListener != null)
                    {
                        new Thread(() =>
                        {
                            try
                            {
                                ResturantFactory.ResturantListener.OnOrderFinish(ResturantPlatformType.Meituan, orderid);
                            }
                            catch { }
                        }).Start();
                    }
                }

            }
            catch (Exception ex)
            {
                using (Log log = new Log("美团OrderFinishCallback解析错误"))
                {
                    log.Log(ex.ToString());
                }
            }
            httpHandler.ResponseWrite( "{\"data\":\"OK\"}");

            return TaskStatus.Completed;
        }
    }
    
}
