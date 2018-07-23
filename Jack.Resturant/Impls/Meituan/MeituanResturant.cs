using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Jack.Resturant.Impls.Meituan
{
    [ResturantType(ResturantPlatformType.Meituan)]
    class MeituanResturant : IResturant
    {
        public Config Config
        {
            get;
            private set;
        }
        public MeituanResturant()
        {
            this.Config = new Config(ResturantFactory.ResturantListener.OnGetPlatformConfigXml( ResturantPlatformType.Meituan));
        }
        public IEnumerable<string> ShowCallbackUrlSetting(string urlDomain)
        {
            List<string> result = new List<string>();
            var types = typeof(MeituanResturant).Assembly.GetTypes().Where(m => m.FullName.StartsWith("Jack.Resturant.Impls.Meituan.Callbacks")).ToArray();
            foreach( var type in types )
            {
                object[] atts = type.GetCustomAttributes(typeof(CallbackDescAttribute), true);
                if( atts.Length > 0 )
                {
                     var desc = ((CallbackDescAttribute)atts[0]).Desc;
                    var fieldInfo = type.GetField("NotifyPageName");
                    var page = fieldInfo.GetValue(null);
                    result.Add($"{desc}：{urlDomain}{page}");
                }
            }
            return result;
        }

        public string StoreMap(int companyid , int erpStoreId, string erpStoreName)
        {
            return $"https://open-erp.meituan.com/storemap?developerId={this.Config.developerId}&businessId=2&ePoiId={erpStoreId}&signKey={this.Config.SignKey}&ePoiName={System.Net.WebUtility.UrlEncode(erpStoreName)}";
        }
        public string ReleaseStoreMap(string token, int erpStoreId)
        {
            return $"https://open-erp.meituan.com/releasebinding?signKey={this.Config.SignKey}&businessId=2&appAuthToken={token}";
        }
        public void ConfirmOrder(ConfirmOrderParameter parameter)
        {
            var postDict = new SortedDictionary<string, object>();
            postDict["appAuthToken"] = parameter.Token;
            postDict["charset"] = "utf-8";
            postDict["timestamp"] = (Helper.ConvertDateTimeInt(DateTime.Now)).ToString();
            postDict["orderId"] = parameter.OrderID;
            postDict["sign"] = MeituanHelper.Sign(postDict, this.Config.SignKey);
            var result = Helper.PostQueryString("http://api.open.cater.meituan.com/waimai/order/confirm", postDict, 8000);
            var jsonObj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(result);
            Newtonsoft.Json.Linq.JToken errobj;
            if(jsonObj.TryGetValue("error" , StringComparison.CurrentCultureIgnoreCase ,out errobj ))
            {
                throw new Exception(errobj.Value<string>("message"));
            }
            if (!string.Equals(jsonObj.Value<string>("data"), "ok", StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception("发生错误");
            }
        }

        public void CancelOrder(CancelOrderParameter parameter)
        {
            var postDict = new SortedDictionary<string, object>();
            postDict["appAuthToken"] = parameter.Token;
            postDict["charset"] = "utf-8";
            postDict["timestamp"] = (Helper.ConvertDateTimeInt(DateTime.Now)).ToString();
            postDict["orderId"] = parameter.OrderID;
            //匹配原因
            switch(parameter.Reason)
            {
                case CancelOrderReason.AddressError:
                    postDict["reasonCode"] = "2010";
                    postDict["reason"] = "地址无法配送";
                    break;
                case CancelOrderReason.Busy:
                    postDict["reasonCode"] = "2008";
                    postDict["reason"] = "店铺太忙";
                    break;
                case CancelOrderReason.CanNotContact:
                    postDict["reasonCode"] = "2012";
                    postDict["reason"] = "联系不上用户";
                    break;
                case CancelOrderReason.CustomerCancel:
                    postDict["reasonCode"] = "2006";
                    postDict["reason"] = "用户要求取消";
                    break;
                case CancelOrderReason.DeliverDelay:
                    postDict["reasonCode"] = "2004";
                    postDict["reason"] = "配送延迟";
                    break;
                case CancelOrderReason.DeliverSlowly:
                    postDict["reasonCode"] = "2015";
                    postDict["reason"] = "配送员送餐慢";
                    break;
                case CancelOrderReason.LostFood:
                    postDict["reasonCode"] = "2016";
                    postDict["reason"] = "配送员丢餐、少餐、餐洒";
                    break;
                case CancelOrderReason.RepeatOrder:
                    postDict["reasonCode"] = "2013";
                    postDict["reason"] = "重复订单";
                    break;
                case CancelOrderReason.SoldOut:
                    postDict["reasonCode"] = "2009";
                    postDict["reason"] = "商品已售完";
                    break;
                case CancelOrderReason.StoreClosed:
                    postDict["reasonCode"] = "2011";
                    postDict["reason"] = "店铺已打烊";
                    break;
                case CancelOrderReason.TakeFoodSlowly:
                    postDict["reasonCode"] = "2014";
                    postDict["reason"] = "配送员取餐慢";
                    break;
                default:
                    postDict["reasonCode"] = "2007";
                    postDict["reason"] = "其他原因";
                    break;
            }

            postDict["sign"] = MeituanHelper.Sign(postDict, this.Config.SignKey);
            var result = Helper.PostQueryString("http://api.open.cater.meituan.com/waimai/order/cancel", postDict, 8000);
            var jsonObj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(result);
            Newtonsoft.Json.Linq.JToken errobj;
            if (jsonObj.TryGetValue("error", StringComparison.CurrentCultureIgnoreCase, out errobj))
            {
                throw new Exception(errobj.Value<string>("message"));
            }
            if (!string.Equals(jsonObj.Value<string>("data"), "ok", StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception("发生错误");
            }
        }

        public void ArgeeOrderRefund(string token, string orderId)
        {
            var postDict = new SortedDictionary<string, object>();
            postDict["appAuthToken"] = token;
            postDict["charset"] = "utf-8";
            postDict["timestamp"] = (Helper.ConvertDateTimeInt(DateTime.Now)).ToString();
            postDict["orderId"] = orderId;
            postDict["reason"] = "同意取消";
            postDict["sign"] = MeituanHelper.Sign(postDict, this.Config.SignKey);
            var result = Helper.PostQueryString("http://api.open.cater.meituan.com/waimai/order/agreeRefund", postDict, 8000);
            var jsonObj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(result);
            Newtonsoft.Json.Linq.JToken errobj;
            if (jsonObj.TryGetValue("error", StringComparison.CurrentCultureIgnoreCase, out errobj))
            {
                throw new Exception(errobj.Value<string>("message"));
            }
            if (!string.Equals(jsonObj.Value<string>("data"), "ok", StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception("发生错误");
            }
        }
        public void DisargeeOrderRefund(string token, string orderId, string reason)
        {
            var postDict = new SortedDictionary<string, string>();
            postDict["appAuthToken"] = token;
            postDict["charset"] = "utf-8";
            postDict["timestamp"] = (Helper.ConvertDateTimeInt(DateTime.Now)).ToString();
            postDict["orderId"] = orderId;
            postDict["reason"] = string.IsNullOrEmpty( reason) ? "拒绝" : reason;
            postDict["sign"] = MeituanHelper.Sign(postDict, this.Config.SignKey);
            var result = Helper.PostQueryString("http://api.open.cater.meituan.com/waimai/order/rejectRefund", postDict, 8000);
            var jsonObj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(result);
            Newtonsoft.Json.Linq.JToken errobj;
            if (jsonObj.TryGetValue("error", StringComparison.CurrentCultureIgnoreCase, out errobj))
            {
                throw new Exception(errobj.Value<string>("message"));
            }
            if (!string.Equals(jsonObj.Value<string>("data"), "ok", StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception("发生错误");
            }
        }
        public void Delivering(DeliverParameter parameter)
        {
            var postDict = new SortedDictionary<string, object>();
            postDict["appAuthToken"] = parameter.Token;
            postDict["charset"] = "utf-8";
            postDict["timestamp"] = (Helper.ConvertDateTimeInt(DateTime.Now)).ToString();
            postDict["orderId"] = parameter.OrderID;
            postDict["courierName"] = parameter.CourierName;
            postDict["courierPhone"] = parameter.CourierPhone;
            postDict["sign"] = MeituanHelper.Sign(postDict, this.Config.SignKey);
            var result = Helper.PostQueryString("http://api.open.cater.meituan.com/waimai/order/delivering", postDict, 8000);
            var jsonObj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(result);
            Newtonsoft.Json.Linq.JToken errobj;
            if (jsonObj.TryGetValue("error", StringComparison.CurrentCultureIgnoreCase, out errobj))
            {
                throw new Exception(errobj.Value<string>("message"));
            }
            if (!string.Equals(jsonObj.Value<string>("data"), "ok", StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception("发生错误");
            }
        }

        public void Delivered(DeliveredParameter parameter)
        {
            var postDict = new SortedDictionary<string, object>();
            postDict["appAuthToken"] = parameter.Token;
            postDict["charset"] = "utf-8";
            postDict["timestamp"] = (Helper.ConvertDateTimeInt(DateTime.Now)).ToString();
            postDict["orderId"] = parameter.OrderID;
            postDict["sign"] = MeituanHelper.Sign(postDict, this.Config.SignKey);
            var result = Helper.PostQueryString("http://api.open.cater.meituan.com/waimai/order/delivered", postDict, 8000);
            var jsonObj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(result);
            Newtonsoft.Json.Linq.JToken errobj;
            if (jsonObj.TryGetValue("error", StringComparison.CurrentCultureIgnoreCase, out errobj))
            {
                throw new Exception(errobj.Value<string>("message"));
            }
            if (!string.Equals(jsonObj.Value<string>("data"), "ok", StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception("发生错误");
            }
        }

        public void GetDishAttribute(string erpDishId,List<DishAttribute> erpAttributes, String token)
        {
            if (string.IsNullOrEmpty(erpDishId))
                return;
            var postDict = new SortedDictionary<string, string>();
            postDict["appAuthToken"] = token;
            postDict["charset"] = "utf-8";
            postDict["timestamp"] = (Helper.ConvertDateTimeInt(DateTime.Now)).ToString();
            postDict["eDishCode"] = erpDishId;
            postDict["sign"] = MeituanHelper.Sign(postDict, this.Config.SignKey);
            StringBuilder url = new StringBuilder("http://api.open.cater.meituan.com/waimai/dish/queryPropertyList?");
            foreach (var item in postDict)
            {
                url.Append(item.Key);
                url.Append('=');
                url.Append(item.Value);
                url.Append('&');
            }

            var result = Helper.GetQueryString(url.ToString(), 8000);
            var jsonObj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(result);
            Newtonsoft.Json.Linq.JToken errobj;
            if (jsonObj.TryGetValue("error", StringComparison.CurrentCultureIgnoreCase, out errobj))
            {
                throw new Exception(errobj.Value<string>("message"));
            }
            var datas = (Newtonsoft.Json.Linq.JArray)jsonObj["data"];
           
            for (int i = 0; i < datas.Count; i++)
            {
                var attJson = datas[i];
                var attribute = new DishAttribute();
                erpAttributes.Add(attribute);

                attribute.Name = attJson.Value<string>("propertyName");
                var details = (Newtonsoft.Json.Linq.JArray)attJson["values"];
                foreach (var item in details)
                {
                    attribute.Values.Add(item.ToString());
                }

            }
        }

        public ServiceAmountInfo GetOrderServiceAmount(string token, string orderId)
        {
            var postDict = new SortedDictionary<string, object>();
            postDict["appAuthToken"] = token;
            postDict["charset"] = "utf-8";
            postDict["timestamp"] = (Helper.ConvertDateTimeInt(DateTime.Now)).ToString();
            postDict["orderId"] = orderId;
            postDict["sign"] = MeituanHelper.Sign(postDict, this.Config.SignKey);

            StringBuilder url = new StringBuilder("http://api.open.cater.meituan.com/waimai/order/queryById?");
            foreach (var item in postDict)
            {
                url.Append(item.Key);
                url.Append('=');
                url.Append(item.Value);
                url.Append('&');
            }

            var result = Helper.GetQueryString(url.ToString(), 8000);

            var jsonObj = (Newtonsoft.Json.Linq.JToken) Newtonsoft.Json.JsonConvert.DeserializeObject(result);
            var poiReceiveDetail = jsonObj["data"].Value<string>("poiReceiveDetail");
            jsonObj = (Newtonsoft.Json.Linq.JToken)Newtonsoft.Json.JsonConvert.DeserializeObject(poiReceiveDetail);
            var info = new ServiceAmountInfo();
            info.PlatformServiceAmount = jsonObj.Value<double>("foodShareFeeChargeByPoi") / 100;
            info.SettleAmount = jsonObj.Value<double>("wmPoiReceiveCent") / 100;
            
            return info;
        }

        public List<DishInfo> GetDishList(int erpStoreId, String token)
        {
            int offset = 0;
            List<DishInfo> dishes = new List<DishInfo>();
            while (true)
            {
                var postDict = new SortedDictionary<string, object>();
                postDict["appAuthToken"] = token;
                postDict["charset"] = "utf-8";
                postDict["timestamp"] = (Helper.ConvertDateTimeInt(DateTime.Now)).ToString();
                postDict["ePoiId"] = erpStoreId.ToString();
                postDict["offset"] = offset.ToString();
                postDict["limit"] = 199;
                postDict["sign"] = MeituanHelper.Sign(postDict, this.Config.SignKey);

                StringBuilder url = new StringBuilder("http://api.open.cater.meituan.com/waimai/dish/queryListByEPoiId?");
                foreach (var item in postDict)
                {
                    url.Append(item.Key);
                    url.Append('=');
                    url.Append(item.Value);
                    url.Append('&');
                }

                var result = Helper.GetQueryString(url.ToString(), 8000);
                var jsonObj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(result);
                Newtonsoft.Json.Linq.JToken errobj;
                if (jsonObj.TryGetValue("error", StringComparison.CurrentCultureIgnoreCase, out errobj))
                {
                    throw new Exception(errobj.Value<string>("message"));
                }
                var datas = (Newtonsoft.Json.Linq.JArray)jsonObj["data"];

                for (int i = 0; i < datas.Count; i++)
                {
                    var dishJson = datas[i];
                    var dish = new DishInfo();
                    dishes.Add(dish);
                    dish.CategoryName = dishJson.Value<string>("categoryName");
                    dish.DishName = dishJson.Value<string>("dishName");
                    dish.ErpDishId = dishJson.Value<string>("eDishCode");
                    dish.BoxNumber = dishJson.Value<int>("boxNum");
                    dish.BoxPrice = dishJson.Value<double>("boxPrice");
                    dish.Description = dishJson.Value<string>("description");
                    dish.Price = dishJson.Value<double>("price");
                    dish.Sequence = dishJson.Value<int>("sequence");
                    dish.Unit = dishJson.Value<string>("unit");

                    var skus = (Newtonsoft.Json.Linq.JArray)dishJson["skus"];
                    for (int j = 0; j < skus.Count; j++)
                    {
                        var skuJson = skus[j];
                        var subdish = new DishSkuInfo();
                        dish.Skus.Add(subdish);

                        subdish.ErpSkuId = skuJson.Value<string>("skuId");
                        subdish.Price = skuJson.Value<double>("price");
                        subdish.Spec = skuJson.Value<string>("spec");
                        if (skuJson["stock"].HasValues)
                        {
                            subdish.Stock = skuJson.Value<uint>("stock");
                        }
                        try
                        {
                            if (dish.AvailableTimes.Count == 0)
                            {
                                var availableTimes = skuJson["availableTimes"];
                                var weekToken = availableTimes.First as Newtonsoft.Json.Linq.JProperty;
                                var strArr = new List<string>(new string[] { "monday", "tuesday", "wednesday", "thursday", "friday", "saturday", "sunday" });
                                while (weekToken != null)
                                {
                                    if (dish.AvailableTimes.Count == 0)
                                    {
                                        for (int k = 0; k < 7; k++)
                                        {
                                            dish.AvailableTimes.Add(new DayOpenTime());
                                        }
                                    }

                                    int dayindex = strArr.IndexOf(weekToken.Name);
                                    var targetTimes = dish.AvailableTimes[dayindex].Times;
                                    if (weekToken.Value.ToString().Length > 0)
                                    {
                                        targetTimes.AddRange(weekToken.Value.ToString().Split(','));
                                    }

                                    weekToken = weekToken.Next as Newtonsoft.Json.Linq.JProperty;
                                }
                            }
                        }
                        catch
                        {

                        }
                    }

                    GetDishAttribute(dish.ErpDishId, dish.Attributes, token);
                }

                if (datas.Count < 199)
                    break;
                else
                {
                    // 继续下一页
                    offset += datas.Count;
                }
            }
            return dishes;
        }

        public void DishMap(DishMapParameter parameter)
        {
            var postDict = new SortedDictionary<string, string>();
            postDict["appAuthToken"] = parameter.Token;
            postDict["charset"] = "utf-8";
            postDict["timestamp"] = (Helper.ConvertDateTimeInt(DateTime.Now)).ToString();
            postDict["ePoiId"] = parameter.ErpStoreID.ToString();
            //转换菜品列表到dishMappings参数
            object[] dishMappings = new object[parameter.DishMapItems.Count];
            for(int i = 0; i < dishMappings.Length; i ++)
            {
                var source = parameter.DishMapItems[i];
                Dictionary<string, object> dishmapObj = new Dictionary<string, object>();
                dishMappings[i] = dishmapObj;

                dishmapObj["dishId"] = int.Parse(source.DishId);
                dishmapObj["eDishCode"] = source.ErpDishId;
            }
            postDict["dishMappings"] = Newtonsoft.Json.JsonConvert.SerializeObject(dishMappings);


            postDict["sign"] = MeituanHelper.Sign(postDict, this.Config.SignKey);
            var result = Helper.PostQueryString("http://api.open.cater.meituan.com/waimai/dish/mapping", postDict, 8000);
            var jsonObj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(result);
            Newtonsoft.Json.Linq.JToken errobj;
            if (jsonObj.TryGetValue("error", StringComparison.CurrentCultureIgnoreCase, out errobj))
            {
                throw new Exception(errobj.Value<string>("message"));
            }
            if (!string.Equals(jsonObj.Value<string>("data") , "ok" , StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception("发生错误");
            }
        }

        public void DeleteDish(int erpStoreId, string token, string erpDishId,string dishId)
        {
            var postDict = new SortedDictionary<string, string>();
            postDict["appAuthToken"] = token;
            postDict["charset"] = "utf-8";
            postDict["timestamp"] = (Helper.ConvertDateTimeInt(DateTime.Now)).ToString();
            postDict["ePoiId"] = erpStoreId.ToString();
            postDict["eDishCode"] = erpDishId;
            postDict["sign"] = MeituanHelper.Sign(postDict, this.Config.SignKey);
            var result = Helper.PostQueryString("http://api.open.cater.meituan.com/waimai/dish/delete", postDict, 8000);
            var jsonObj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(result);
            Newtonsoft.Json.Linq.JToken errobj;
            if (jsonObj.TryGetValue("error", StringComparison.CurrentCultureIgnoreCase, out errobj))
            {
                throw new Exception(errobj.Value<string>("message"));
            }
            if (!string.Equals(jsonObj.Value<string>("data"), "ok", StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception("发生错误");
            }
        }

        public void DeleteDishSku(int erpStoreId, string token, string erpDishId, string erpSkuId)
        {
            var postDict = new SortedDictionary<string, string>();
            postDict["appAuthToken"] = token;
            postDict["charset"] = "utf-8";
            postDict["timestamp"] = (Helper.ConvertDateTimeInt(DateTime.Now)).ToString();
            postDict["eDishCode"] = erpDishId;
            postDict["eDishSkuCode"] = erpSkuId;
            postDict["sign"] = MeituanHelper.Sign(postDict, this.Config.SignKey);
            var result = Helper.PostQueryString("http://api.open.cater.meituan.com/waimai/dish/deleteSku", postDict, 8000);
            var jsonObj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(result);
            Newtonsoft.Json.Linq.JToken errobj;
            if (jsonObj.TryGetValue("error", StringComparison.CurrentCultureIgnoreCase, out errobj))
            {
                throw new Exception(errobj.Value<string>("message"));
            }
            if (!string.Equals(jsonObj.Value<string>("data"), "ok", StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception("发生错误");
            }
        }
        public void DeleteDishCategory(int erpStoreId, string token, string categoryName)
        {
            var postDict = new SortedDictionary<string, string>();
            postDict["appAuthToken"] = token;
            postDict["charset"] = "utf-8";
            postDict["timestamp"] = (Helper.ConvertDateTimeInt(DateTime.Now)).ToString();
            postDict["catName"] = categoryName;
            postDict["sign"] = MeituanHelper.Sign(postDict, this.Config.SignKey);
            var result = Helper.PostQueryString("http://api.open.cater.meituan.com/waimai/dish/deleteCat", postDict, 8000);
            var jsonObj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(result);
            Newtonsoft.Json.Linq.JToken errobj;
            if (jsonObj.TryGetValue("error", StringComparison.CurrentCultureIgnoreCase, out errobj))
            {
                throw new Exception(errobj.Value<string>("message"));
            }
            if (!string.Equals(jsonObj.Value<string>("data"), "ok", StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception("发生错误");
            }
        }

        public void CreateDishCategory(CreateDishCategoryParameter parameter)
        {
            var postDict = new SortedDictionary<string, object>();
            postDict["appAuthToken"] = parameter.Token;
            postDict["charset"] = "utf-8";
            postDict["timestamp"] = (Helper.ConvertDateTimeInt(DateTime.Now)).ToString();
            postDict["catName"] = parameter.CategoryName;
            postDict["sequence"] = parameter.Sequence.ToString();
            postDict["sign"] = MeituanHelper.Sign(postDict, this.Config.SignKey);
            var result = Helper.PostQueryString("http://api.open.cater.meituan.com/waimai/dish/updateCat", postDict, 8000);
            var jsonObj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(result);
            Newtonsoft.Json.Linq.JToken errobj;
            if (jsonObj.TryGetValue("error", StringComparison.CurrentCultureIgnoreCase, out errobj))
            {
                throw new Exception(errobj.Value<string>("message"));
            }
            if (!string.Equals(jsonObj.Value<string>("data"), "ok", StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception("发生错误");
            }
        }
        public void UpdateDishCategory(UpdateDishCategoryParameter parameter)
        {
            var postDict = new SortedDictionary<string, string>();
            postDict["appAuthToken"] = parameter.Token;
            postDict["charset"] = "utf-8";
            postDict["timestamp"] = (Helper.ConvertDateTimeInt(DateTime.Now)).ToString();
            postDict["catName"] = parameter.CategoryName;
            postDict["oldCatName"] = parameter.OriginalCategoryName;
            postDict["sequence"] = parameter.Sequence.ToString();
            postDict["sign"] = MeituanHelper.Sign(postDict, this.Config.SignKey);
            var result = Helper.PostQueryString("http://api.open.cater.meituan.com/waimai/dish/updateCat", postDict, 8000);
            var jsonObj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(result);
            Newtonsoft.Json.Linq.JToken errobj;
            if (jsonObj.TryGetValue("error", StringComparison.CurrentCultureIgnoreCase, out errobj))
            {
                throw new Exception(errobj.Value<string>("message"));
            }
            if (!string.Equals(jsonObj.Value<string>("data"), "ok", StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception("发生错误");
            }
        }
        public List<DishCategory> GetDishCategoryList(int erpStoreId, String token)
        {
            var postDict = new SortedDictionary<string, object>();
            postDict["appAuthToken"] = token;
            postDict["charset"] = "utf-8";
            postDict["timestamp"] = (Helper.ConvertDateTimeInt(DateTime.Now)).ToString();
            postDict["sign"] = MeituanHelper.Sign(postDict, this.Config.SignKey);
            StringBuilder url = new StringBuilder("http://api.open.cater.meituan.com/waimai/dish/queryCatList?");
            foreach (var item in postDict)
            {
                url.Append(item.Key);
                url.Append('=');
                url.Append(item.Value);
                url.Append('&');
            }

            var result = Helper.GetQueryString(url.ToString(), 8000);
            var jsonObj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(result);
            Newtonsoft.Json.Linq.JToken errobj;
            if (jsonObj.TryGetValue("error", StringComparison.CurrentCultureIgnoreCase, out errobj))
            {
                throw new Exception(errobj.Value<string>("message"));
            }
            var datas = (Newtonsoft.Json.Linq.JArray)jsonObj["data"];
            List<DishCategory> cats = new List<DishCategory>(datas.Count);
            for (int i = 0; i < datas.Count; i++)
            {
                var dishJson = datas[i];
                var cat = new DishCategory();
                cats.Add(cat);
                cat.Name = dishJson.Value<string>("name");
                cat.Sequence = dishJson.Value<int>("sequence");               
            }
            return cats;
        }

        public string UploadDishPicture(int erpStoreId, String token, string imageName, byte[] picContent)
        {
            var postDict = new SortedDictionary<string, string>();
            postDict["appAuthToken"] = token;
            postDict["charset"] = "utf-8";
            postDict["timestamp"] = (Helper.ConvertDateTimeInt(DateTime.Now)).ToString();
            postDict["sign"] = MeituanHelper.Sign(postDict, this.Config.SignKey);
            StringBuilder url = new StringBuilder("http://api.open.cater.meituan.com/waimai/image/upload?");
            foreach (var item in postDict)
            {
                url.Append(item.Key);
                url.Append('=');
                url.Append(item.Value);
                url.Append('&');
            }

            var result = Helper.HttpUploadFile(url.ToString(), picContent,"file", imageName, "image/jpeg", new Dictionary<string, string>() {
                { "ePoiId" , erpStoreId.ToString() } ,
                { "imageName" ,imageName }
            });
            var jsonObj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(result);
            Newtonsoft.Json.Linq.JToken errobj;
            if (jsonObj.TryGetValue("error", StringComparison.CurrentCultureIgnoreCase, out errobj))
            {
                throw new Exception(errobj.Value<string>("message"));
            }
            return jsonObj.Value<string>("data");
        }

        public DishInfo GetDish(int erpStoreId, String token, string erpDishId)
        {
            var postDict = new SortedDictionary<string, string>();
            postDict["appAuthToken"] = token;
            postDict["charset"] = "utf-8";
            postDict["timestamp"] = (Helper.ConvertDateTimeInt(DateTime.Now)).ToString();
            postDict["ePoiId"] = erpStoreId.ToString();
            postDict["eDishCodes"] = erpDishId;

            postDict["sign"] = MeituanHelper.Sign(postDict, this.Config.SignKey);
            var result = Helper.PostQueryString("http://api.open.cater.meituan.com/waimai/dish/queryListByEdishCodes", postDict, 8000);
            var jsonObj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(result);
            Newtonsoft.Json.Linq.JToken errobj;
            if (jsonObj.TryGetValue("error", StringComparison.CurrentCultureIgnoreCase, out errobj))
            {
                throw new Exception(errobj.Value<string>("message"));
            }
            var dishJson = ((Newtonsoft.Json.Linq.JArray) jsonObj["data"]["list"])[0];

            var dish = new DishInfo();
            dish.BoxNumber = dishJson.Value<int>("boxNum");
            dish.BoxPrice = dishJson.Value<double>("boxPrice");
            dish.CategoryName = dishJson.Value<string>("categoryName");
            dish.Description = dishJson.Value<string>("description");
            dish.DishName = dishJson.Value<string>("dishName");
            dish.ErpDishId = dishJson.Value<string>("eDishCode");
            dish.Price = dishJson.Value<double>("price");
            dish.Picture = dishJson.Value<string>("picture");
            dish.Sequence = dishJson.Value<int>("Sequence");
            dish.Unit = dishJson.Value<string>("unit");

            var waiMaiDishSkuBases = (Newtonsoft.Json.Linq.JArray)dishJson["skus"];
            for (int j = 0; j < waiMaiDishSkuBases.Count; j++)
            {
                var skuJson = waiMaiDishSkuBases[j];
                var subdish = new DishSkuInfo();
                dish.Skus.Add(subdish);

                subdish.ErpSkuId = skuJson.Value<string>("skuId");
                try
                {
                    subdish.Stock = Convert.ToUInt32(skuJson.Value<string>("stock"));
                }
                catch
                {

                }
                subdish.Price = skuJson.Value<double>("price");
                subdish.Spec = skuJson.Value<string>("spec");
            }
            return dish;
        }

        public void UploadDish(int erpStoreId, String token, List<DishInfo> dishes)
        {
            List<object> attributesToUpload = new List<object>();

            var postDict = new SortedDictionary<string, string>();
            postDict["appAuthToken"] = token;
            postDict["charset"] = "utf-8";
            postDict["timestamp"] = (Helper.ConvertDateTimeInt(DateTime.Now)).ToString();
            postDict["ePoiId"] = erpStoreId.ToString();
            var dishJsonArray = new object[dishes.Count];
            for(int i = 0; i < dishJsonArray.Length; i ++)
            {
                var source = dishes[i];
                var dishJsonObj = new Dictionary<string, object>();
                dishJsonArray[i] = dishJsonObj;

                dishJsonObj["boxNum"] = source.BoxNumber;
                dishJsonObj["boxPrice"] = source.BoxPrice;
                dishJsonObj["categoryName"] = source.CategoryName;
                dishJsonObj["description"] = source.Description;
                dishJsonObj["dishName"] = source.DishName;
                dishJsonObj["EDishCode"] = source.ErpDishId;
                dishJsonObj["epoiId"] = erpStoreId;
                dishJsonObj["isSoldOut"] = 0;
                dishJsonObj["minOrderCount"] = 1;
                if (!string.IsNullOrEmpty(source.Picture))
                {
                    dishJsonObj["picture"] = source.Picture;
                }
                dishJsonObj["price"] = source.Price;
                dishJsonObj["sequence"] = source.Sequence;
                dishJsonObj["unit"] = source.Unit;

                DishInfo serverDish = null;
                try
                {
                    serverDish = GetDish(erpStoreId, token, source.ErpDishId);
                }
                catch
                {

                }
                //删除旧sku
                if(serverDish != null)
                {
                    foreach (var skuObj in serverDish.Skus)
                    {
                        if (!string.IsNullOrEmpty(skuObj.ErpSkuId))
                        {
                            DeleteDishSku(erpStoreId, token, source.ErpDishId, skuObj.ErpSkuId);
                        }
                    }
                }

                var skus = new object[source.Skus.Count];
                dishJsonObj["skus"] = skus;
                for (int j = 0; j < source.Skus.Count; j ++)
                {
                    var subSource = source.Skus[j];
                    var skuJsonObj = new Dictionary<string, object>();
                    skus[j] = skuJsonObj;

                    skuJsonObj["skuId"] = source.ErpDishId + "-" + j;
                    skuJsonObj["spec"] = subSource.Spec;
                    skuJsonObj["stock"] = subSource.Stock!=null? subSource.Stock.ToString() : "*";
                    skuJsonObj["price"] = subSource.Price;

                    if (source.AvailableTimes.Count > 0)
                    {
                        if (source.AvailableTimes.Count != 7 && source.AvailableTimes.Count != 1)
                        {
                            throw new Exception("AvailableTimes必须是1个，或者7个元素");
                        }
                        var sourceAvTimes = new List<DayOpenTime>();
                        sourceAvTimes.AddRange(source.AvailableTimes);
                        if (source.AvailableTimes.Count == 1)
                        {
                            //补够7天
                            for (int k = 0; k < 6; k++)
                            {
                                sourceAvTimes.Add(source.AvailableTimes[0]);
                            }
                        }
                        var availableTimes = new Dictionary<string, string>();
                        skuJsonObj["availableTimes"] = availableTimes;

                        // 填充每天的销售时间
                        for (int k = 0; k < sourceAvTimes.Count; k++)
                        {
                            if (sourceAvTimes[k].Times.Count == 0)
                                continue;
                            var dayofweek = k + 1;
                            if (dayofweek == 7)
                                dayofweek = 0;
                            var name = ((DayOfWeek)dayofweek).ToString().ToLower();
                            StringBuilder timeStr = new StringBuilder();

                            foreach( var time in sourceAvTimes[k].Times)
                            {
                                if (timeStr.Length > 0)
                                    timeStr.Append(',');
                                timeStr.Append(time);
                            }
                            availableTimes[name] = timeStr.ToString();
                        }
                    }
                }

                List<object> dishAttToUpload = new List<object>();
                attributesToUpload.Add(new
                {
                    eDishCode = source.ErpDishId.ToString(),
                    properties = dishAttToUpload
                });
                foreach (var att in source.Attributes )
                {
                    dishAttToUpload.Add(new
                    {
                        propertyName = att.Name,
                        values = att.Values
                    });
                }
            }

            postDict["dishes"] = Newtonsoft.Json.JsonConvert.SerializeObject(dishJsonArray);
            postDict["sign"] = MeituanHelper.Sign(postDict, this.Config.SignKey);
            var result = Helper.PostQueryString("http://api.open.cater.meituan.com/waimai/dish/batchUpload", postDict, 8000);
            var jsonObj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(result);
            Newtonsoft.Json.Linq.JToken errobj;
            if (jsonObj.TryGetValue("error", StringComparison.CurrentCultureIgnoreCase, out errobj))
            {
                throw new Exception(errobj.Value<string>("message"));
            }
            if (!string.Equals(jsonObj.Value<string>("data"), "ok", StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception("发生错误");
            }
            if (attributesToUpload.Count > 0)
            {
                UploadDishAttributes(attributesToUpload,token);
            }
        }

        void UploadDishAttributes(object content,string token)
        {
            var postDict = new SortedDictionary<string, string>();
            postDict["appAuthToken"] = token;
            postDict["charset"] = "utf-8";
            postDict["timestamp"] = (Helper.ConvertDateTimeInt(DateTime.Now)).ToString();
            postDict["dishProperty"] = Newtonsoft.Json.JsonConvert.SerializeObject(content);
            postDict["sign"] = MeituanHelper.Sign(postDict, this.Config.SignKey);
            var result = Helper.PostQueryString("http://api.open.cater.meituan.com/waimai/dish/updateProperty", postDict, 8000);
            var jsonObj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(result);
            Newtonsoft.Json.Linq.JToken errobj;
            if (jsonObj.TryGetValue("error", StringComparison.CurrentCultureIgnoreCase, out errobj))
            {
                throw new Exception(errobj.Value<string>("message"));
            }
            if (!string.Equals(jsonObj.Value<string>("data"), "ok", StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception("发生错误");
            }
        }

        public StoreInfo GetStoreInfo(int erpStoreId, String token)
        {
            var postDict = new SortedDictionary<string, string>();
            postDict["appAuthToken"] = token;
            postDict["ePoiIds"] = erpStoreId.ToString();
            postDict["charset"] = "utf-8";
            postDict["timestamp"] = (Helper.ConvertDateTimeInt(DateTime.Now)).ToString();
            postDict["sign"] = MeituanHelper.Sign(postDict, this.Config.SignKey);
            StringBuilder url = new StringBuilder("http://api.open.cater.meituan.com/waimai/poi/queryPoiInfo?");
            foreach (var item in postDict)
            {
                url.Append(item.Key);
                url.Append('=');
                url.Append(item.Value);
                url.Append('&');
            }

            var result = Helper.GetQueryString(url.ToString(), 8000);

            var jsonObj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(result);
            Newtonsoft.Json.Linq.JToken errobj;
            if (jsonObj.TryGetValue("error", StringComparison.CurrentCultureIgnoreCase, out errobj))
            {
                throw new Exception(errobj.Value<string>("message"));
            }
            var data = ((Newtonsoft.Json.Linq.JArray)jsonObj["data"])[0];
            var storeInfo = new StoreInfo();
            storeInfo.Address = data.Value<string>("address");
            storeInfo.Name = data.Value<string>("name");
            storeInfo.NoticeInfo = data.Value<string>("noticeInfo");
            var phone = data.Value<string>("phone");
            if (!string.IsNullOrEmpty(phone))
            {
                storeInfo.Phones.Add(phone);
            }
            storeInfo.Status = data.Value<int>("isOpen") == 1 ? StoreStatus.Opened : StoreStatus.Closed;
            string[] openTimes = data.Value<string>("openTime").Split(';');
            for(int i = 0; i < openTimes.Length; i ++)
            {
                var daytime = new DayOpenTime();
                storeInfo.DayOpenTimes.Add(daytime);

                string[] times = openTimes[i].Split(',');
                if (times.Length == 1 && times[0] == "00:00-00:00")
                    continue;
                daytime.Times.AddRange(times);
            }
            return storeInfo;
        }
        public void SetStoreStatus(int erpStoreId, String token, StoreStatus status)
        {
            var postDict = new SortedDictionary<string, string>();
            postDict["appAuthToken"] = token;
            postDict["charset"] = "utf-8";
            postDict["timestamp"] = (Helper.ConvertDateTimeInt(DateTime.Now)).ToString();
            postDict["sign"] = MeituanHelper.Sign(postDict, this.Config.SignKey);
            var url = status == StoreStatus.Opened ? "http://api.open.cater.meituan.com/waimai/poi/open" : "http://api.open.cater.meituan.com/waimai/poi/close";
            var result = Helper.PostQueryString(url, postDict, 8000);
            var jsonObj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(result);
            Newtonsoft.Json.Linq.JToken errobj;
            if (jsonObj.TryGetValue("error", StringComparison.CurrentCultureIgnoreCase, out errobj))
            {
                throw new Exception(errobj.Value<string>("message"));
            }
            if (!string.Equals(jsonObj.Value<string>("data"), "ok", StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception("发生错误");
            }
        }
        public void SetStoreOpenTimes(int erpStoreId, String token, List<DayOpenTime> dayOpenTimes)
        {
            var postDict = new SortedDictionary<string, string>();
            postDict["appAuthToken"] = token;
            postDict["charset"] = "utf-8";
            postDict["timestamp"] = (Helper.ConvertDateTimeInt(DateTime.Now)).ToString();
            var openTimeStr = new StringBuilder();
            if(dayOpenTimes.Count == 1)
            {
                var newOpenTimes = new List<DayOpenTime>();
                for (int i = 0; i < 7; i++)
                    newOpenTimes.Add(dayOpenTimes[0]);
                dayOpenTimes = newOpenTimes;
            }
            if (dayOpenTimes.Count != 7)
                throw new Exception("dayOpenTimes必须包含7天的营业时间");
            for(int i = 0; i < dayOpenTimes.Count; i ++)
            {
                var daytime = dayOpenTimes[i];
                if(daytime.Times == null || daytime.Times.Count == 0)
                {
                    openTimeStr.Append($"00:00-00:00");
                }
                else
                {
                    for(int j = 0; j < daytime.Times.Count; j ++)
                    {
                        openTimeStr.Append(daytime.Times[j]);
                        if(j < daytime.Times.Count - 1)
                        {
                            openTimeStr.Append(',');
                        }
                    }
                }
                if(i != 6)
                {
                    openTimeStr.Append(';');
                }
            }
            postDict["openTime"] = openTimeStr.ToString();

            postDict["sign"] = MeituanHelper.Sign(postDict, this.Config.SignKey);

            var result = Helper.PostQueryString("http://api.open.cater.meituan.com/waimai/poi/updateOpenTime", postDict, 8000);
            var jsonObj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(result);
            Newtonsoft.Json.Linq.JToken errobj;
            if (jsonObj.TryGetValue("error", StringComparison.CurrentCultureIgnoreCase, out errobj))
            {
                throw new Exception(errobj.Value<string>("message"));
            }
            if (!string.Equals(jsonObj.Value<string>("data"), "ok", StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception("发生错误");
            }
        }

        public TokenResult UpdateToken(int erpStoreId, string token, string refresh_token)
        {
            throw new Exception("美团不需要更新token");
        }
    }
}
