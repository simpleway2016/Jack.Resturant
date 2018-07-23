using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Jack.Resturant.Impls.Baidu
{
    class Config
    {
        public string Source;
        public string Secret;
        public string StoreMapSource;
        public Config(string xml)
        {
            if(string.IsNullOrEmpty(xml))
            {
                throw new Exception("没有获取到正确的百度外卖xml");
            }
            try
            {
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.LoadXml(xml);
                this.Source = xmldoc.DocumentElement.SelectSingleNode("Source").InnerText;
                this.Secret = xmldoc.DocumentElement.SelectSingleNode("Secret").InnerText;
                this.StoreMapSource = xmldoc.DocumentElement.SelectSingleNode("StoreMapSource").InnerText;
            }
            catch
            {
                throw new Exception("没有获取到正确的百度外卖xml");
            }
        }
    }
}
