using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

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
    /// 新订单
    /// </summary>
    [CallbackDesc("推送订单URL")]
    class CreateOrderCallback:IRequestHandler
    {
        public const string NotifyPageName = "/Jack_Resturant_Callback_CreateOrder";

        public string UrlPageName => NotifyPageName;


        static void handleContent(string orderContent,int erpStoreID)
        {
            var orderJSONObj = orderContent.JsonToObject<JCreateOrder>();
            OrderInfo orderInfo = new OrderInfo();
            int ctime = orderJSONObj.ctime;//订单创建时间
            orderInfo.CreateTime = Helper.UnixToDateTime(ctime);
            orderInfo.DayOrderNumber = orderJSONObj.daySeq;
            orderInfo.Remark = orderJSONObj.caution;

            int deliverytime = orderJSONObj.deliveryTime.GetValueOrDefault();
            if (deliverytime > 0)
            {
                orderInfo.DeliveryTime = Helper.UnixToDateTime(deliverytime);
                orderInfo.DeliveryTimeTag = DeliveryTimeTag.OnTime;
            }
            else
            {
                orderInfo.DeliveryTimeTag = DeliveryTimeTag.ASAP;
            }
            orderInfo.ErpStoreID = erpStoreID;
            orderInfo.HasInvoiced = orderJSONObj.hasInvoiced == 1;
            if (orderInfo.HasInvoiced)
            {
                orderInfo.InvoiceTitle = orderJSONObj.invoiceTitle;
                orderInfo.TaxpayerId = orderJSONObj.taxpayerId;
            }
            orderInfo.ThirdOrderId = orderJSONObj.orderId;
            orderInfo.OriginalPrice = orderJSONObj.originalPrice.GetValueOrDefault();
            orderInfo.PayType = (PayType)orderJSONObj.payType;
            orderInfo.DeliveryAddr = orderJSONObj.recipientAddress;
            orderInfo.CustomerName = orderJSONObj.recipientName;
            orderInfo.Cellphone = orderJSONObj.recipientPhone;
            orderInfo.ShippingFee = orderJSONObj.shippingFee.GetValueOrDefault();
            orderInfo.ReceiptsMoney = orderJSONObj.total.GetValueOrDefault();

            var poiReceiveDetail = orderJSONObj.poiReceiveDetail.JsonToObject<JCreateOrder_PoiReceiveDetail>();
            orderInfo.IncomeMoney = poiReceiveDetail.wmPoiReceiveCent.GetValueOrDefault() / 100.0;

            var detail = orderJSONObj.detail.JsonToObject<JCreateOrder_Detail[]>();
            for (int i = 0; detail != null && i < detail.Length; i++)
            {
                var detailJson = detail[i];
                var newDetail = new OrderInfo.Detail();
                newDetail.BoxNumber = detailJson.box_num;
                newDetail.BoxPrice = detailJson.box_price;
                newDetail.ErpSkuId = detailJson.sku_id;
                newDetail.FoodDiscount = detailJson.food_discount;
                newDetail.FoodName = detailJson.food_name;
                newDetail.FoodProperty = detailJson.food_property;
                newDetail.Price = detailJson.price;
                newDetail.Quantity = detailJson.quantity;
                newDetail.Unit = detailJson.unit;
                newDetail.FoodSpec = detailJson.spec;

                orderInfo.Details.Add(newDetail);
            }
            orderInfo.PackageFee = (from b in orderInfo.Details select b.BoxNumber * b.BoxPrice).Sum();

            try
            {
                foreach( var item in poiReceiveDetail.actOrderChargeByPoi)
                {
                    orderInfo.Comments.Add(item.comment);
                }
            }
            catch
            {

            }

            try
            {
                var extras = orderJSONObj.extras.JsonToObject<JCreateOrder_Extra[]>();
                for (int i = 0; extras != null && i < extras.Length; i++)
                {
                    var extrasJson = extras[i];
                    if (extrasJson.type == 0)
                        continue;

                    var discount = new OrderInfo.DiscountInfo();
                    discount.PlatformCharge = extrasJson.mt_charge;
                    discount.PoiCharge = extrasJson.poi_charge;
                    discount.ReduceFee = extrasJson.reduce_fee;
                    discount.Remark = extrasJson.remark;
                    orderInfo.DiscountInfos.Add(discount);
                }
            }
            catch
            {

            }

            if (ResturantFactory.ResturantListener != null)
            {
                new Thread(() =>
                {
                    try
                    {
                        using (Log log = new Log("美团.CreateOrder", false))
                        {
                            log.Log("调用ResturantFactory.ResturantListener.OnReceiveNewOrder");
                        }
                        ResturantFactory.ResturantListener.OnReceiveNewOrder(ResturantPlatformType.Meituan, orderInfo);
                    }
                    catch (Exception ex)
                    {
                        using (Log log = new Log("美团.CreateOrder", false))
                        {
                            log.Log("调用ResturantFactory.ResturantListener.OnReceiveNewOrder失败,err:" + ex.ToString());
                        }
                    }
                }).Start();
            }
        }

        public TaskStatus Handle(IHttpProxy httpHandler)
        {
            using (Log log = new Log("美团.CreateOrder" , false))
            {
                httpHandler.ResponseContentType = "application/json";
                var forms = httpHandler.Form.ToObject<SortedDictionary<string, string>>();
                log.LogJson(forms);
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

                        handleContent(forms["order"], int.Parse(forms["ePoiId"]));
                    }

                }
                catch (Exception ex)
                {
                    using (Log logErr = new Log("美团CreateOrderCallback解析错误"))
                    {
                        logErr.Log(ex.ToString());
                    }
                    throw ex;
                }
                httpHandler.ResponseWrite("{\"data\":\"OK\"}");

                return TaskStatus.Completed;
            }
        }
    }
    
}
