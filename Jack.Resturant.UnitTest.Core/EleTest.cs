using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Jack.Resturant.UnitTest.Core
{
    [TestClass]
    public class EleTest:IResturantListener
    {
        string Token = "87f418b68c1af41b510b4269435c8a63";
        int ErpStoreId = 894;
        public EleTest()
        {
            ResturantFactory.Enable(this, null, true);
        }

        public void OnCompanyMapSuccess(ResturantPlatformType platformType, int companyId, StoreMapInfo[] mpainfos)
        {
           
        }

        public string OnGetPlatformConfigXml(ResturantPlatformType platformType)
        {
            if (platformType == ResturantPlatformType.Meituan)
                return @"<?xml version=""1.0"" encoding=""utf-8""?>           
<r>
    <developerId>101572</developerId>
    <SignKey>q25w5ibpttho0xmb</SignKey>
</r>";
            else if (platformType == ResturantPlatformType.Ele)
            {
                return @"<?xml version=""1.0"" encoding=""utf-8""?>           
<r>
    <Key>h5JvsZ7aCZ</Key>
    <Secret>b7d6dd01f0f8b91f797b03f78c23bdf56e1f96ba</Secret>
</r>";
            }

            return null;
        }

        public List<Shop> OnGetShopList(ResturantPlatformType platformType, int companyId)
        {
            return new List<Shop>();
        }

        public void OnOrderCancel(ResturantPlatformType platformType, OrderCancelInfo orderInfo)
        {
           
        }

        public void OnReceiveNewOrder(ResturantPlatformType platformType, OrderInfo orderInfo)
        {
        }

        public void OnStoreMapSuccess(ResturantPlatformType platformType, StoreMapInfo mapinfo)
        {
 
        }

        [TestMethod]
        public void UpdateToken()
        {
            /*
             2018/2/24 14:00:14 发现过期token，{"ShopTokenId":6,"ShopId":898,"CurrentToken":"e34ecfe86567bc6e0ed7bb007368ff3f","RefreshToken":"6feadf112824492d0eb64368561ea106","ExpireTime":"2018-02-16T18:30:48.267","PlatformName":"Ele"}，准备更新
2018/2/24 14:00:14 发现过期token，{"ShopTokenId":8,"ShopId":897,"CurrentToken":"590dcf731fafb8009bc45181e6a153a0","RefreshToken":"ff559ae01a8b8df241f8abfbc4bcde15","ExpireTime":"2018-02-17T15:40:28.023","PlatformName":"Ele"}，准备更新
             */
            var resturant = ResturantFactory.CreateResturant(ResturantPlatformType.Ele);
            
            var result = resturant.UpdateToken(0, null, "267514cb84c44df1d012dd7f62860118");
        }


        [TestMethod]
        public void StoreMapTest()
        {
            var resturant = ResturantFactory.CreateResturant(ResturantPlatformType.Ele);
            var url = resturant.StoreMap(0, 1, "");
        }


        [TestMethod]
        public void GetDishCategoryList()
        {
            var resturant = ResturantFactory.CreateResturant(ResturantPlatformType.Ele);
            var result = resturant.GetDishCategoryList(ErpStoreId, Token);
        }

        [TestMethod]
        public void UpdateDishCategory()
        {
            var resturant = ResturantFactory.CreateResturant(ResturantPlatformType.Ele);
            var updateParameter = new UpdateDishCategoryParameter();
            updateParameter.ErpStoreId = ErpStoreId;
            updateParameter.CategoryName = "东北菜";
            updateParameter.OriginalCategoryName = "东北菜";
            updateParameter.Sequence = 1;
            updateParameter.Token = Token;

            resturant.UpdateDishCategory(updateParameter);
        }

        [TestMethod]
        public void UploadDish()
        {
            var resturant = ResturantFactory.CreateResturant(ResturantPlatformType.Ele);
            List<DishInfo> dishes = new List<DishInfo>();
            var dish = new DishInfo();
            dish.BoxNumber = 1;
            dish.BoxPrice = 0.5;
            dish.Attributes.Add(new DishAttribute() {
                Name = "辣度",
                Values = new List<string>(new string[] { "微辣", "中辣", "超辣" }),
            });
            dish.CategoryName = "东北菜";
            dish.DishName = "东北乱炖";
            dish.ErpDishId = "Donbeiluandun";
            dish.Picture = "6b20261afba57226d98b22ae1d93b528jpeg";
            dish.Price = 28.5;
            dish.Unit = "份";
            dish.Sequence = 1;
            dishes.Add(dish);

            dish = new DishInfo();
            dish.BoxNumber = 1;
            dish.BoxPrice = 0.5;
            dish.Attributes.Add(new DishAttribute()
            {
                Name = "辣度",
                Values = new List<string>(new string[] { "微辣", "中辣", "超辣" }),
            });
            dish.CategoryName = "东北菜";
            dish.DishName = "芹菜粉";
            dish.ErpDishId = "Qincaifen";
            dish.Picture = "6b20261afba57226d98b22ae1d93b528jpeg";
            dish.Price = 16;
            dish.Unit = "份";
            dish.Sequence = 0;
            dishes.Add(dish);

            dish.AvailableTimes.Add(new DayOpenTime() {
                Times = new List<string>(new string[] { "08:00-12:00" , "13:00-19:00"})
            });

            resturant.UploadDish(ErpStoreId, Token, dishes);
        }

        [TestMethod]
        public void UploadDishPicture()
        {
            //6b20261afba57226d98b22ae1d93b528jpeg
            var content = System.IO.File.ReadAllBytes("i:\\a.jpg");
            var resturant = ResturantFactory.CreateResturant(ResturantPlatformType.Ele);
            string hash = resturant.UploadDishPicture(1, Token, "test", content);
        }

        [TestMethod]
        public void GetDishList()
        {
            var resturant = ResturantFactory.CreateResturant(ResturantPlatformType.Ele);
            var result = resturant.GetDishList(ErpStoreId, Token);
            string text = Newtonsoft.Json.JsonConvert.SerializeObject(result);
            System.IO.File.WriteAllText("i:\\dish.txt", text);
        }
        [TestMethod]
        public void ArgeeOrderRefund()
        {
            var resturant = ResturantFactory.CreateResturant(ResturantPlatformType.Ele);
            resturant.ArgeeOrderRefund( Token , "3014502384389684272");
        }
        [TestMethod]
        public void DisargeeOrderRefund()
        {
            var resturant = ResturantFactory.CreateResturant(ResturantPlatformType.Ele);
            resturant.DisargeeOrderRefund(Token, "3014504119514528816", "就是不同意");
        }
        [TestMethod]
        public void DishMap()
        {
            int index = 1;
            var resturant = ResturantFactory.CreateResturant(ResturantPlatformType.Ele);
            var dishes = resturant.GetDishList(1, Token);
            DishMapParameter parameter = new DishMapParameter();
            parameter.ErpStoreID = ErpStoreId;
            parameter.Token = Token;
            foreach( var dish in dishes )
            {
                parameter.DishMapItems.Add(new DishMapParameter.DishMapItem() {
                    DishId = dish.DishId,
                    ErpDishId = (index++).ToString(),
                });
            }

            resturant.DishMap(parameter);
        }

        [TestMethod]
        public void GetStoreInfo()
        {
            var resturant = ResturantFactory.CreateResturant(ResturantPlatformType.Ele);
            var info = resturant.GetStoreInfo(ErpStoreId, Token);
        }
        [TestMethod]
        public void SetStoreStatus()
        {
            var resturant = ResturantFactory.CreateResturant(ResturantPlatformType.Ele);
            resturant.SetStoreStatus(ErpStoreId, Token , StoreStatus.Opened);
        }
        [TestMethod]
        public void SetStoreOpenTimes()
        {
            var resturant = ResturantFactory.CreateResturant(ResturantPlatformType.Ele);
            List<DayOpenTime> dayOpenTimes = new List<DayOpenTime>();
            dayOpenTimes.Add(new DayOpenTime() {
                Times = new List<string>(new string[] { "08:00-12:00","13:00-20:00"})
            });
            resturant.SetStoreOpenTimes(ErpStoreId, Token, dayOpenTimes);
        }
        public void OnOrderRefund(ResturantPlatformType platformType, OrderRefundInfo info)
        {

        }

        public void OnCancelOrderRefund(ResturantPlatformType platformType, OrderRefundInfo info)
        {

        }

        public void OnOrderFinish(ResturantPlatformType platformType, string orderId)
        {

        }

        public string OnGetPlatformToken(ResturantPlatformType platformType, string orderId)
        {
            return Token;
        }

        public void OnReceiveOrderSettlement(ResturantPlatformType platformType, string orderId, ServiceAmountInfo settleAmount)
        {

        }

        public void OnOrderRefundCompleted(ResturantPlatformType platformType, OrderRefundInfo info)
        {

        }


        [TestMethod]
        public void Push_handleNewOrder()
        {
            //handleNewOrder
            Type type = typeof(Jack.Resturant.CancelOrderParameter).Assembly.GetType("Jack.Resturant.Impls.Ele.Callbacks.Push");

            var jsonStr = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\ele_push_10.json");
            var method = type.GetMethod("handleNewOrder", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            method.Invoke(null, new object[] { Newtonsoft.Json.JsonConvert.DeserializeObject(jsonStr) });
        }

       [TestMethod]
       public void Push_handleOrderFinish()
        {
            Type type = typeof(Jack.Resturant.CancelOrderParameter).Assembly.GetType("Jack.Resturant.Impls.Ele.Callbacks.Push");

            var jsonStr = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\ele_push_18.json");
            var method = type.GetMethod("handleOrderFinish", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            method.Invoke(null, new object[] { Newtonsoft.Json.JsonConvert.DeserializeObject(jsonStr) });
        }

        public void OnReleaseStoreMapSuccess(ResturantPlatformType platformType, int erpStoreId)
        {

        }
    }
}
