using System;
using System.Collections.Generic;
using System.Text;

using System.Xml.Linq;
using System.Xml.XPath;
using System.Security.Cryptography;
using Way.Lib;
using System.Collections.Concurrent;

#if NET46
#else
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Internal;
#endif
using Jack.HttpRequestHandlers;

namespace Jack.Resturant.Impls.Baidu.Callbacks
{
    class ResponsePicture : IRequestHandler
    {
        internal static ConcurrentDictionary<string, byte[]> Pictures = new ConcurrentDictionary<string, byte[]>();
        public const string NotifyPageName = "/Jack_Resturant_Baidu_ResponsePicture";

        public string UrlPageName => NotifyPageName;

        public static byte[] HandleForm(string url)
        {
            var str = $"/{ResponsePicture.NotifyPageName}/";
            var index =  url.IndexOf(str);
            url = url.Substring(index + str.Length);
            var id = url.Substring(0, url.IndexOf("/"));
            byte[] content;
            Pictures.TryRemove(id ,out content);
            return content;
        }

        public TaskStatus Handle(IHttpProxy httpHandler)
        {
           httpHandler.ResponseContentType = "image/png";
            var url = httpHandler.UrlAbsolutePath;

            var str = $"/{ResponsePicture.NotifyPageName}/";
            var index = url.IndexOf(str);
            url = url.Substring(index + str.Length);
            var id = url.Substring(0, url.IndexOf("/"));
            byte[] content;
            Pictures.TryRemove(id, out content);
            httpHandler.ResponseWrite( content , 0 , content.Length);

            return TaskStatus.Completed;
        }
    }
    
}
