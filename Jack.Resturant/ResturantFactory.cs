
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Way.Lib;
using System.Linq;
using System.Threading.Tasks;
#if NET46
#else
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
#endif


namespace Jack.Resturant
{
    public enum ResturantPlatformType
    {
        /// <summary>
        /// 美团
        /// </summary>
        Meituan = 1,
        /// <summary>
        /// 饿了吗
        /// </summary>
        Ele = 2,
        /// <summary>
        /// 百度外卖
        /// </summary>
        Baidu = 3,
    }

    public class ResturantFactory
    {
        static bool UseHttps; 
        static bool SettedRequestHandlers = false;
        internal static IResturantListener ResturantListener;


#if NET46
        internal static string CurrentDomainUrl
        {
            get
            {
                string url;
                var request = System.Web.HttpContext.Current.Request;
                var scheme = UseHttps ? "https" : "http";

                url = $"{scheme}://{ request.Headers["Host"]}";
                return url;
            }
        }
        /// <summary>
        /// 激活接口功能，此方法需要在Global的静态【析构】函数里面调用
        /// </summary>
        /// <param name="listener"></param>
        /// <param name="useHttps">运行的站点是否采用https</param>
        public static void Enable(IResturantListener listener,bool useHttps)
        {
            ResturantListener = listener;
            UseHttps = useHttps;
            if (!SettedRequestHandlers)
            {
                SettedRequestHandlers = true;

                try
                {
                    Jack.HttpRequestHandlers.Manager.AddRequestHandlers(typeof(ResturantFactory).Assembly);
                }
                catch { }
            }
        }
       
#else
        static IServiceProvider _ServiceProvider;
        internal static string CurrentDomainUrl
        {
            get
            {
                if (_ServiceProvider == null)
                    return null;
                string url;
                var accessor = (Microsoft.AspNetCore.Http.IHttpContextAccessor)_ServiceProvider.GetService(typeof(Microsoft.AspNetCore.Http.IHttpContextAccessor));
                var scheme = UseHttps ? "https" : "http";

                //Headers["Host"]会包含端口号
                url = $"{scheme}://{accessor.HttpContext.Request.Headers["Host"]}";
                return url;
            }
        }
        /// <summary>
        /// 激活接口功能，此方法需要在startup.cs的Configure里面调用
        /// </summary>
        /// <param name="listener"></param>
        /// <param name="mvcApp"></param>
        /// <param name="useHttps">运行的站点是否采用https</param>
        public static void Enable(IResturantListener listener , IApplicationBuilder mvcApp,bool useHttps)
        {
            if (mvcApp != null)
            {
                _ServiceProvider = mvcApp.ApplicationServices;
            }
            UseHttps = useHttps;
            ResturantListener = listener;
            if (!SettedRequestHandlers)
            {
                SettedRequestHandlers = true;
                if (mvcApp != null)
                {
                    try
                    {
                        Jack.HttpRequestHandlers.Manager.AddRequestHandlers(mvcApp, typeof(ResturantFactory).Assembly);
                    }
                    catch (Exception ex)
                    {
                        using (Log logErr = new Log("ResturantFactory.Enable error "))
                        {
                            logErr.Log(ex.ToString());
                        }
                    }
                }
            }
        }
#endif

        static Dictionary<ResturantPlatformType, Type> _ResturantPlatformDefineds;
        /// <summary>
        /// 记录每个平台对应的枚举类型
        /// </summary>
        static Dictionary<ResturantPlatformType, Type> ResturantPlatformDefineds
        {
            get
            {
                if (_ResturantPlatformDefineds == null)
                {
                    _ResturantPlatformDefineds = new Dictionary<ResturantPlatformType, Type>();
                    //循环当前程序集的所有Type，取出所有接口类型
                    Type iresturantType = typeof(IResturant);
                    Type[] types = iresturantType.GetTypeInfo().Assembly.GetTypes();
                    foreach (Type type in types)
                    {
                        //判断这个类是否实现了IResturant接口
                        if (type.GetTypeInfo().GetInterface(iresturantType.FullName) != null)
                        {
                            ResturantTypeAttribute myAtt = type.GetTypeInfo().GetCustomAttribute<ResturantTypeAttribute>();
                            if (myAtt != null)
                            {
                                _ResturantPlatformDefineds.Add(myAtt.PlatformType, type);
                            }
                        }
                    }
                }
                return _ResturantPlatformDefineds;
            }
        }

        public static IResturant CreateResturant(ResturantPlatformType platformType)
        {
            var type = ResturantPlatformDefineds[platformType];
            return Activator.CreateInstance(type) as IResturant;
        }
    }
}
