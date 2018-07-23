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


namespace Jack.Resturant.Impls.Baidu.Callbacks
{
    /// <summary>
    /// 回调Url
    /// </summary>
    [CallbackDesc("回调Url")]
    class Callback:IRequestHandler
    {
        public const string NotifyPageName = "/Jack_Resturant_Baidu_Callback";

        public string UrlPageName => NotifyPageName;

        bool checkSign(SortedDictionary<string, object> forms)
        {
            var config = new Config(ResturantFactory.ResturantListener.OnGetPlatformConfigXml(ResturantPlatformType.Baidu));
            forms["secret"] = config.Secret;
            return BaiduHelper.CheckSign(forms, null);
        }

        public TaskStatus Handle(IHttpProxy httpHandler)
        {
           
            var forms = httpHandler.Form.ToObject<SortedDictionary<string, object>>();
            using (Log log = new Log("Baidu.Callback", false))
            {
                log.LogJson(forms);
            }
            try
            {
                
                if (forms.Count > 0)
                {
                    var signResult = checkSign(forms);

                    if (signResult)
                    {
                        if (forms["cmd"].Equals("order.create"))
                        {
                            var bodyJson = (Newtonsoft.Json.Linq.JToken)Newtonsoft.Json.JsonConvert.DeserializeObject(forms["body"].ToString());
                            var order_id = bodyJson.Value<string>("order_id");
                            var resturant = new BaiduResturant();
                            var orderinfo = resturant.GetOrder(order_id);
                            new Thread(() =>
                            {
                                try
                                {
                                    ResturantFactory.ResturantListener.OnReceiveNewOrder(ResturantPlatformType.Baidu, orderinfo);
                                }
                                catch { }
                            }).Start();

                        }
                        else if (forms["cmd"].Equals("order.status.push"))
                        {
                            var bodyJson = (Newtonsoft.Json.Linq.JToken)Newtonsoft.Json.JsonConvert.DeserializeObject(forms["body"].ToString());
                            var order_id = bodyJson.Value<string>("order_id");
                            var status = bodyJson.Value<int>("status");
                            string reason = null;
                            try
                            {
                                reason = bodyJson.Value<string>("reason");
                            }
                            catch
                            {

                            }

                            new Thread(() =>
                            {
                                try
                                {
                                    if (status == 9)
                                    {
                                        //已完成
                                        try
                                        {
                                            //获取店铺实际到账金额
                                            BaiduResturant resturant = new BaiduResturant();
                                            var info = resturant.GetOrderServiceAmount(null, order_id);
                                            ResturantFactory.ResturantListener.OnReceiveOrderSettlement(ResturantPlatformType.Baidu, order_id, info);
                                        }
                                        catch
                                        {

                                        }
                                        ResturantFactory.ResturantListener.OnOrderFinish(ResturantPlatformType.Baidu, order_id);
                                    }
                                    else if (status == 10)
                                    {
                                        //已取消
                                        ResturantFactory.ResturantListener.OnOrderCancel(ResturantPlatformType.Baidu, new OrderCancelInfo()
                                        {
                                            OrderID = order_id,
                                            Reason = reason
                                        });
                                    }
                                }
                                catch { }
                            }).Start();

                        }
                        else if (forms["cmd"].Equals("shop.unbind.msg"))
                        {
                            //门店解除绑定
                            var bodyJson = (Newtonsoft.Json.Linq.JToken)Newtonsoft.Json.JsonConvert.DeserializeObject(forms["body"].ToString());
                            var shop_list = (Newtonsoft.Json.Linq.JArray)bodyJson["shop_list"];
                            foreach (var shopJson in shop_list)
                            {
                                var erpShopId = shopJson.Value<int>("supplier_id");
                                ResturantFactory.ResturantListener.OnReleaseStoreMapSuccess(ResturantPlatformType.Baidu, erpShopId);
                            }
                        }
                        else if (forms["cmd"].Equals("shop.bind.msg"))
                        {
                            //门店绑定
                            BaiduResturant resturant = new BaiduResturant();
                            var bodyJson = (Newtonsoft.Json.Linq.JToken)Newtonsoft.Json.JsonConvert.DeserializeObject(forms["body"].ToString());
                            var shop_list = (Newtonsoft.Json.Linq.JArray)bodyJson["shop_list"];
                            foreach (var shopJson in shop_list)
                            {
                                var baidu_shop_id = shopJson.Value<string>("baidu_shop_id");
                                var erpShopId = resturant.GetErpShopId(baidu_shop_id);
                                ResturantFactory.ResturantListener.OnStoreMapSuccess(ResturantPlatformType.Baidu, new StoreMapInfo()
                                {
                                    ErpStoreId = int.Parse(erpShopId)
                                });
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("签名校验失败");
                    }
                }
            }

            catch (Exception ex)
            {
                using (Log log = new Log("Baidu_Callback Error "))
                {
                    log.LogJson(forms);
                    log.Log(ex.ToString());
                }
                throw ex;
            }

            httpHandler.ResponseWrite( "OK");
            return TaskStatus.Completed;
        }
    }
    
}
