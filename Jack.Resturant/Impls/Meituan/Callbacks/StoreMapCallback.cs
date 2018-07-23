using System;
using System.Collections.Generic;
using System.Text;

using System.Xml.Linq;
using System.Xml.XPath;
using System.Security.Cryptography;
using Way.Lib;

#if NET46
#else
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Internal;
#endif
using Jack.HttpRequestHandlers;

namespace Jack.Resturant.Impls.Meituan.Callbacks
{
    /// <summary>
    /// 门店映射
    /// </summary>
    [CallbackDesc("门店映射回调")]
    class StoreMapCallback:IRequestHandler
    {
        public const string NotifyPageName = "/Jack_Resturant_Callback_StoreMap";

        public string UrlPageName => NotifyPageName;

        public TaskStatus Handle(IHttpProxy httpHandler)
        {
            httpHandler.ResponseContentType = "application/json";
            var forms = httpHandler.Form.ToObject<Dictionary<string, string>>();
            try
            {
                if (forms.Count > 0)
                {
                    var ePoiId = forms["ePoiId"];
                    var appAuthToken = forms["appAuthToken"];
                    var businessId = forms["businessId"];
                    if (businessId != "2")//不是外卖
                        return TaskStatus.ContinueOtherTask;

                    if (ResturantFactory.ResturantListener != null && !string.IsNullOrEmpty(appAuthToken))
                    {
                        ResturantFactory.ResturantListener.OnStoreMapSuccess(ResturantPlatformType.Meituan, new StoreMapInfo()
                        {
                            ErpStoreId = int.Parse(ePoiId),
                            Token = appAuthToken
                        });
                    }
                }
                else
                {
                    return TaskStatus.ContinueOtherTask;
                }
            }
            catch (Exception ex)
            {
                using (Log log = new Log("美团StoreMapCallback解析错误"))
                {
                    log.Log(ex.ToString());
                }
                throw ex;
            }
            httpHandler.ResponseWrite( "{\"data\":\"success\"}");

            return TaskStatus.Completed;
        }
    }
    
}
