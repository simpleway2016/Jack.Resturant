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

namespace Jack.Resturant.Impls.Meituan.Callbacks
{
    /// <summary>
    /// 云端心跳
    /// </summary>
    [CallbackDesc("云端心跳回调")]
    class Callback:IRequestHandler
    {
        public const string NotifyPageName = "/Jack_Resturant_Callback_Heart";

        public string UrlPageName => NotifyPageName;


        public TaskStatus Handle(IHttpProxy httpHandler)
        {
            httpHandler.ResponseContentType = "application/json";
            httpHandler.ResponseWrite("{\"data\":\"success\"}");
            return TaskStatus.Completed;
        }
    }

}
