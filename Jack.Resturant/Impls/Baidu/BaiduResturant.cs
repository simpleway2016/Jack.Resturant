using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using Jack.Resturant.Impls.Baidu.Callbacks;

namespace Jack.Resturant.Impls.Baidu
{
    [ResturantType(ResturantPlatformType.Baidu)]
    public class BaiduResturant : IResturant
    {
        internal Config Config;
        public BaiduResturant()
        {
            this.Config = new Config(ResturantFactory.ResturantListener.OnGetPlatformConfigXml(ResturantPlatformType.Baidu));
        }
        /// <summary>
        /// 往服务器提交请求
        /// </summary>
        /// <param name="cmd">请求的命令</param>
        /// <param name="body">请求参数</param>
        /// <returns></returns>
        public Newtonsoft.Json.Linq.JToken Post(string cmd,Dictionary<string,object> body)
        {
            var dict = new SortedDictionary<string, string>();
            dict["cmd"] = cmd;
            dict["version"] = "3";
            dict["timestamp"] = Helper.ConvertDateTimeInt(DateTime.Now).ToString();
            dict["ticket"] = Guid.NewGuid().ToString().ToUpper();
            dict["source"] = this.Config.Source; //"63976";
            dict["body"] = Newtonsoft.Json.JsonConvert.SerializeObject(body, new Newtonsoft.Json.JsonSerializerSettings() { StringEscapeHandling = Newtonsoft.Json.StringEscapeHandling.EscapeNonAscii });
            dict["secret"] = this.Config.Secret;// "5956da960b9ffd71";
            dict["encrypt"] = "";
            dict["sign"] = BaiduHelper.Sign(dict);

            StringBuilder content = new StringBuilder();
            foreach (var item in dict)
            {
                if (content.Length > 0)
                    content.Append('&');
                content.Append(item.Key);
                content.Append('=');
                content.Append( System.Net.WebUtility.UrlEncode( item.Value));
            }

            var result = Helper.PostQueryString("https://api.waimai.baidu.com" , content.ToString() , 8000);
            var bodystr = result.Substring(result.IndexOf("\"body\":{") + "\"body\":{".Length - 1);
            bodystr = bodystr.Substring(0, bodystr.IndexOf(",\"cmd\""));
            var resultDict = Newtonsoft.Json.JsonConvert.DeserializeObject<SortedDictionary<string,object>>(result);
            resultDict["secret"] = this.Config.Secret;
            if (BaiduHelper.CheckSign(resultDict , bodystr) == false)
                throw new Exception("返回结果签名验证失败");
            var json = (Newtonsoft.Json.Linq.JToken)resultDict["body"];
            if (json["error"].ToString() != "success")
                throw new Exception(json["error"].ToString());
            return json["data"];
        }

        public string GetErpShopId(string baidu_shopid)
        {
            var result = Post("shop.get", new Dictionary<string, object>() {
                { "baidu_shop_id" , baidu_shopid}
            });
            return result.Value<string>("shop_id");
        }

        public OrderInfo GetOrder(string orderid)
        {
            var parameter = new Dictionary<string, object>();
            parameter["order_id"] = orderid;
            var body = Post("order.get", parameter).ToString().JsonToObject<JOrderData>();
            var user = body.user??new JOrderData_User();
            var orderJson = body.order;
            var order = new OrderInfo();
            try
            {
                order.ErpStoreID = Convert.ToInt32(body.shop.id);
            }
            catch
            {

            }
            order.Cellphone = user.phone;
            order.CreateTime = Helper.UnixToDateTime( orderJson.create_time);
            order.CustomerName = user.name;
            order.DayOrderNumber = orderJson.order_index;
            order.DeliveryAddr = user.address;
            if (orderJson.send_immediately != 1)
            {
                order.DeliveryTimeTag = DeliveryTimeTag.OnTime;
                order.DeliveryTime = Helper.UnixToDateTime( orderJson.send_time);
            }
            else
            {
                order.DeliveryTimeTag = DeliveryTimeTag.ASAP;
            }
            order.HasInvoiced = orderJson.need_invoice == 1;
            if (order.HasInvoiced)
            {
                order.TaxpayerId = orderJson.taxer_id;
                order.InvoiceTitle = orderJson.invoice_title;
            }
            order.OriginalPrice = orderJson.total_fee / 100;
            order.PackageFee = orderJson.package_fee / 100;
            order.PayType = orderJson.pay_type == 2 ? PayType.PayOnline : PayType.PayOnDelivery;
            order.ReceiptsMoney = orderJson.user_fee / 100; 
            order.IncomeMoney = orderJson.shop_fee / 100;
            order.Remark = orderJson.remark;
            order.ShippingFee = orderJson.send_fee / 100;
            order.ThirdOrderId = orderJson.order_id;

            var productJsonArr = body.products;
            for (int j = 0; productJsonArr != null && j < productJsonArr.Length; j++)
            {
                for (int i = 0; i < productJsonArr[j].Length; i++)
                {
                    var productJsonItem = productJsonArr[j][i];
                    var detail = new OrderInfo.Detail();
                    order.Details.Add(detail);
                    detail.BoxNumber = productJsonItem.package_amount;
                    detail.BoxPrice = productJsonItem.package_price / 100;
                    detail.ErpSkuId = productJsonItem.other_dish_id;
                    detail.FoodName = productJsonItem.product_name;

                    var product_features_arr = productJsonItem.product_features;
                    detail.FoodProperty = "";
                    if (product_features_arr != null)
                    {
                        foreach (var feature in product_features_arr)
                        {
                            if (detail.FoodProperty.Length > 0)
                                detail.FoodProperty += ";";
                            detail.FoodProperty += feature.option;
                        }
                    }

                    var product_attr_arr = productJsonItem.product_attr;
                    detail.FoodSpec = "";
                    if (product_attr_arr != null)
                    {
                        foreach (var attr in product_attr_arr)
                        {
                            if (detail.FoodSpec.Length > 0)
                                detail.FoodSpec += ";";
                            detail.FoodSpec += attr.option;
                        }
                    }

                    detail.Price = productJsonItem.product_price / 100;
                    detail.Quantity = productJsonItem.product_amount;
                }
            }

            //优惠信息
            try
            {
                var discount = body.discount;
                if (discount != null)
                {
                    foreach (var disItem in discount)
                    {
                        OrderInfo.DiscountInfo info = new OrderInfo.DiscountInfo();
                        info.PlatformCharge = disItem.baidu_rate / 100;
                        info.PoiCharge = disItem.shop_rate / 100;
                        info.ReduceFee = disItem.fee / 100;
                        info.Remark = disItem.desc;
                        order.DiscountInfos.Add(info);
                    }
                }
            }
            catch
            {

            }

            return order;
        }
       
        public void CancelOrder(CancelOrderParameter parameter)
        {
            var dict = new Dictionary<string, object>();
            dict["order_id"] = parameter.OrderID;
            switch(parameter.Reason)
            {
                case CancelOrderReason.AddressError:
                    dict["type"] = 1;
                    dict["reason"] = "不在配送范围内";
                    break;
                case CancelOrderReason.Busy:
                    dict["type"] = 7;
                    dict["reason"] = "餐厅太忙";
                    break;
                case CancelOrderReason.CanNotContact:
                    dict["type"] = 8;
                    dict["reason"] = "联系不上用户";
                    break;
                case CancelOrderReason.CustomerCancel:
                    dict["type"] = 5;
                    dict["reason"] = "用户取消订单";
                    break;
                case CancelOrderReason.RepeatOrder:
                    dict["type"] = 6;
                    dict["reason"] = "重复订单";
                    break;
                case CancelOrderReason.SoldOut:
                    dict["type"] = 3;
                    dict["reason"] = "美食已售完";
                    break;
                case CancelOrderReason.StoreClosed:
                    dict["type"] = 2;
                    dict["reason"] = "餐厅已打烊";
                    break;
                default:
                    dict["type"] = -1;
                    dict["reason"] = "其他";
                    break;
            }
            Post("order.cancel", dict);
        }

        public void ConfirmOrder(ConfirmOrderParameter parameter)
        {
            Post("order.confirm", new Dictionary<string, object>() {
                {"order_id",parameter.OrderID}
            });
        }

        public void CreateDishCategory(CreateDishCategoryParameter parameter)
        {
            Post("dish.category.create", new Dictionary<string, object>() {
                { "shop_id",parameter.ErpStoreId.ToString()},
                { "name",parameter.CategoryName},
                { "rank",parameter.Sequence + 1},//parameter.Sequence不能是0
            });
        }

        public void DeleteDish(int erpStoreId, string token, string erpDishId, string dishId)
        {
            Post("dish.delete", new Dictionary<string, object>() {
                { "shop_id",erpStoreId.ToString()},
                { "dish_id",erpDishId}
            });
        }

        public void DeleteDishCategory(int erpStoreId, string token, string categoryName)
        {
            //获取分类id
            var data = Post("dish.category.get", new Dictionary<string, object>() {
                { "shop_id",erpStoreId.ToString()},
                { "name",categoryName}
            });
            var category_id = data.Value<string>("category_id");


            //删除分类
            Post("dish.category.delete", new Dictionary<string, object>() {
                { "shop_id",erpStoreId.ToString()},
                { "category_ids",new string[]{ category_id } }
            });
        }

        public void Delivered(DeliveredParameter parameter)
        {
            throw new NotImplementedException();
        }

        public void Delivering(DeliverParameter parameter)
        {
            throw new NotImplementedException();
        }

        public void ArgeeOrderRefund(string token, string orderId)
        {
            throw new NotImplementedException();
        }

        public void DisargeeOrderRefund(string token, string orderId, string reason)
        {
            throw new NotImplementedException();
        }

        public void DishMap(DishMapParameter parameter)
        {
            List<object> mapObjects = new List<object>();
            foreach( var mapitem in parameter.DishMapItems )
            {
                mapObjects.Add(new {
                    baidu_dish_id = mapitem.DishId,
                    dish_id = mapitem.ErpDishId
                });
            }
            Post("dishes.id.batchupdate", new Dictionary<string, object>() {
                { "shop_id" , parameter.ErpStoreID.ToString()},
                { "ids" , mapObjects}
            });

        }

        public List<DishCategory> GetDishCategoryList(int erpStoreId, string token)
        {
            var data = (Newtonsoft.Json.Linq.JArray)Post("dish.category.all",new Dictionary<string, object>() {
                { "shop_id" , erpStoreId.ToString()}
            } );
            var result = new List<DishCategory>();

            foreach( var jsonData in data )
            {
                var category = new DishCategory();
                category.CategoryId = jsonData.Value<string>("category_id");
                category.Name = jsonData.Value<string>("name");
                category.Sequence = jsonData.Value<int>("rank");
                result.Add(category);
            }

            return result;
        }

        public DishInfo GetDish(int erpStoreId, string erpDishId)
        {
            var dict = new Dictionary<string, object>();
            dict["shop_id"] = erpStoreId.ToString();
            dict["dish_id"] = erpDishId;
            var data = Post("dish.get", dict);
            return null;
        }

        public List<DishInfo> GetDishList(int erpStoreId, string token)
        {
            List<DishInfo> result = new List<DishInfo>();

            var data = (Newtonsoft.Json.Linq.JArray)Post("dish.menu.get", new Dictionary<string, object>() {
                { "shop_id",erpStoreId.ToString()}
            });

            // 循环所有分类
            foreach( var group in data )
            {
                var categoryName = group["category"].Value<string>("name");
                var products = (Newtonsoft.Json.Linq.JArray)group["products"];
                
                // 循环分类下的菜品
                foreach( var dishJson in products )
                {
                    // 判断是否是菜品
                    if (dishJson.Value<int>("type") != 1)
                        continue;

                    DishInfo dish = new DishInfo();
                    result.Add(dish);
                    dish.CategoryName = categoryName;
                    dish.ErpDishId = dishJson.Value<string>("dish_id");
                    dish.DishId = dishJson.Value<string>("baidu_dish_id");
                    dish.DishName = dishJson.Value<string>("name");
                    dish.Price = dishJson.Value<double>("price")/100;
                    dish.Picture = dishJson.Value<string>("pic");
                    dish.BoxNumber = dishJson.Value<int>("package_box_num");
                    dish.Description = dishJson.Value<string>("description");
                    try
                    {
                        var available_times = (Newtonsoft.Json.Linq.JArray)dishJson["available_times"];
                        foreach(Newtonsoft.Json.Linq.JArray dayJson in available_times )
                        {
                            DayOpenTime dayTime = new DayOpenTime();
                            dish.AvailableTimes.Add(dayTime);

                            foreach( var timeJson in dayJson )
                            {
                                dayTime.Times.Add($"{timeJson["start_time"]}-{timeJson["end_time"]}");
                            }
                        }
                        if(dish.AvailableTimes.Count == 7)
                        {
                            //把第一个移到最后，因为第一个是周日
                            var sunday = dish.AvailableTimes[0];
                            dish.AvailableTimes.RemoveAt(0);
                            dish.AvailableTimes.Add(sunday);
                        }
                    }
                    catch
                    {

                    }
                }
            }
            return result;
        }

       
        public string StoreMap(int companyId, int erpStoreId, string erpStoreName)
        {
            return $"https://wmcrm.baidu.com/crm?qt=apishopbindpage&source={this.Config.StoreMapSource}&otherShopId={erpStoreId}";
        }
        public string ReleaseStoreMap(string token, int erpStoreId)
        {
            return $"https://wmcrm.baidu.com/crm?qt=apishopunbindpage";
        }

        public StoreInfo GetStoreInfo(int erpStoreId, string token)
        {
            var data = Post("shop.get", new Dictionary<string, object>() {
                { "shop_id" , erpStoreId.ToString()}
            });
            StoreInfo info = new StoreInfo();
            info.Address = data.Value<string>("address");
            info.Name = data.Value<string>("name");
            var phone = data.Value<string>("phone");
            if (!string.IsNullOrEmpty(phone))
            {
                info.Phones.Add(phone);
            }
            info.Status = data.Value<int>("status") == 1 ? StoreStatus.Opened : StoreStatus.Closed;
            var business_time = (Newtonsoft.Json.Linq.JArray)data["business_time"];
            info.DayOpenTimes.Add(new DayOpenTime());
            foreach ( var timeJson in business_time )
            {
                var endstr = timeJson.Value<string>("end");
                if (endstr == "00:00")
                    endstr = "23:59";
                info.DayOpenTimes[0].Times.Add($"{timeJson["start"]}-{endstr}");
            }
            return info;
        }

        public void SetStoreOpenTimes(int erpStoreId, string token, List<DayOpenTime> dayOpenTimes)
        {
            var bodyDict = new Dictionary<string, object>();
            var business_time = new List<object>();
            bodyDict["business_time"] = business_time;
            bodyDict["shop_id"] = erpStoreId.ToString();

            foreach( var time in dayOpenTimes[0].Times )
            {
                var arr = time.Split('-');
                business_time.Add(new {
                    start = arr[0].Trim(),
                    end = arr[1].Trim()
                });
            }
            Post("shop.update", bodyDict);
        }

        public void SetStoreStatus(int erpStoreId, string token, StoreStatus status)
        {
            var cmd = (status == StoreStatus.Opened) ? "shop.open" : "shop.close";
            Post(cmd, new Dictionary<string, object>() {
                { "shop_id" , erpStoreId.ToString()}
            });
        }

        public IEnumerable<string> ShowCallbackUrlSetting(string urlDomain)
        {
            List<string> result = new List<string>();
            var types = typeof(BaiduResturant).Assembly.GetTypes().Where(m => m.FullName.StartsWith("Jack.Resturant.Impls.Baidu.Callbacks")).ToArray();
            foreach (var type in types)
            {
                object[] atts = type.GetCustomAttributes(typeof(CallbackDescAttribute), true);
                if (atts.Length > 0)
                {
                    var desc = ((CallbackDescAttribute)atts[0]).Desc;
                    var fieldInfo = type.GetField("NotifyPageName");
                    var page = fieldInfo.GetValue(null);
                    result.Add($"{desc}：{urlDomain}{page}");
                }
            }
            return result;
        }

       

        public void UpdateDishCategory(UpdateDishCategoryParameter parameter)
        {
            var bodyDict = new Dictionary<string, object>();
            bodyDict["shop_id"] = parameter.ErpStoreId.ToString();
            bodyDict["old_name"] = parameter.OriginalCategoryName;
            bodyDict["name"] = parameter.CategoryName;
            bodyDict["rank"] = parameter.Sequence + 1;
            Post("dish.category.update", bodyDict);
        }

        public TokenResult UpdateToken(int erpStoreId, string token, string refresh_token)
        {
            throw new Exception("百度外卖不需要更新token");
        }

        public void UploadDish(int erpStoreId, string token, List<DishInfo> dishes)
        {
            foreach( var dish in dishes )
            {
                var bodyDict = new Dictionary<string, object>();
                bodyDict["dish_id"] = dish.ErpDishId;
                bodyDict["shop_id"] = erpStoreId.ToString();
                bodyDict["name"] = dish.DishName;
                bodyDict["price"] = Convert.ToInt32(dish.Price*100);
                if(!string.IsNullOrEmpty(dish.Picture))
                {
                    bodyDict["pic"] = dish.Picture;
                }
                bodyDict["min_order_num"] = 1;
                bodyDict["package_box_num"] = dish.BoxNumber;
                bodyDict["description"] = dish.Description;

                var categories = new List<object>();
                bodyDict["category"] = categories;
                categories.Add(new {
                    name = dish.CategoryName,
                    rank = dish.Sequence
                });

                var specs = new List<object>();
                bodyDict["norms"] = specs;
                int skuindex = 1;
                foreach (var dishSku in dish.Skus)
                {
                    specs.Add(new
                    {
                        value = dishSku.Spec,
                        price = Convert.ToInt32(dishSku.Price * 100).ToString(),
                        stock = dishSku.Stock == null ? "999999" : dishSku.Stock.ToString(),
                        selfid = $"{dish.ErpDishId}_{skuindex++}"
                    });
                }

                var attrs = new List<object>();
                bodyDict["attr"] = attrs;
                foreach (var dishAttr in dish.Attributes)
                {
                    foreach (var attrValue in dishAttr.Values)
                    {
                        attrs.Add(new
                        {
                            name = dishAttr.Name,
                            value = attrValue
                        });
                    }
                }

                if ( dish.AvailableTimes.Count > 0 )
                {
                    var available_times = new List<object>();
                    bodyDict["available_times"] = available_times;

                    foreach ( var day in dish.AvailableTimes )
                    {
                        var daytimeSetting = new List<object>();
                        available_times.Add(daytimeSetting);

                        foreach (var time in day.Times)
                        {
                            var start_end = time.Split('-');
                            daytimeSetting.Add(new
                            {
                                start = start_end[0].Trim(),
                                end = start_end[1].Trim()
                            });
                        }
                    }
                    if (available_times.Count == 1)
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            available_times.Add(available_times[0]);
                        }
                    }
                    if(available_times.Count != 7)
                    {
                        throw new Exception("AvailableTimes必须是1个，或者7个元素");
                    }
                    //把最后一个元素移到第一，因为周日是第一个元素
                    var lastobj = available_times.Last();
                    available_times.RemoveAt(available_times.Count - 1);
                    available_times.Insert(0, lastobj);
                }

                try
                {
                    Post("dish.update", bodyDict);
                }
                catch(Exception ex)
                {
                    if (ex.Message.Contains("dish not exist"))
                    {
                        Post("dish.create", bodyDict);
                    }
                    else
                        throw ex;
                }
            }
        }

        public string UploadDishPicture(int erpStoreId, string token, string imageName, byte[] picContent)
        {
            //生成图片id
            var picId = Guid.NewGuid().ToString("N");
            //把图片放入静态字典
            ResponsePicture.Pictures.TryAdd(picId, picContent);
            //构造虚拟图片地址
            var myurl = $"{(ResturantFactory.CurrentDomainUrl)}{ResponsePicture.NotifyPageName}/{picId}/{imageName}";
            //提交数据到百度
            var data = Post("picture.upload", new Dictionary<string, object>() {
                { "url" , myurl.Replace("https" , "http")}
            });
            //返回百度图片地址
            return data.Value<string>("url");
        }

        public ServiceAmountInfo GetOrderServiceAmount(string token, string orderId)
        {
            var parameter = new Dictionary<string, object>();
            parameter["order_id"] = orderId;
            var body = Post("order.get", parameter);
            var user = body["user"];
            var orderJson = body["order"];
            return new ServiceAmountInfo() {
                PlatformServiceAmount = orderJson.Value<double>("commission")/100,
                SettleAmount = orderJson.Value<double>("shop_fee") / 100
            };
        }
    }
}
