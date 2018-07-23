using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Jack.Resturant.Impls.Ele
{
    [ResturantType(ResturantPlatformType.Ele)]
    class EleResturant : IResturant
    {
        internal static string ServerUrl
        {
            get
            {
                return $"https://open-api{(Environment.IsTestMode ? "-sandbox" : "")}.shop.ele.me";
            }
        }
        internal static string ApiUrl
        {
            get
            {
                return $"https://open-api{(Environment.IsTestMode ? "-sandbox" : "")}.shop.ele.me/api/v1/";
            }
        }

        public Config Config
        {
            get;
            private set;
        }
        public EleResturant()
        {
            this.Config = new Config(ResturantFactory.ResturantListener.OnGetPlatformConfigXml(ResturantPlatformType.Ele));
        }
        public IEnumerable<string> ShowCallbackUrlSetting(string urlDomain)
        {
            List<string> result = new List<string>();
            var types = typeof(EleResturant).Assembly.GetTypes().Where(m => m.FullName.StartsWith("Jack.Resturant.Impls.Ele.Callbacks")).ToArray();
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

        public string StoreMap(int companyid , int erpStoreId, string erpStoreName)
        {
            var url = ResturantFactory.CurrentDomainUrl;
            if( url.StartsWith("https") == false)
            {
                url = $"https{url.Substring(4)}";
            }
            url += Callbacks.Callback.NotifyPageName;
            url = System.Net.WebUtility.UrlEncode(url);
            return $"{ServerUrl}/authorize?response_type=code&client_id={this.Config.Key}&redirect_uri={url}&state={companyid}_{erpStoreId}&scope=all";
        }
        public string ReleaseStoreMap(string token, int erpStoreId)
        {
            int shopid = this.getShopIdByErpStoreId(token, erpStoreId);
            var myShopList = getShops(token);
           
            if (myShopList.Count == 1)//确定这是最后一个映射的门店了
            {
                //如果最后一个门店都已经解除，那么，需要返回一个url，让用户去点击【取消授权】按钮，这样，饿了么就完全和本系统解除关系了
                var url = ResturantFactory.CurrentDomainUrl;
                if (url.StartsWith("https") == false)
                {
                    url = $"https{url.Substring(4)}";
                }
                url += Callbacks.Callback.NotifyPageName + "?erpShopId=" + erpStoreId + "&token=" + token;
                url = System.Net.WebUtility.UrlEncode(url);
                return $"{ServerUrl}/authorize?response_type=code&client_id={this.Config.Key}&redirect_uri={url}&state=unbind&scope=all";
            }
            else
            {
                this.UnBindShop(token, erpStoreId, shopid);
                ResturantFactory.ResturantListener.OnReleaseStoreMapSuccess(ResturantPlatformType.Ele, erpStoreId);
                return null;
            }
        }
        Newtonsoft.Json.Linq.JToken Post(string token,string action,Dictionary<string,object> param)
        {
            var postDict = new SortedDictionary<string, object>();
            postDict["nop"] = "1.0.0";
            postDict["id"] = Guid.NewGuid().ToString().ToLower();
            postDict["metas"] = new Dictionary<string, object> { { "app_key", Config.Key }, { "timestamp", Helper.ConvertDateTimeInt(DateTime.Now) } };
            postDict["action"] = action;
            postDict["token"] = token;
            postDict["params"] = param == null ? new Dictionary<string, object>() : param;
            postDict["signature"] = EleHelper.Sign(Config, token, action, postDict);

            var result = Helper.PostJson(ApiUrl, postDict, 8000);
            var jsonObj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(result);
            if (jsonObj["error"].HasValues)
            {
                if (jsonObj["error"] is Newtonsoft.Json.Linq.JObject)
                {
                    throw new Exception(((Newtonsoft.Json.Linq.JObject)jsonObj["error"]).Value<string>("message"));
                }
                throw new Exception(jsonObj.Value<string>("error"));
            }
            return (Newtonsoft.Json.Linq.JToken)jsonObj["result"];
        }


        public List<Shop> getShops(string token)
        {
            var jsonObj = Post(token, "eleme.user.getUser", null);
            var jsonArr =(Newtonsoft.Json.Linq.JArray) jsonObj["authorizedShops"];
            List<Shop> result = new List<Shop>();
            foreach ( var shopitemJson in jsonArr )
            {
                result.Add(new Shop() {
                    id = shopitemJson.Value<int>("id"),
                    Name = shopitemJson.Value<string>("name")
                });
            }
            return result;
        }
        public string getErpShopIdByShopId(string token , string shopid)
        {
            var param = new Dictionary<string, object>();
            param["shopId"] = shopid;
            var jsonObj = Post(token, "eleme.shop.getShop", param);
            return jsonObj.Value<string>("openId");
        }
        public int getShopIdByErpStoreId(string token,int erpStoreId)
        {
            var shops = getShops(token);
            foreach( var shop in shops )
            {
                int shopid = shop.id;

                var param = new Dictionary<string, object>();
                param["shopId"] = shopid;
                var jsonObj = Post(token, "eleme.shop.getShop", param);
                int openid = int.Parse(jsonObj.Value<string>("openId"));
                if (openid == erpStoreId)
                    return shopid;
            }
            return 0;
        }

        public void BindShop(string token , int erpStoreId,int eleShopId)
        {
            var param = new Dictionary<string, object>();
            param["shopId"] = eleShopId;
            param["properties"] = new Dictionary<string, object>() {{ "openId" , erpStoreId}};
            var jsonObj = Post(token, "eleme.shop.updateShop", param);
        }
        public void UnBindShop(string token, int erpStoreId, int eleShopId)
        {
            var param = new Dictionary<string, object>();
            param["shopId"] = eleShopId;
            param["properties"] = new Dictionary<string, object>() { { "openId", "" } };
            var jsonObj = Post(token, "eleme.shop.updateShop", param);
        }
        public TokenResult UpdateToken(int erpStoreId, string token,string refresh_token)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers["Authorization"] = "Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{this.Config.Key}:{this.Config.Secret}"));
            string query = $"grant_type=refresh_token&refresh_token={refresh_token}";

            string result = Helper.PostQueryString($"{EleResturant.ServerUrl}/token", headers, query, 8000);
            var jsonObj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(result);
            string access_token = jsonObj.Value<string>("access_token");
            int expires_in = jsonObj.Value<int>("expires_in");
            refresh_token = jsonObj.Value<string>("refresh_token");

            ResturantFactory.ResturantListener.OnCompanyMapSuccess(ResturantPlatformType.Ele, 0,new StoreMapInfo[]{ new StoreMapInfo {
                ErpStoreId = erpStoreId,
                Expires = DateTime.Now.AddSeconds(expires_in),
                Token = access_token,
                Refresh_token = refresh_token
            } });

            return new TokenResult() {
                Expires = DateTime.Now.AddSeconds(expires_in),
                Token = access_token,
                RefreshToken = refresh_token,
            };
        }
       
        public void ConfirmOrder(ConfirmOrderParameter parameter)
        {
            var param = new Dictionary<string, object>();
            param["orderId"] = parameter.OrderID;
            var jsonObj = Post(parameter.Token, "eleme.order.confirmOrderLite", param);
        }

        public void CancelOrder(CancelOrderParameter parameter)
        {
            var param = new Dictionary<string, object>();
            param["orderId"] = parameter.OrderID;
            switch(parameter.Reason)
            {
                case CancelOrderReason.AddressError:
                    param["type"] = "distanceTooFar";
                    break;
                case CancelOrderReason.Busy:
                    param["type"] = "restaurantTooBusy";
                    break;
                case CancelOrderReason.CanNotContact:
                    param["type"] = "contactUserFailed";
                    break;
                case CancelOrderReason.CustomerCancel:
                    param["type"] = "forceRejectOrder";
                    break;
                case CancelOrderReason.SoldOut:
                    param["type"] = "foodSoldOut";
                    break;
                case CancelOrderReason.StoreClosed:
                    param["type"] = "restaurantClosed";
                    break;
                default:
                    param["type"] = "others";
                    break;
            }
            var jsonObj = Post(parameter.Token, "eleme.order.cancelOrderLite", param);
        }

        public void Delivering(DeliverParameter parameter)
        {
            var param = new Dictionary<string, object>();
            param["orderId"] = parameter.OrderID;
            var jsonObj = Post(parameter.Token, "eleme.order.deliveryBySelfLite", param);
        }

        public void Delivered(DeliveredParameter parameter)
        {
            var param = new Dictionary<string, object>();
            param["orderId"] = parameter.OrderID;
            var jsonObj = Post(parameter.Token, "eleme.order.receivedOrderLite", param);
        }
        public List<DishInfo> GetDishListByCategoryId(int erpStoreId,object categoryId,string catName, string token)
        {
            int index = 0;
            List<DishInfo> dishes = new List<DishInfo>();
            var param = new Dictionary<string, object>();
            param["categoryId"] = categoryId;
            var result = Post(token, "eleme.product.item.getItemsByCategoryId", param);
            var cur = result.First;
            while (cur != null && cur.First != null)
            {
                var json = cur.First;
                var dish = new DishInfo()
                {
                    Sequence = index++,
                    CategoryName = catName,
                    DishId = json.Value<string>("id"),
                    DishName = json.Value<string>("name"),
                };
                dishes.Add(dish);
                var specs = (Newtonsoft.Json.Linq.JArray)json["specs"];
                foreach (var specJson in specs)
                {
                    DishSkuInfo sku = new DishSkuInfo();
                    dish.Skus.Add(sku);
                    sku.Spec = specJson.Value<string>("name");
                    sku.Price = specJson.Value<double>("price");
                    sku.SkuId = specJson.Value<Int64>("specId");
                    sku.ErpSkuId = specJson.Value<string>("extendCode");
                    if (dish.Price == 0)
                        dish.Price = sku.Price;

                }
                var attributes = (Newtonsoft.Json.Linq.JArray)json["attributes"];
                foreach (var attJson in attributes)
                {
                    DishAttribute att = new DishAttribute();
                    dish.Attributes.Add(att);
                    att.Name = attJson.Value<string>("name");
                    var details = (Newtonsoft.Json.Linq.JArray)attJson["details"];
                    foreach (var item in details)
                    {
                        att.Values.Add(item.ToString());
                    }
                }
                try
                {
                    var sellingTime = json["sellingTime"];
                    var weeks = (Newtonsoft.Json.Linq.JArray)sellingTime["weeks"];
                    var times = (Newtonsoft.Json.Linq.JArray)sellingTime["times"];
                    if (weeks.Count > 0)
                    {
                        for(int i = 0; i < 7; i ++)
                        {
                            dish.AvailableTimes.Add(new DayOpenTime());
                        }
                        var strArr = new List<string>( new string[] { "MONDAY", "TUESDAY", "WEDNESDAY", "THURSDAY", "FRIDAY", "SATURDAY", "SUNDAY" });
                        for (int i = 0; i < weeks.Count; i ++)
                        {
                            var day = weeks[i].ToString();
                            var dayIndex = strArr.IndexOf(day);
                            if(dayIndex >= 0)
                            {
                                var targetTimes = dish.AvailableTimes[dayIndex].Times;
                                if (times.Count > 0)
                                {
                                    foreach (var timeJson in times)
                                    {
                                        targetTimes.Add(timeJson["beginTime"] + "-" + timeJson["endTime"]);
                                    }
                                }
                                else
                                {
                                    //全时段
                                    targetTimes.Add("00:00-23:59");
                                }
                            }
                        }
                    }
                }
                catch
                {
                }
                cur = cur.Next;
            }
            return dishes;
        }
         public List<DishInfo> GetDishList(int erpStoreId, string token)
        {
            var categories = this.GetDishCategoryList(erpStoreId, token);
            List<DishInfo> dishes = new List<DishInfo>();
            foreach( var cat in categories )
            {
                dishes.AddRange( GetDishListByCategoryId(erpStoreId , cat.CategoryId , cat.Name , token) );
            }
            return dishes;
        }

        public void DishMap(DishMapParameter parameter)
        {
            //饿了么这个方法不用实现，因为饿了么的菜品里面没有erpDishId属性，只有sku里面有extendCode
            //erp方面，只要自己记录下来，菜品对应的饿了么菜品id是什么即可，因为饿了么关于菜品的操作，都是基于饿了么自己的菜品id进行
        }

      
        public void DeleteDish(int erpStoreId, string token, string erpDishId, string dishId)
        {
            //删除商品
            var param = new Dictionary<string, object>();
            param["itemId"] = dishId;
            Post(token, "eleme.product.item.removeItem", param);
        }

        public string UploadDishPicture(int erpStoreId, string token, string imageName, byte[] picContent)
        {
            var param = new Dictionary<string, object>();
            param["image"] = Convert.ToBase64String(picContent);
            var result = Post(token, "eleme.file.uploadImage", param);
            return result.ToString();
        }

        public void CreateDish(string token,Int64 categoryId, DishInfo dish )
        {
            UpdateDish(token, "eleme.product.item.createItem", 0, categoryId, dish);
        }
        public void UpdateDish(string token, string action, long dishid, Int64 categoryId, DishInfo dish)
        {
            var param = new Dictionary<string, object>();
            if (dishid != 0)
            {
                param["itemId"] = dishid;
            }
            param["categoryId"] = categoryId;
            var properties = new Dictionary<string, object>();
            param["properties"] = properties;

            properties["name"] = dish.DishName;
            properties["description"] = dish.Description;
            if (!string.IsNullOrEmpty(dish.Picture))
            {
                properties["imageHash"] = dish.Picture;
            }
            //设置销售时间
            if(dish.AvailableTimes.Count > 0)
            {
                var sellingTime = new Dictionary<string, object>();
                properties["sellingTime"] = sellingTime;
                if( dish.AvailableTimes.Count != 7 && dish.AvailableTimes.Count != 1 )
                {
                    throw new Exception("AvailableTimes必须是1个，或者7个元素");
                }

                //设置星期几可以销售
                var strArr = new string[] { "MONDAY", "TUESDAY", "WEDNESDAY", "THURSDAY", "FRIDAY", "SATURDAY", "SUNDAY" };
                if ( dish.AvailableTimes.Count == 1 )
                {
                    sellingTime["weeks"] = strArr;
                }
                else
                {
                    List<string> weeks = new List<string>();
                    sellingTime["weeks"] = weeks;
                    for (int i = 0; i < 7; i ++)
                    {
                        if( dish.AvailableTimes[i].Times.Count > 0 )
                        {
                            weeks.Add(strArr[i]);
                        }
                    }
                }

                sellingTime["beginDate"] = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd");
                sellingTime["endDate"] = DateTime.Now.AddYears(100).ToString("yyyy-MM-dd");

                //设置每天销售时间
                var times = new List<object>();
                sellingTime["times"] = times;
                var dayTime = dish.AvailableTimes.FirstOrDefault(m => m.Times.Count > 0);
                if (dayTime != null)
                {
                    for (int i = 0; i < 3 && i < dayTime.Times.Count; i++)
                    {
                        var begin_end = dayTime.Times[i].Split('-');
                        times.Add(new {
                            beginTime = begin_end[0].Trim(),
                            endTime = begin_end[1].Trim(),
                        });
                    }
                }
            }

            var specs = new List<object>();
            properties["specs"] = specs;
            foreach (var sku in dish.Skus)
            {
                specs.Add(new
                {
                    name = sku.Spec,
                    price = sku.Price,
                    packingFee = dish.BoxNumber * dish.BoxPrice,
                    onShelf = 1,
                    extendCode = sku.ErpSkuId,
                    stock = sku.Stock == null ? 10000 : sku.Stock,
                    maxStock = 10000,
                });
            }

            var attributes = new List<object>();
            properties["attributes"] = attributes;
            foreach (var att in dish.Attributes)
            {
                attributes.Add(new
                {
                    name = att.Name,
                    details = att.Values
                });
            }
            properties["unit"] = dish.Unit;
            var result = Post(token, action, param);
        }
        public void UploadDish(int erpStoreId, string token, List<DishInfo> dishes)
        {
            foreach (var dish in dishes)
            {
                if (dish.Skus.Count == 0)
                {
                    throw new Exception($"{dish.DishName}不包含任何Sku");
                }
            }
            var categories = this.GetDishCategoryList(erpStoreId, token);
            foreach( var dish in dishes )
            {
             
                var cat = categories.FirstOrDefault(m => string.Equals(m.Name,dish.CategoryName, StringComparison.CurrentCultureIgnoreCase));
                long dishid = Convert.ToInt64(dish.DishId);
                if (cat == null)
                {
                    //添加这个分类
                    throw new Exception($"请先添加分类：{dish.CategoryName}");
                }
                if(dishid == 0)
                {
                    CreateDish(token,Convert.ToInt64( cat.CategoryId), dish);
                }
                else
                {
                    UpdateDish(token, "eleme.product.item.updateItem" , dishid, Convert.ToInt64(cat.CategoryId), dish);
                }
            }

            //把菜按类别分组
            var groups = (from m in dishes
                          group m by m.CategoryName into g
                          select new {
                              CategoryName = g.Key,
                              List = g.ToArray()
                          }).ToArray();
            foreach( var group in groups )
            {
                var cat = categories.FirstOrDefault(m => string.Equals(m.Name, group.CategoryName, StringComparison.CurrentCultureIgnoreCase));
                //获取此分类下所有菜品
                var serverGroupDishes = GetDishListByCategoryId(erpStoreId, cat.CategoryId, group.CategoryName, token);
                //对这些菜品重新排序
                bool seqChanged = false;
                //把upload菜品的排序号，赋给服务器的菜品
                foreach( var newItem in group.List )
                {
                    var serverItem = serverGroupDishes.FirstOrDefault(m => m.ErpDishId == newItem.ErpDishId);
                    if (serverItem != null && serverItem.Sequence != newItem.Sequence)
                    {
                        serverItem.Sequence = newItem.Sequence;
                        seqChanged = true;
                    }
                }
                if (seqChanged)//如果排序发生更改，修改排序
                {
                    //重新排序，拿到正确顺序id
                    var newSequenceIdArr = serverGroupDishes.OrderBy(m => m.Sequence).Select(m => m.DishId).ToArray();
                    var param = new Dictionary<string, object>();
                    param["categoryId"] = cat.CategoryId;
                    param["itemIds"] = newSequenceIdArr;
                    Post(token, "eleme.product.item.setItemPositions", param);
                }
            }
        }


        public void DeleteDishCategory(int erpStoreId, string token, string categoryName)
        {
            var categories = this.GetDishCategoryList(erpStoreId, token);
            var cat = categories.FirstOrDefault(m => m.Name == categoryName);
            if(cat != null)
            {
                var param = new Dictionary<string, object>();
                param["categoryId"] = cat.CategoryId;
                Post(token, "eleme.product.category.removeCategory", param);
            }
        }

        public void CreateDishCategory(CreateDishCategoryParameter parameter)
        {
            var updateParameter = new UpdateDishCategoryParameter();
            updateParameter.ErpStoreId = parameter.ErpStoreId;
            updateParameter.CategoryName = parameter.CategoryName;
            updateParameter.Sequence = parameter.Sequence;
            updateParameter.Token = parameter.Token;
            UpdateDishCategory(updateParameter);
        }

        public void UpdateDishCategory(UpdateDishCategoryParameter parameter)
        {
            var shopid = this.getShopIdByErpStoreId(parameter.Token ,  parameter.ErpStoreId);
            var categories = this.GetDishCategoryList(parameter.ErpStoreId, parameter.Token);
            var original = categories.FirstOrDefault(m => m.Name == parameter.OriginalCategoryName);
            int id;
            bool isNew = false;
            if( parameter.OriginalCategoryName == null || original == null )
            {
                //添加商品
                isNew = true;
                var param = new Dictionary<string, object>();
                param["shopId"] = shopid;
                param["name"] = parameter.CategoryName;
                var result = this.Post(parameter.Token, "eleme.product.category.createCategory", param);
                id = result.Value<int>("id");
                DishCategory newCategory = new DishCategory() { CategoryId = id};
                if (parameter.Sequence < categories.Count)
                    categories.Insert((int)parameter.Sequence, newCategory);
                else
                    categories.Add(newCategory);
            }
            else
            {
                //修改商品
                id = Convert.ToInt32( original.CategoryId);
                var param = new Dictionary<string, object>();
                param["categoryId"] = id;
                param["name"] = parameter.CategoryName;
                this.Post(parameter.Token, "eleme.product.category.updateCategory", param);
            }
            //设置排序
            if( isNew || parameter.Sequence != original.Sequence)
            {
                var param = new Dictionary<string, object>();
                param["categoryIds"] = categories.Select(m=>m.CategoryId).ToArray();
                param["shopId"] = shopid;
                this.Post(parameter.Token, "eleme.product.category.setCategoryPositions", param);
            }
        }

        public List<DishCategory> GetDishCategoryList(int erpStoreId, string token)
        {
            int shopid = getShopIdByErpStoreId(token, erpStoreId);
            var param = new Dictionary<string, object>();
            param["shopId"] = shopid;
            var jsonArr = (Newtonsoft.Json.Linq.JArray)(object)Post(token, "eleme.product.category.getShopCategories", param);
            var result = new List<DishCategory>();
            int order = 0;
            foreach ( var jsonItem in jsonArr )
            {
                var category = new DishCategory() {
                    Name = jsonItem.Value<string>("name"),
                    Sequence = order++,
                    CategoryId = jsonItem.Value<int>("id")
                };
                result.Add(category);

            }
            return result;
        }

    
        public StoreInfo GetStoreInfo(int erpStoreId, String token)
        {
            var shopid = getShopIdByErpStoreId(token, erpStoreId);
            var param = new Dictionary<string, object>();
            param["shopId"] = shopid;
            var resultJson = Post(token, "eleme.shop.getShop", param);
            var info = new StoreInfo() {
                Address = resultJson.Value<string>("addressText"),
                Name = resultJson.Value<string>("name"),
                NoticeInfo = resultJson.Value<string>("promotionInfo"),
                Status = resultJson.Value<int>("isOpen") == 1 ? StoreStatus.Opened : StoreStatus.Closed
            };
            var phoneArr =(Newtonsoft.Json.Linq.JArray) resultJson["phones"];
            foreach (var phone in phoneArr)
            {
                var strPhone = phone.ToString();
                if (!string.IsNullOrEmpty(strPhone))
                {
                    info.Phones.Add(strPhone);
                }
            }
            var servingTimeArr = (Newtonsoft.Json.Linq.JArray)resultJson["servingTime"];
            var dayTime = new DayOpenTime();
            info.DayOpenTimes.Add(dayTime);

            foreach (var timestr in servingTimeArr)
            {
                dayTime.Times.Add(timestr.ToString());
            }
            return info;
        }
        public void SetStoreStatus(int erpStoreId, String token, StoreStatus status)
        {
            var shopid = getShopIdByErpStoreId(token, erpStoreId);
            var param = new Dictionary<string, object>();
            param["shopId"] = shopid;
            param["properties"] = new {
                isOpen = status == StoreStatus.Opened ? 1 : 0
            };
            Post(token, "eleme.shop.updateShop", param);
        }
        public void SetStoreOpenTimes(int erpStoreId, String token, List<DayOpenTime> dayOpenTimes)
        {
            var daytime = dayOpenTimes[0];
            StringBuilder openTimeStr = new StringBuilder();
            for (int j = 0; j < daytime.Times.Count; j++)
            {
                openTimeStr.Append(daytime.Times[j]);
                if (j < daytime.Times.Count - 1)
                {
                    openTimeStr.Append(',');
                }
            }

            var shopid = getShopIdByErpStoreId(token, erpStoreId);
            var param = new Dictionary<string, object>();
            param["shopId"] = shopid;
            param["properties"] = new
            {
                openTime = openTimeStr.ToString()
            };
            Post(token, "eleme.shop.updateShop", param);
        }

        public void ArgeeOrderRefund(string token, string orderId)
        {
            var param = new Dictionary<string, object>();
            param["orderId"] = orderId;
            Post(token, "eleme.order.agreeRefundLite", param);
        }
        public void DisargeeOrderRefund(string token, string orderId,string reason)
        {
            var param = new Dictionary<string, object>();
            param["orderId"] = orderId;
            param["reason"] = string.IsNullOrEmpty(reason) ? "拒绝" : reason;
            Post(token, "eleme.order.disagreeRefundLite", param);
        }


        public ServiceAmountInfo GetOrderServiceAmount(string token, string orderId)
        {
            var param = new Dictionary<string, object>();
            param["orderId"] = orderId;
            var result = Post(token, "eleme.order.getOrder", param);
            return new ServiceAmountInfo() { PlatformServiceAmount = result.Value<double>("serviceFee"),SettleAmount= result.Value<double>("income") };
        }
    }

}
