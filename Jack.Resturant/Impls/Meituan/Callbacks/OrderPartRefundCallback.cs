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
    [CallbackDesc("外卖部分退款URL")]
    class OrderPartRefundCallback : IRequestHandler
    {
        public const string NotifyPageName = "/Jack_Resturant_Callback_OrderPartRefund";

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


                    string notifyType = forms["notifyType"];
                    /*
                     part	发起退款
agree	确认退款
reject	驳回退款
                     */
                    OrderRefundInfo info = new OrderRefundInfo();
                    info.OrderID = forms["orderId"];
                    info.Reason = forms["reason"];
                    info.Money = Convert.ToDouble(forms["money"]);
                    if (ResturantFactory.ResturantListener != null)
                    {
                        new Thread(() =>
                        {
                            try
                            {
                                var foodJsonArr = (Newtonsoft.Json.Linq.JArray)Newtonsoft.Json.JsonConvert.DeserializeObject(forms["food"]);
                                for (int i = 0; i < foodJsonArr.Count; i++)
                                {
                                    var dishinfo = new RefundDishInfo();
                                    dishinfo.DishName = foodJsonArr[i].Value<string>("food_name");
                                    dishinfo.ErpDishId = foodJsonArr[i].Value<string>("app_food_code");
                                    dishinfo.RefundPrice = foodJsonArr[i].Value<double>("refund_price");
                                    dishinfo.Price = foodJsonArr[i].Value<double>("origin_food_price");
                                    dishinfo.Quantity = foodJsonArr[i].Value<int>("count");
                                    info.RefundDishInfos.Add(dishinfo);
                                }
                            }
                            catch
                            {

                            }
                            try
                            {
                                if (notifyType == "part")
                                {
                                    ResturantFactory.ResturantListener.OnOrderRefund(ResturantPlatformType.Meituan, info);
                                }
                                else if (notifyType == "agree")
                                {
                                    ResturantFactory.ResturantListener.OnOrderRefundCompleted(ResturantPlatformType.Meituan, info);
                                }
                            }
                            catch { }
                        }).Start();
                    }

                }

            }
            catch (Exception ex)
            {
                using (Log log = new Log("美团OrderPartRefundCallback解析错误"))
                {
                    log.Log(ex.ToString());
                }
                throw ex;
            }
            httpHandler.ResponseWrite( "{\"data\":\"OK\"}");

            return TaskStatus.Completed;
        }
    }
}
