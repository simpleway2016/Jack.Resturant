using System;
using System.Collections.Generic;
using System.Text;

using System.Xml.Linq;
using System.Xml.XPath;
using System.Security.Cryptography;

#if NET46
#else
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Internal;
#endif
using Jack.HttpRequestHandlers;

namespace Jack.Resturant.Impls.Ele.Callbacks
{
    /// <summary>
    /// 回调Url
    /// </summary>
    [CallbackDesc("回调Url")]
    class Callback:IRequestHandler
    {
        public const string NotifyPageName = "/Jack_Resturant_ELE_Callback";

        public string UrlPageName => NotifyPageName;


        public TaskStatus Handle(IHttpProxy httpHandler)
        {
            var querys = httpHandler.QueryString.ToObject<Dictionary<string, string>>();
            var forms = httpHandler.Form.ToObject<Dictionary<string, string>>();

            using (Log log = new Log("饿了么.Callback", false))
            {
                log.Log("querys:");
                log.LogJson(querys);
                log.Log("forms:");
                log.LogJson(forms);
            }

            if (forms.ContainsKey("pairValue"))
            {
                //手动关联后，提交上来的关联信息
                EleResturant resturan = new EleResturant();
                var idinfo = querys["state"].Split('_');
                int companyid = int.Parse(idinfo[0]);

                string token = forms["token"];
                string refresh_token = forms["refresh_token"];
                DateTime expires_in = Convert.ToDateTime(forms["expires_in"]);
                var pairs = forms["pairValue"].Split(',');//配对字符串，格式为：erpShopId_eleShopId,erpShopId_eleShopId,
                List<StoreMapInfo> mapinfos = new List<StoreMapInfo>();
                foreach (var str in pairs)
                {
                    if (str.Length == 0)
                        continue;
                    idinfo = str.Split('_');
                    int erpStoreId = int.Parse(idinfo[0]);
                    int eleShopId = int.Parse(idinfo[1]);
                    resturan.BindShop(token, erpStoreId, eleShopId);

                    mapinfos.Add(new StoreMapInfo()
                    {
                        ErpStoreId = erpStoreId,
                        Token = token,
                        Refresh_token = refresh_token,
                        Expires = expires_in
                    });
                }
                ResturantFactory.ResturantListener.OnCompanyMapSuccess(ResturantPlatformType.Ele, companyid, mapinfos.ToArray());
                //输出页面
                var stream = typeof(Callback).Assembly.GetManifestResourceStream("Jack.Resturant.Impls.Ele.StoreMapSuccess.html");
                byte[] bs = new byte[stream.Length];
                stream.Read(bs, 0, bs.Length);
                stream.Dispose();

                httpHandler.ResponseWrite( System.Text.Encoding.UTF8.GetString(bs));
            }
            else if (querys.ContainsKey("error"))//error=unauthorized_client
            {
                if (querys["error"] == "unauthorized_client")
                {
                    try
                    {
                        //确定饿了么解除绑定
                        int erpShopId = Convert.ToInt32(querys["erpShopId"]);
                        string token = querys["token"];
                        EleResturant resturan = new EleResturant();
                        try
                        {
                            //这时候，有可能token已经失效了
                            var shopid = resturan.getShopIdByErpStoreId(token, erpShopId);
                            resturan.UnBindShop(token, erpShopId, shopid);
                        }
                        catch (Exception ex)
                        {
                            using (Log log = new Log("Ele 处理解绑时的error"))
                            {
                                log.Log("此错误不影响代码运行，只是记录");
                                log.Log(ex.ToString());
                            }
                        }
                        ResturantFactory.ResturantListener.OnReleaseStoreMapSuccess(ResturantPlatformType.Ele, erpShopId);

                        //输出页面
                        var stream = typeof(Callback).Assembly.GetManifestResourceStream("Jack.Resturant.Impls.Ele.ReleaseStoreMap.html");
                        byte[] bs = new byte[stream.Length];
                        stream.Read(bs, 0, bs.Length);
                        stream.Dispose();
                        httpHandler.ResponseWrite(System.Text.Encoding.UTF8.GetString(bs));
                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }
                }
                httpHandler.ResponseWrite("OK");
            }
            else if (querys.ContainsKey("code"))
            {
                //把state参数解析出companyid和erpStoreId
                var idinfo = querys["state"].Split('_');
                int companyid = int.Parse(idinfo[0]);
                int erpStoreId = int.Parse(idinfo[1]);

                //组织回调地址
                var url = ResturantFactory.CurrentDomainUrl;
                if (url.StartsWith("https") == false)
                {
                    url = $"https{url.Substring(4)}";
                }
                url += Callbacks.Callback.NotifyPageName;
                url = System.Net.WebUtility.UrlEncode(url);

                var config = new Config(ResturantFactory.ResturantListener.OnGetPlatformConfigXml(ResturantPlatformType.Ele));
                string code = querys["code"];

                //根据code获取token
                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers["Authorization"] = "Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{config.Key}:{config.Secret}"));
                string query = $"grant_type=authorization_code&code={code}&redirect_uri={url}&client_id={config.Key}";

                string result = Helper.PostQueryString($"{EleResturant.ServerUrl}/token", headers, query, 8000);
                var jsonObj = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(result);
                string access_token = jsonObj.Value<string>("access_token");
                int expires_in = jsonObj.Value<int>("expires_in");
                string refresh_token = jsonObj.Value<string>("refresh_token");

                //根据token读取店信息
                EleResturant resturan = new EleResturant();
                var ele_shops = resturan.getShops(access_token);
                var sys_shops = ResturantFactory.ResturantListener.OnGetShopList(ResturantPlatformType.Ele, companyid);

                if (ele_shops.Count == 1)
                {
                    resturan.BindShop(access_token, erpStoreId, ele_shops[0].id);
                    List<StoreMapInfo> mapinfos = new List<StoreMapInfo>();
                    mapinfos.Add(new StoreMapInfo()
                    {
                        ErpStoreId = erpStoreId,
                        Expires = DateTime.Now.AddSeconds(expires_in),
                        Refresh_token = refresh_token,
                        Token = access_token
                    });
                    ResturantFactory.ResturantListener.OnCompanyMapSuccess(ResturantPlatformType.Ele, companyid, mapinfos.ToArray());

                    //输出页面
                    var stream = typeof(Callback).Assembly.GetManifestResourceStream("Jack.Resturant.Impls.Ele.StoreMapSuccess.html");
                    byte[] bs = new byte[stream.Length];
                    stream.Read(bs, 0, bs.Length);
                    stream.Dispose();
                    httpHandler.ResponseWrite(System.Text.Encoding.UTF8.GetString(bs));
                }
                else
                {
                    //输出手动关联门店页面
                    var stream = typeof(Callback).Assembly.GetManifestResourceStream("Jack.Resturant.Impls.Ele.StoreMap.html");
                    byte[] bs = new byte[stream.Length];
                    stream.Read(bs, 0, bs.Length);
                    stream.Dispose();
                    httpHandler.ResponseWrite(System.Text.Encoding.UTF8.GetString(bs) + $"<script lang=\"ja\">document.querySelector('#token').value='{access_token}';document.querySelector('#refresh_token').value='{refresh_token}';document.querySelector('#expires_in').value='{DateTime.Now.AddSeconds(expires_in)}';sys_shops={Newtonsoft.Json.JsonConvert.SerializeObject(sys_shops)};ele_shops={Newtonsoft.Json.JsonConvert.SerializeObject(ele_shops)};</script>");
                }
            }
            else
            {
                httpHandler.ResponseWrite("OK");
            }

            return TaskStatus.Completed;
        }
    }
    
}
