using System;
using System.Collections.Generic;
using System.Text;


using System.Xml.Linq;
using System.Xml.XPath;
using System.Security.Cryptography;
using Way.Lib;
using Jack.HttpRequestHandlers;
using System.Threading;

#if NET46
#else
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Internal;
#endif


namespace Jack.Resturant.Impls.Meituan.Callbacks
{
    /// <summary>
    /// 
    /// </summary>
    [CallbackDesc("美团用户或客服取消订单URL")]
    class CancelOrderCallback:IRequestHandler
    {
        public const string NotifyPageName = "/Jack_Resturant_Callback_CancelOrder";

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

                    OrderCancelInfo info = new OrderCancelInfo();
                    info.ErpStoreID = int.Parse(forms["ePoiId"]);
                    var orderCancelObj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(forms["orderCancel"]);
                    info.OrderID = orderCancelObj.Value<string>("orderId");
                    info.Reason = orderCancelObj.Value<string>("reason");

                    new Thread(() =>
                    {
                        try
                        {
                            ResturantFactory.ResturantListener.OnOrderCancel(ResturantPlatformType.Meituan, info);
                        }
                        catch { }
                    }).Start();

                }
            }
            catch (Exception ex)
            {
                using (Log log = new Log("美团CancelOrderCallback解析错误"))
                {
                    log.Log(ex.ToString());
                }
            }
            httpHandler.ResponseWrite( "{\"data\":\"OK\"}");
            return TaskStatus.Completed;
        }
    }
    
}
