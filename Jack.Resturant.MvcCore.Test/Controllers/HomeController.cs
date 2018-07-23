using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Jack.Resturant.MvcCore.Test.Models;
using System.Net.Security;

namespace Jack.Resturant.MvcCore.Test.Controllers
{
    public class HomeController : Controller
    {
        ResturantPlatformType platform = ResturantPlatformType.Ele;

        public IActionResult Index()
        {
            return View();
        }
        public string Url()
        {
            string p = Request.Headers["X-Forwarded-Proto"];
            foreach(var item in Request.Headers)
            {
               if(item.Key.StartsWith("X-Forwarded-") && item.Value.Contains("http"))
                {
                    break;
                }
            }
            return Request.Host.ToString();
        }
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public string Test()
        {
            return Request.Headers["Host"];
        }
        public IActionResult StoreMap()
        {
            var meituan = ResturantFactory.CreateResturant(platform );
            var url = meituan.StoreMap(46,46, "测试门店1");
            return this.Redirect(url);
        }
      
      

        public string DeleteDish()
        {
            return "ok";
        }
        public string UploadDish()
        {
            var meituan = ResturantFactory.CreateResturant(platform);
            var dish = new DishInfo() {
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
            //dish.SubDishes.Add(new SubDishInfo()
            //{
            //    ErpSubDishId = "Sub100",
            //    Price = 10,
            //    Spec = "小份",
            //    Stock = 1
            //});
            meituan.UploadDish(1, Startup.StoreInfos[1], new List<DishInfo>(new DishInfo[] { dish}));
            return "ok";
        }

        public string UploadPic()
        {
            //B98DC1B75A27D699176CB5A6CFFC26D0  对应  i:\\a.jpg
            var content = System.IO.File.ReadAllBytes("i:\\a.jpg");
            var meituan = ResturantFactory.CreateResturant( ResturantPlatformType.Baidu);
            return meituan.UploadDishPicture(1, Startup.StoreInfos[1], "test.jpg", content);
        }
        public string CreateCat()
        {
            string name = "test";
            var meituan = ResturantFactory.CreateResturant(platform);
            meituan.CreateDishCategory(new CreateDishCategoryParameter() {
                CategoryName = name,
                ErpStoreId = 1,
                Sequence = 1,
                Token = Startup.StoreInfos[1]
            });
            meituan.UpdateDishCategory(new UpdateDishCategoryParameter()
            {
                CategoryName = name + "2",
                OriginalCategoryName = name,
                ErpStoreId = 1,
                Sequence = 1,
                Token = Startup.StoreInfos[1]
            });
            meituan.DeleteDishCategory(1, Startup.StoreInfos[1], name + "2");
            return "ok";
        }
        public List<DishCategory> GetCats()
        {
            var meituan = ResturantFactory.CreateResturant(platform);
            var list = meituan.GetDishCategoryList(1, Startup.StoreInfos[1]);
            Response.ContentType = "application/json";
            return list;
        }
        public List<DishInfo> GetDishes()
        {
            var meituan = ResturantFactory.CreateResturant(platform);
            var list = meituan.GetDishList(1 , Startup.StoreInfos[1]);
            Response.ContentType = "application/json";
            return list;
        }
        public object GetStoreInfo()
        {
            var meituan = ResturantFactory.CreateResturant(platform);
            var info = meituan.GetStoreInfo(1, Startup.StoreInfos[1]);
            Response.ContentType = "application/json";
            return info;
        }
        public object SetStatus(StoreStatus status)
        {
            var meituan = ResturantFactory.CreateResturant(platform);
            meituan.SetStoreStatus(1, Startup.StoreInfos[1] , status);
            Response.ContentType = "application/json";
            return "ok";
        }
        public object SetOpentime()
        {
            //SslStream stream = new SslStream();
            var meituan = ResturantFactory.CreateResturant(platform);
            var opentimes = new List<DayOpenTime>();
            for(int i = 0; i < 7; i ++)
            {
                if (i != 5)
                {
                    opentimes.Add(new DayOpenTime()
                    {
                        Times = new List<string>(new string[] { "09:00-12:00", "14:00-19:00" })
                    });
                }
                else
                {
                    opentimes.Add(new DayOpenTime()
                    {
                        Times = null
                    });
                }
            }
            meituan.SetStoreOpenTimes(1, Startup.StoreInfos[1], opentimes);
            Response.ContentType = "application/json";
            return "ok";
        }
        public void MapAllDishes()
        {
            var meituan = ResturantFactory.CreateResturant(platform);
            var dishes = GetDishes();

            DishMapParameter parameter = new DishMapParameter();
            parameter.Token = Startup.StoreInfos[1];
            parameter.ErpStoreID = 1;
            int dishindex = 1;
            int subdishIndex = 1;

            foreach ( var dish in dishes )
            {
                var map = new DishMapParameter.DishMapItem()
                {
                    DishId = dish.DishId,
                    ErpDishId = (dishindex ++).ToString()
                };
                parameter.DishMapItems.Add(map);
            }

          
            meituan.DishMap(parameter);
        }

        public object ShowCallbackUrlSetting()
        {
            var meituan = ResturantFactory.CreateResturant(platform);
            var list = meituan.ShowCallbackUrlSetting("http://test.xiaohengjiaozi.com");
            Response.ContentType = "application/json";
            return list;
        }
    }
}
