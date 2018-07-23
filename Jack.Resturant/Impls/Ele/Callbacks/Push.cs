using System;
using System.Collections.Generic;
using System.Text;

using System.Xml.Linq;
using System.Xml.XPath;
using System.Security.Cryptography;
using System.IO;
using Way.Lib;
using System.Threading;

#if NET46
#else
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Internal;
#endif
using Jack.HttpRequestHandlers;

namespace Jack.Resturant.Impls.Ele.Callbacks
{
    /// <summary>
    /// 推送Url
    /// </summary>
    [CallbackDesc("推送Url")]
    class Push : IRequestHandler
    {
        public const string NotifyPageName = "/Jack_Resturant_ELE_Push";

        public string UrlPageName => NotifyPageName;

        public TaskStatus Handle(IHttpProxy httpHandler)
        {
            using (Log logPush = new Log("饿了么.Push", false))
            {
                httpHandler.ResponseContentType = "application/json";
                string postContent = httpHandler.ReadRequestBody();
                logPush.Log(postContent);

                Newtonsoft.Json.Linq.JObject jsonObj = postContent.JsonToObject<Newtonsoft.Json.Linq.JObject>();

                Config config = null;
                try
                {
                    //签名验证
                    config = new Config(ResturantFactory.ResturantListener.OnGetPlatformConfigXml(ResturantPlatformType.Ele));
                    if (EleHelper.CheckSign(jsonObj, config) == false)
                        throw new Exception("签名错误");

                    int type = jsonObj.Value<int>("type");
                    System.Diagnostics.Debug.WriteLine($"饿了么 callback type:{type}");
                    if (type == 10)
                    {
                        //新订单
                        new Thread(() =>
                        {
                            try
                            {
                                handleNewOrder(jsonObj);
                            }
                            catch (Exception ex)
                            {
                                using (Log log = new Log("handleNewOrder error"))
                                {
                                    log.Log(ex.ToString());
                                    log.Log("post content:{0}", postContent);
                                }
                            }
                        }).Start();
                    }
                    else if (type == 14)
                    {
                        //订单取消
                        new Thread(() =>
                        {
                            try
                            {
                                handleCancelOrder(jsonObj);
                            }
                            catch (Exception ex)
                            {
                                using (Log log = new Log("handleCancelOrder error"))
                                {
                                    log.Log(ex.ToString());
                                    log.Log("post content:{0}", postContent);
                                }
                            }
                        }).Start();
                    }
                    else if (type == 18)
                    {
                        //订单完结
                        new Thread(() =>
                        {
                            try
                            {
                                handleOrderFinish(jsonObj);
                            }
                            catch (Exception ex)
                            {
                                using (Log log = new Log("handleOrderFinish error"))
                                {
                                    log.Log(ex.ToString());
                                    log.Log("post content:{0}", postContent);
                                }
                            }
                        }).Start();
                    }
                    else if (type == 20)
                    {
                        //用户申请取消订单
                        new Thread(() =>
                        {
                            try
                            {
                                handleOrderToCancel(jsonObj);
                            }
                            catch (Exception ex)
                            {
                                using (Log log = new Log("handleOrderToCancel error"))
                                {
                                    log.Log(ex.ToString());
                                    log.Log("post content:{0}", postContent);
                                }
                            }
                        }).Start();
                    }
                    else if (type == 21 || type == 31)
                    {
                        //用户取消退单
                        new Thread(() =>
                        {
                            try
                            {
                                handleCancelOrderRefund(jsonObj);
                            }
                            catch (Exception ex)
                            {
                                using (Log log = new Log("handleCancelOrderRefund error"))
                                {
                                    log.Log(ex.ToString());
                                    log.Log("post content:{0}", postContent);
                                }
                            }
                        }).Start();
                    }
                    else if (type == 30)
                    {
                        //用户申请退单
                        new Thread(() =>
                        {
                            try
                            {
                                handleOrderRefund(jsonObj);
                            }
                            catch (Exception ex)
                            {
                                using (Log log = new Log("handleOrderRefund error"))
                                {
                                    log.Log(ex.ToString());
                                    log.Log("post content:{0}", postContent);
                                }
                            }
                        }).Start();
                    }
                    else if (type == 33)
                    {
                        //用户同意退单
                        new Thread(() =>
                        {
                            try
                            {
                                handleOrderAgreeRefund(jsonObj);
                            }
                            catch (Exception ex)
                            {
                                using (Log log = new Log("handleOrderAgreeRefund error"))
                                {
                                    log.Log(ex.ToString());
                                    log.Log("post content:{0}", postContent);
                                }
                            }
                        }).Start();
                    }
                }
                catch (Exception ex)
                {
                    using (Log log = new Log("饿了么回调解析错误"))
                    {
                        log.Log(ex.ToString());
                        log.Log("post content:{0}", postContent);
                        if (jsonObj != null)
                        {
                            log.LogJson(jsonObj);
                        }
                        if (config != null)
                        {
                            log.LogJson(config);
                        }
                    }
                    throw ex;
                }
                httpHandler.ResponseWrite("{\"message\": \"ok\"}");

                return TaskStatus.Completed;
            }
        }

        /// <summary>
        /// 用户取消退单
        /// </summary>
        /// <param name="jsonObj"></param>
        static void handleCancelOrderRefund(Newtonsoft.Json.Linq.JObject jsonObj)
        {
            var message = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(jsonObj.Value<string>("message"));
            var orderid = message.Value<string>("orderId");
            string refundStatus = message.Value<string>("refundStatus");
            string reason = message.Value<string>("reason");
            if (refundStatus == "failed")
            {
                ResturantFactory.ResturantListener.OnCancelOrderRefund(ResturantPlatformType.Ele, new OrderRefundInfo()
                {
                    OrderID = orderid,
                    Reason = reason
                });
            }
        }

        /// <summary>
        /// 用户申请退款
        /// </summary>
        /// <param name="jsonObj"></param>
        static void handleOrderRefund(Newtonsoft.Json.Linq.JObject jsonObj)
        {
            var message = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(jsonObj.Value<string>("message"));
            var orderid = message.Value<string>("orderId");
            string reason = message.Value<string>("reason");
            //退款金额
            double totalPrice = message.Value<double>("totalPrice");
            var refundInfo = new OrderRefundInfo()
            {
                OrderID = orderid,
                Reason = reason,
                Money = totalPrice
            };


            try
            {
                var goodsList = (Newtonsoft.Json.Linq.JArray)message["goodsList"];
                for (int i = 0; i < goodsList.Count; i++)
                {
                    var good = goodsList[i];
                    RefundDishInfo info = new RefundDishInfo();
                    info.DishName = good.Value<string>("name");
                    info.Quantity = good.Value<int>("quantity");
                    info.Price = good.Value<double>("price");
                    info.RefundPrice = good.Value<double>("price");
                    refundInfo.RefundDishInfos.Add(info);
                }
            }
            catch
            {

            }
            ResturantFactory.ResturantListener.OnOrderRefund(ResturantPlatformType.Ele, refundInfo);
        }
        static void handleOrderAgreeRefund(Newtonsoft.Json.Linq.JObject jsonObj)
        {
            var message = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(jsonObj.Value<string>("message"));
            var orderid = message.Value<string>("orderId");
            string reason = message.Value<string>("reason");

            var refundInfo = new OrderRefundInfo()
            {
                OrderID = orderid,
                Reason = reason,
                Money = message.Value<double?>("totalPrice")
            };


            try
            {
                var goodsList = (Newtonsoft.Json.Linq.JArray)message["goodsList"];
                for (int i = 0; i < goodsList.Count; i++)
                {
                    var good = goodsList[i];
                    RefundDishInfo info = new RefundDishInfo();
                    info.DishName = good.Value<string>("name");
                    info.Quantity = good.Value<int>("quantity");
                    info.Price = good.Value<double>("price");
                    info.RefundPrice = good.Value<double>("price");
                    refundInfo.RefundDishInfos.Add(info);
                }
            }
            catch
            {

            }
            ResturantFactory.ResturantListener.OnOrderRefundCompleted(ResturantPlatformType.Ele, refundInfo);
        }
        static void handleOrderFinish(Newtonsoft.Json.Linq.JObject jsonObj)
        {
            if (jsonObj == null)
                throw new Exception("jsonObj is null");
            var message = jsonObj.Value<string>("message").JsonToObject<JSettlement>();

            if (message == null)
                throw new Exception("message is null");

            var orderid = message.orderId;
            string state = message.state;


            try
            {
                //获取订单信息
                var eleRestuant = new EleResturant();
                var token = ResturantFactory.ResturantListener.OnGetPlatformToken(ResturantPlatformType.Ele, orderid);
                if (string.IsNullOrEmpty(token))
                {
                    throw new Exception($"OnGetPlatformToken无法获取到有效的token，orderId:{orderid}");
                }
                var info = eleRestuant.GetOrderServiceAmount(token, orderid);
                ResturantFactory.ResturantListener.OnReceiveOrderSettlement(ResturantPlatformType.Ele, orderid, info);
            }
            catch (Exception ex)
            {
                using (Log log = new Log("饿了么处理结算信息发生错误"))
                {
                    log.Log(ex.ToString());
                    log.Log(jsonObj.ToString());
                }
            }

            ResturantFactory.ResturantListener.OnOrderFinish(ResturantPlatformType.Ele, orderid);
        }

        /// <summary>
        /// 用户申请取消订单
        /// </summary>
        /// <param name="jsonObj"></param>
        static void handleOrderToCancel(Newtonsoft.Json.Linq.JObject jsonObj)
        {

        }

        /// <summary>
        /// 处理取消的订单
        /// </summary>
        /// <param name="jsonObj"></param>
        static void handleCancelOrder(Newtonsoft.Json.Linq.JObject jsonObj)
        {
            var message = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(jsonObj.Value<string>("message"));
            var orderid = message.Value<string>("orderId");
            ResturantFactory.ResturantListener.OnOrderCancel(ResturantPlatformType.Ele, new OrderCancelInfo()
            {
                OrderID = orderid
            });
        }

        /// <summary>
        /// 处理新订单
        /// </summary>
        /// <param name="jsonObj"></param>
        static void handleNewOrder(Newtonsoft.Json.Linq.JObject jsonObj)
        {
            var message = jsonObj.Value<string>("message").JsonToObject<JOrder>();
            OrderInfo newOrder = new OrderInfo();
            newOrder.ThirdOrderId = message.orderId;
            newOrder.ErpStoreID = message.openId;


            newOrder.PayType = message.onlinePaid ? PayType.PayOnline : PayType.PayOnDelivery;
            newOrder.CreateTime = message.createdAt;
            try
            {
                if (message.deliverTime != null)
                {
                    newOrder.DeliveryTime = message.deliverTime;
                    newOrder.DeliveryTimeTag = DeliveryTimeTag.OnTime;
                }
                else
                {
                    newOrder.DeliveryTimeTag = DeliveryTimeTag.ASAP;
                }
            }
            catch
            {
                newOrder.DeliveryTimeTag = DeliveryTimeTag.ASAP;
            }
            newOrder.InvoiceTitle = message.invoice;
            newOrder.HasInvoiced = !string.IsNullOrEmpty(newOrder.InvoiceTitle);
            newOrder.OriginalPrice = message.originalPrice.GetValueOrDefault();
            newOrder.DayOrderNumber = message.daySn.ToString();
            newOrder.Remark = message.description;
            newOrder.PackageFee = message.packageFee.GetValueOrDefault();
            newOrder.ShippingFee = message.deliverFee.GetValueOrDefault() - message.vipDeliveryFeeDiscount.GetValueOrDefault();
            newOrder.TaxpayerId = message.taxpayerId;
            newOrder.ReceiptsMoney = message.totalPrice.GetValueOrDefault();
            newOrder.IncomeMoney = message.income;
            newOrder.DeliveryAddr = message.deliveryPoiAddress;
            newOrder.CustomerName = message.consignee;
            newOrder.Cellphone = "";
            var phoneArr = message.phoneList ?? new string[0];

            foreach (var item in phoneArr)
            {
                if (newOrder.Cellphone.Length > 0)
                    newOrder.Cellphone += ",";
                newOrder.Cellphone += item.ToString();
            }
            var jsonGroupArr = message.groups ?? new JOrder_Groups[0];
            foreach (var jsonGroupItem in jsonGroupArr)
            {
                var jsonItemsArr = jsonGroupItem.items ?? new JOrder_Groups_Items[0];
                foreach (var foodJson in jsonItemsArr)
                {
                    var newSpecs = foodJson.newSpecs ?? new JOrder_Groups_Items_NewSpecs[0];
                    var attributes = foodJson.attributes ?? new JOrder_Groups_Items_Attributes[0];

                    OrderInfo.Detail detail = new OrderInfo.Detail();
                    detail.BoxNumber = 0;
                    detail.BoxPrice = 0;
                    detail.FoodName = foodJson.name;
                    detail.ErpSkuId = foodJson.extendCode;
                    foreach (var item in newSpecs)
                    {
                        if (detail.FoodSpec.Length > 0)
                            detail.FoodSpec += ",";
                        detail.FoodSpec += item.value;
                    }
                    foreach (var item in attributes)
                    {
                        if (detail.FoodProperty.Length > 0)
                            detail.FoodProperty += ",";
                        detail.FoodProperty += item.value;
                    }
                    detail.Price = foodJson.price.GetValueOrDefault();
                    detail.Quantity = foodJson.quantity;
                    if (jsonGroupItem.type == "normal")
                    {
                        //菜品
                       
                    }
                    else if (jsonGroupItem.type == "discount")
                    {
                        //赠品
                        detail.FoodName = $"[赠品]{detail.FoodName}";

                    }
                    else if (jsonGroupItem.type == "extra")
                    {
                        //赠品
                        detail.FoodName = $"[其他费用]{detail.FoodName}";
                    }

                    newOrder.Details.Add(detail);
                }
            }

            if (ResturantFactory.ResturantListener != null)
            {
                ResturantFactory.ResturantListener.OnReceiveNewOrder(ResturantPlatformType.Ele, newOrder);

            }

        }


    }

}
