using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System;

namespace Jack.Resturant.UnitTest.Core
{
    [TestClass]
    public class MeituanTest : IResturantListener
    {
        string Token = "45d22ab52b83e87f8dc90868119e1d602a78933272bb536db06003fdf7e54246";
        int erpStoreId = 49;

        public MeituanTest()
        {
            TimeSpan t;
            
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
    <developerId>101636</developerId>
    <SignKey>lk7n5vc76uvq11pr</SignKey>
</r>";
            else        if (platformType == ResturantPlatformType.Meituan)
                return @"<?xml version=""1.0"" encoding=""utf-8""?>           
<r>
    <developerId>101572</developerId>
    <SignKey>q25w5ibpttho0xmb</SignKey>
</r>";
            else if (platformType == ResturantPlatformType.Ele)
            {
                return @"<?xml version=""1.0"" encoding=""utf-8""?>           
<r>
    <Key>luHfyTsQLO</Key>
    <Secret>000358ab2127e08fbcbb8e34aa0252563a9c01dc</Secret>
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
        public void GetDish()
        {
            var resturant = ResturantFactory.CreateResturant(ResturantPlatformType.Meituan);
            var dishes = resturant.GetDishList(erpStoreId , Token);
            var result = resturant.GetType().InvokeMember("GetDish", System.Reflection.BindingFlags.InvokeMethod, null, resturant, new object[] { erpStoreId , Token , dishes[0].ErpDishId});
        }

        [TestMethod]
        public void UploadDish()
        {
            var resturant = ResturantFactory.CreateResturant(ResturantPlatformType.Meituan);
            var dish = new DishInfo()
            {
                BoxNumber = 1,
                BoxPrice = 0.1,
                CategoryName = "冷菜",
                DishName = "测试菜品1",
                ErpDishId = "100",
                Picture = "B98DC1B75A27D699176CB5A6CFFC26D0",
                Price = 10,
                Sequence = 1,
                Unit = "份"
            };
            dish.Skus.Add(new DishSkuInfo() {
                Price = 10,
                Spec = "大份",
                Stock = 100
            } );
            dish.Skus.Add(new DishSkuInfo()
            {
                Price = 6,
                Spec = "小份",
                Stock = 88
            });
            dish.Attributes.Add(new DishAttribute() {
                Name = "辣度",
                Values = new List<string>( new string[] {"微辣","中辣","超辣" })
            });

            dish.AvailableTimes.Add(new DayOpenTime()
            {
                Times = new List<string>(new string[] { "08:00-12:00", "13:00-19:00" })
            });
            resturant.UploadDish(erpStoreId , Token , new List<DishInfo>(new DishInfo[] { dish }));
        }

        [TestMethod]
        public void QueryOrderServiceMoney()
        {
            var resturant = ResturantFactory.CreateResturant(ResturantPlatformType.Meituan);
          var QueryOrderServiceMoney =   resturant.GetType().InvokeMember("GetOrderServiceAmount", System.Reflection.BindingFlags.InvokeMethod, null , resturant, new object[] { Token, "8361402627"  });
          
        }

        [TestMethod]
        public void GetDishList()
        {
            var resturant = ResturantFactory.CreateResturant(ResturantPlatformType.Meituan);
           var result =  resturant.GetDishList(erpStoreId, Token);
            string text = Newtonsoft.Json.JsonConvert.SerializeObject(result);
            System.IO.File.WriteAllText("i:\\dish.txt", text);
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

        public void OnReceiveOrderSettlement(ResturantPlatformType platformType, string orderId, ServiceAmountInfo settleAmount)
        {
            
        }

        public string OnGetPlatformToken(ResturantPlatformType platformType, string orderId)
        {
            return Token;
        }

        public void OnOrderRefundCompleted(ResturantPlatformType platformType, OrderRefundInfo info)
        {
            
        }

        /// <summary>
        /// 重新推送订单数据
        /// </summary>
        [TestMethod]
        public void PushCreateOrderCallback()
        {
            var jsonStr = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\meituan_newOrder.json");
            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonStr);

            var ms = typeof(Jack.Resturant.IResturant).Assembly.GetType("Jack.Resturant.Helper").GetMethods().Where(m => m.Name == "PostQueryString" && m.GetParameters().Length == 3);
            foreach (var method in ms)
            {
                try
                {
                    method.Invoke(null, new object[] { "http://192.168.3.199:168/Jack_Resturant_Callback_CreateOrder", obj, 8000 });
                }
                catch (System.ArgumentException ex)
                {

                }
            }
        }


        [TestMethod]
        public void CreateOrderCallback()
        {
            Type type = typeof(Jack.Resturant.CancelOrderParameter).Assembly.GetType("Jack.Resturant.Impls.Meituan.Callbacks.CreateOrderCallback");
            var method = type.GetMethod("handleContent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            var jsonStr = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\meituan_newOrder.json");          
            var obj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(jsonStr);
            var orderStr = obj.Value<string>("order");
            method.Invoke(null, new object[] { obj.Value<string>("order"), erpStoreId });

            jsonStr = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\meituan_newOrder2.json");
            obj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(jsonStr);
            method.Invoke(null, new object[] { obj.Value<string>("order"), erpStoreId });
        }

        public void OnReleaseStoreMapSuccess(ResturantPlatformType platformType, int erpStoreId)
        {
           
        }
    }
}
