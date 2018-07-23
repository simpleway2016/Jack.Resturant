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
    [CallbackDesc("门店映射解绑回调")]
    class ReleaseStoreMapCallback:IRequestHandler
    {
        public const string NotifyPageName = "/Jack_Resturant_Callback_ReleaseStoreMap";

        public string UrlPageName => NotifyPageName;


        public TaskStatus Handle(IHttpProxy httpHandler)
        {
            httpHandler.ResponseContentType = "application/json";
            var forms = httpHandler.Form.ToObject<Dictionary<string, string>>();
            try
            {
                if (forms.Count > 0)
                {

                    var businessId = forms["businessId"];
                    if (businessId != "2")//不是外卖
                        return TaskStatus.ContinueOtherTask;

                    var ePoiId = Convert.ToInt32(forms["ePoiId"]);

                    if (ResturantFactory.ResturantListener != null)
                    {
                        ResturantFactory.ResturantListener.OnReleaseStoreMapSuccess(ResturantPlatformType.Meituan, ePoiId);
                    }
                }
                else
                {
                    return TaskStatus.ContinueOtherTask;
                }
            }
            catch (Exception ex)
            {
                using (Log log = new Log("美团ReleaseStoreMapCallback解析错误"))
                {
                    log.Log(ex.ToString());
                }
            }
            httpHandler.ResponseWrite( "{\"data\":\"success\"}");

            return TaskStatus.Completed;
        }
    }
    
}
