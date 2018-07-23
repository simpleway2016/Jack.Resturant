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
    [CallbackDesc("美团用户或客服退款流程URL")]
    class OrderRefundCallback:IRequestHandler
    {
        public const string NotifyPageName = "/Jack_Resturant_Callback_OrderRefund";

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

                    Newtonsoft.Json.Linq.JObject orderJSONObj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(forms["orderRefund"]);
                    string notifyType = orderJSONObj.Value<string>("notifyType");
                    /*
                     apply	发起退款
agree	确认退款
reject	驳回退款
cancelRefund	用户取消退款申请
cancelRefundComplaint	取消退款申诉
                     */
                    OrderRefundInfo info = new OrderRefundInfo();
                    info.OrderID = orderJSONObj.Value<string>("orderId");
                    info.ErpStoreID = Convert.ToInt32(forms["ePoiId"]);
                    info.Reason = orderJSONObj.Value<string>("reason");

                    if (ResturantFactory.ResturantListener != null)
                    {
                        new Thread(() =>
                        {
                            try
                            {
                                if (notifyType == "apply")
                                {
                                    ResturantFactory.ResturantListener.OnOrderRefund(ResturantPlatformType.Meituan, info);
                                }
                                else if (notifyType == "agree")
                                {
                                    ResturantFactory.ResturantListener.OnOrderRefundCompleted(ResturantPlatformType.Meituan, info);
                                }
                                else if (notifyType == "cancelRefund" || notifyType == "cancelRefundComplaint")
                                {
                                    ResturantFactory.ResturantListener.OnCancelOrderRefund(ResturantPlatformType.Meituan, info);
                                }
                            }
                            catch { }
                        }).Start();
                    }

                }

            }
            catch (Exception ex)
            {
                using (Log log = new Log("美团OrderRefundCallback解析错误"))
                {
                    log.Log(ex.ToString());
                }
            }
            httpHandler.ResponseWrite( "{\"data\":\"OK\"}");

            return TaskStatus.Completed;
        }
    }
    
}
