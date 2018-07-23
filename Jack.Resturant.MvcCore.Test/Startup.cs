using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jack.Resturant.MvcCore.Test
{
    public class Startup : IResturantListener
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

                ResturantFactory.Enable(this, app , true);

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
        
        public static Dictionary<int, string> StoreInfos = new Dictionary<int, string> {
            { 1 , "e209d915485dd797b0f572988de3a3550c883d84966a98dde2875aa7d46663b3" },
            { 2 , "19cb57ffd661a0a43f11919c50c2b382254b22aa7445b6cc21768226edd7915e" }
        };

        public string OnGetPlatformConfigXml(ResturantPlatformType platformType)
        {
            if (platformType == ResturantPlatformType.Meituan)
                return @"<?xml version=""1.0"" encoding=""utf-8""?>           
<r>
    <developerId>101572</developerId>
    <SignKey>q25w5ibpttho0xmb</SignKey>
</r>";
            else if(platformType == ResturantPlatformType.Ele)
            {
                return @"<?xml version=""1.0"" encoding=""utf-8""?>           
<r>
    <Key>luHfyTsQLO</Key>
    <Secret>000358ab2127e08fbcbb8e34aa0252563a9c01dc</Secret>
</r>";
            }
            else if(platformType == ResturantPlatformType.Baidu)
            {
                return @"<?xml version=""1.0"" encoding=""utf-8""?>           
<r>
    <Source>32517</Source>
    <Secret>e9f6c48545ee588c</Secret>
<StoreMapSource>D8E7CE6CF9ECD7D5F396B1E0BB366A5A9614A7F101CAA7AFFD666A2282894D07</StoreMapSource>
</r>";
            }

            return null; 
        }

        public void OnStoreMapSuccess(ResturantPlatformType platformType, StoreMapInfo mapinfo)
        {
            //门店映射成功，接收token
            using (Way.Lib.CLog log = new Way.Lib.CLog("成功映射门店"))
            {
                log.Log($"erpStoreId:{mapinfo.ErpStoreId} token:{mapinfo.Token} refresh_token:{mapinfo.Refresh_token} expires:{mapinfo.Expires}");
            }
        }
        public void OnCompanyMapSuccess(ResturantPlatformType platformType,int companyId, StoreMapInfo[] mapinfo)
        {
          
        }
        public string OnGetToken(ResturantPlatformType platformType, int erpStoreId)
        {
            if(platformType == ResturantPlatformType.Ele)
                return "e4d1ffda63f5a1023224f8f6e23af571";
            else
                return null;
        }
        public void OnOrderCancel(ResturantPlatformType platformType, OrderCancelInfo orderInfo)
        {
            using (Way.Lib.CLog log = new Way.Lib.CLog("取消订单"))
            {
                log.LogJson(orderInfo);
            }
        }
        public void OnReceiveNewOrder(ResturantPlatformType platformType, OrderInfo orderInfo)
        {
            using (Way.Lib.CLog log = new Way.Lib.CLog("新订单"))
            {
                log.LogJson(orderInfo);
            }

            var resturant = ResturantFactory.CreateResturant(platformType);

            //取消订单
            //resturant.CancelOrder(new CancelOrderParameter() {
            //    OrderID = orderInfo.ThirdOrderId,
            //    Reason = CancelOrderReason.Busy,
            //    Token = StoreInfos[orderInfo.ErpStoreID.Value]
            //});
          

            //确认订单

            ConfirmOrderParameter parameter = new ConfirmOrderParameter();
            parameter.OrderID = orderInfo.ThirdOrderId;
            parameter.Token = "f8b8211858f6307d24b97e67aa3915e3e5aa78d0dbc7765618fb8485901db153";
            resturant.ConfirmOrder(parameter);

            return;

            //发起配送
            resturant.Delivering(new DeliverParameter() {
                CourierName = "刘培松",
                CourierPhone = "13261952754",
                OrderID = orderInfo.ThirdOrderId,
                Token = StoreInfos[orderInfo.ErpStoreID.Value],
            });

            resturant.Delivered(new DeliveredParameter()
            {
                OrderID = orderInfo.ThirdOrderId,
                Token = StoreInfos[orderInfo.ErpStoreID.Value],
            });
        }

        public List<Shop> OnGetShopList(ResturantPlatformType platformType, int companyId)
        {
            var result = new List<Shop>();
            result.Add(new Shop() {
                id = 1,
                Name = "我的测试门店"
            });
            return result;
        }

        public void OnOrderRefund(ResturantPlatformType platformType, OrderRefundInfo info)
        {
            using (Way.Lib.CLog log = new Way.Lib.CLog("用户申请取消订单"))
            {
                log.LogJson(info);
            }
            
        }

        public void OnOrderFinish(ResturantPlatformType platformType, string orderId)
        {
            using (Way.Lib.CLog log = new Way.Lib.CLog("订单已完成"))
            {
                log.Log(orderId);
            }
        }

        public void OnCancelOrderRefund(ResturantPlatformType platformType, OrderRefundInfo info)
        {
            using (Way.Lib.CLog log = new Way.Lib.CLog("用户取消退单"))
            {
                log.LogJson(info);
            }
        }

        public void OnReceiveOrderSettlement(ResturantPlatformType platformType, string orderId, double settleAmount)
        {
            using (Way.Lib.CLog log = new Way.Lib.CLog("收到订单结算信息"))
            {
                log.Log("orderId:{0}", orderId);
                log.Log("settleAmount:{0}", settleAmount);
            }
        }

        public string OnGetPlatformToken(ResturantPlatformType platformType, string orderId)
        {
            if(platformType == ResturantPlatformType.Ele)
            {
                return "";
            }
            return null;
        }

        public void OnOrderRefundCompleted(ResturantPlatformType platformType, OrderRefundInfo info)
        {
           
        }

        public void OnReceiveOrderSettlement(ResturantPlatformType platformType, string orderId, ServiceAmountInfo serviceAmountInfo)
        {
           
        }

        public void OnReleaseStoreMapSuccess(ResturantPlatformType platformType, int erpStoreId)
        {
            
        }
    }
}
