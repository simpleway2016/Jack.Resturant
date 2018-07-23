using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Jack.Resturant.Impls.Ele
{
    class Config
    {
        public string Secret;
        public string Key;
        public Config(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                throw new Exception("没有获取到正确的饿了么xml");
            }
            try
            {
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.LoadXml(xml);
                this.Key = xmldoc.DocumentElement.SelectSingleNode("Key").InnerText;
                this.Secret = xmldoc.DocumentElement.SelectSingleNode("Secret").InnerText;
            }
            catch
            {
                throw new Exception("没有获取到正确的饿了么xml");
            }
            
        }
    }
}
