using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Jack.Resturant.Impls.Meituan
{
    class Config
    {
        public string developerId;
        public string SignKey;
        public Config(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                throw new Exception("没有获取到正确的美团xml");
            }
            try
            {
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.LoadXml(xml);
                this.developerId = xmldoc.DocumentElement.SelectSingleNode("developerId").InnerText;
                this.SignKey = xmldoc.DocumentElement.SelectSingleNode("SignKey").InnerText;
            }
            catch
            {
                throw new Exception("没有获取到正确的美团xml");
            }
           
        }
    }
}
