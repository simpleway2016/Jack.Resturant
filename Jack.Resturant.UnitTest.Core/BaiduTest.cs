using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jack.Resturant.UnitTest.Core
{
    [TestClass]
    public class BaiduTest : IResturantListener
    {
        const int ErpShopId = 99999;
        public BaiduTest()
        {
            ResturantFactory.Enable(this, null, false);
        }
        public void OnCancelOrderRefund(ResturantPlatformType platformType, OrderRefundInfo info)
        {
            throw new NotImplementedException();
        }

        public void OnCompanyMapSuccess(ResturantPlatformType platformType, int companyId, StoreMapInfo[] mapinfos)
        {
            throw new NotImplementedException();
        }

        public string OnGetPlatformConfigXml(ResturantPlatformType platformType)
        {
            return @"<?xml version=""1.0"" encoding=""utf-8""?>           
<r>
    <Source>63968</Source>
    <Secret>27051c3adcf34097</Secret>
<StoreMapSource>D8E7CE6CF9ECD7D5F396B1E0BB366A5A9614A7F101CAA7AFFD666A2282894D07</StoreMapSource>
</r>";
        }

        public List<Shop> OnGetShopList(ResturantPlatformType platformType, int companyId)
        {
            throw new NotImplementedException();
        }

        public void OnOrderCancel(ResturantPlatformType platformType, OrderCancelInfo orderInfo)
        {
            throw new NotImplementedException();
        }

        public void OnOrderFinish(ResturantPlatformType platformType, string orderId)
        {
            throw new NotImplementedException();
        }

        public void OnOrderRefund(ResturantPlatformType platformType, OrderRefundInfo info)
        {
            throw new NotImplementedException();
        }

        public void OnReceiveNewOrder(ResturantPlatformType platformType, OrderInfo orderInfo)
        {
            throw new NotImplementedException();
        }

        public void OnStoreMapSuccess(ResturantPlatformType platformType, StoreMapInfo mapinfo)
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void CallbackSign()
        {
            var jsonStr = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\Baidu_callback.json");
            var forms = Newtonsoft.Json.JsonConvert.DeserializeObject<SortedDictionary<string, object>>(jsonStr);
            var type = typeof(Jack.Resturant.ResturantFactory).Assembly.GetType("Jack.Resturant.Impls.Baidu.Callbacks.Callback");
            var obj = Activator.CreateInstance(type);
            var method = type.GetMethod("checkSign" , System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var result = method.Invoke(obj , new object[] { forms });
        }

        [TestMethod]
        public void test()
        {
            Jack.Resturant.Impls.Baidu.BaiduResturant baidu = new Impls.Baidu.BaiduResturant();
            var dict = new Dictionary<string, object>();
            dict["baidu_shop_id"] = "2147052144";
            List<object> delivery_region = new List<object>();
            delivery_region.Add(new
            {
                name = "西二旗配送区",
                delivery_time = "60",
                delivery_fee = "600",
                min_buy_free = 6000,
                min_order_price = 1000,
                region = new object[] {
                    new object[]{
                        new { latitude = "39.988619",longitude="116.280034"},//116.280034,39.988619
                         new { latitude = "39.997906",longitude="116.58014"},//116.58014,39.997906
                          new { latitude = "39.824791",longitude="116.635907"},//116.635907,39.824791
                           new { latitude = "39.836316",longitude="116.246401"},//116.246401,39.836316
                            new { latitude = "39.988619",longitude="116.280034"}
                    }
                },
            });
            dict["delivery_region"] = delivery_region;

            var result = baidu.Post("shop.update", dict);

        }

        [TestMethod]
        public void GetOrder()
        {
            //
            Jack.Resturant.Impls.Baidu.BaiduResturant baidu = new Impls.Baidu.BaiduResturant();
            var result = baidu.GetOrder("15101281300071");
        }
        [TestMethod]
        public void Close()
        {
            //
            Jack.Resturant.Impls.Baidu.BaiduResturant baidu = new Impls.Baidu.BaiduResturant();
            baidu.SetStoreStatus(112 , null, StoreStatus.Opened);
        }
        [TestMethod]
        public void ConfirmOrder()
        {
            //15143471287383
            Jack.Resturant.Impls.Baidu.BaiduResturant baidu = new Impls.Baidu.BaiduResturant();
            baidu.ConfirmOrder(new ConfirmOrderParameter() {
                OrderID = "15143471287383"
            });
        }

        [TestMethod]
        public void UploadDish()
        {
            Jack.Resturant.Impls.Baidu.BaiduResturant baidu = new Impls.Baidu.BaiduResturant();
            var dishes = new List<DishInfo>();
            var dish = new DishInfo();
            dishes.Add(dish);
            dish.Attributes.Add(new DishAttribute() {
                Name = "辣度",
                Values = new List<string>( new string[] { "微辣","超辣"})
            });
            dish.Attributes.Add(new DishAttribute()
            {
                Name = "甜度",
                Values = new List<string>(new string[] { "无糖", "少糖" })
            });
            dish.CategoryName = "凉菜";
            dish.Description = "我的测试蔡";
            dish.ErpDishId = "2";
            dish.DishName = "宫保鸡丁";
            dish.Price = 18;
            dish.Sequence = 0;
            dish.Unit = "份";
            dish.Picture = "https://img.waimai.baidu.com/pb/996b20261afba57226d98b22ae1d93b528";
            dish.Skus.Add(new DishSkuInfo() {
                Price = 30,
                Spec = "大份",
                Stock =8888
            });
            dish.Skus.Add(new DishSkuInfo()
            {
                Price = 18,
                Spec = "小份",
                Stock = 8888
            });
            for (int i = 0; i < 7; i++)
            {
                dish.AvailableTimes.Add(new DayOpenTime()
                {
                    Times = new List<string>(new string[] { "08:00-13:00", "14:30-19:00" })
                });
            }
            dish.AvailableTimes[3].Times[0] = "08:00-10:00";
           // baidu.GetDish(1, "1510018544");

            baidu.UploadDish(ErpShopId, null, dishes);
        }

        [TestMethod]
        public void CancelOrder()
        {
            //
            Jack.Resturant.Impls.Baidu.BaiduResturant baidu = new Impls.Baidu.BaiduResturant();
            baidu.CancelOrder(new CancelOrderParameter() {
                OrderID = "15143471287383",
                Reason = CancelOrderReason.AddressError
            });
        }
        [TestMethod]
        public void DeleteDish()
        {
            //

        }

        [TestMethod]
        public void GetDishCategoryList()
        {
            //
            Jack.Resturant.Impls.Baidu.BaiduResturant baidu = new Impls.Baidu.BaiduResturant();
            var result = baidu.GetDishCategoryList(ErpShopId, null);
        }

        [TestMethod]
        public void CreateDishCategory()
        {
            var baidu = new Impls.Baidu.BaiduResturant();
            baidu.CreateDishCategory(new CreateDishCategoryParameter() {
                CategoryName = "凉菜2",
                ErpStoreId = ErpShopId,
                Sequence = 1
            });
        }

        [TestMethod]
        public void UpdateDishCategory()
        {
            var baidu = new Impls.Baidu.BaiduResturant();
            baidu.UpdateDishCategory(new UpdateDishCategoryParameter() {
                CategoryName = "凉菜3",
                ErpStoreId = ErpShopId,
                OriginalCategoryName = "凉菜2",
                Sequence = 2
            });
        }

        [TestMethod]
        public void DeleteDishCategory()
        {
            var baidu = new Impls.Baidu.BaiduResturant();
            baidu.DeleteDishCategory(ErpShopId, null , "凉菜3");
        }

        [TestMethod]
        public void GetDishList()
        {
            var baidu = new Impls.Baidu.BaiduResturant();
            var result = baidu.GetDishList(ErpShopId, null);
        }

        [TestMethod]
        public void DishMap()
        {
            var baidu = new Impls.Baidu.BaiduResturant();
            var dishes = baidu.GetDishList(ErpShopId, null);
            var parameter = new DishMapParameter();
            parameter.ErpStoreID = ErpShopId;
            foreach( var dish in dishes )
            {
                parameter.DishMapItems.Add(new DishMapParameter.DishMapItem() {
                    DishId = dish.DishId,
                    ErpDishId = (Convert.ToInt32( dish.ErpDishId) + 100).ToString()
                });
            }

            baidu.DishMap(parameter);
        }

        [TestMethod]
        public void GetStoreInfo()
        {
            var baidu = new Impls.Baidu.BaiduResturant();
            var result = baidu.GetStoreInfo(ErpShopId, null);
        }

        [TestMethod]
        public void SetStoreOpenTimes()
        {
            var baidu = new Impls.Baidu.BaiduResturant();
            baidu.SetStoreOpenTimes(ErpShopId, null , new List<DayOpenTime>() {
                new DayOpenTime(){
                    Times = new List<string>(new string[]{"02:00-11:00" , "12:30-20:00" })
                }
            });
        }

        public string OnGetPlatformToken(ResturantPlatformType platformType, string orderId)
        {
            throw new NotImplementedException();
        }

        public void OnReceiveOrderSettlement(ResturantPlatformType platformType, string orderId, ServiceAmountInfo settleAmount)
        {
            throw new NotImplementedException();
        }

        public void OnOrderRefundCompleted(ResturantPlatformType platformType, OrderRefundInfo info)
        {
            throw new NotImplementedException();
        }

        public void OnReleaseStoreMapSuccess(ResturantPlatformType platformType, int erpStoreId)
        {

        }
    }
}
