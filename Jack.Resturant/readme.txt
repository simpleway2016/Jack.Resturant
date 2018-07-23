
mvc core使用：
需要在startup.cs的Configure里面,执行ResturantFactory.Enable

public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
	//MyResturantListener是一个自定义类，实现了IResturantListener接口
	ResturantFactory.Enable(new MyResturantListener(), app);
}


public class MyResturantListener: IResturantListener
{
}

实例化外卖接口对象：
var meituan = ResturantFactory.CreateResturant(ResturantPlatformType.Meituan);

//首先需要在外卖平台的后台管理页面，配置回调页面，至于回调地址是什么？可以执行ShowCallbackUrlSetting函数查看回调地址
var list = meituan.ShowCallbackUrlSetting("http://tan.xododo.com");




//        OnGetPlatformConfigXml返回xml格式示例：

		public string OnGetPlatformConfigXml(ResturantPlatformType platformType)
        {
            if (platformType == ResturantPlatformType.Meituan)
                return @"<?xml version=""1.0"" encoding=""utf-8""?>           
<r>
    <developerId>1572</developerId>
    <SignKey>q25w5itho0xmb</SignKey>
</r>";
            else if(platformType == ResturantPlatformType.Ele)
            {
                return @"<?xml version=""1.0"" encoding=""utf-8""?>           
<r>
    <Key>lyTsQLO</Key>
    <Secret>000358ab2127e08fbcbbaa0252563a9c01dc</Secret>
</r>";
            }
            else if(platformType == ResturantPlatformType.Baidu)
            {
				//Source：合作方账号
				//StoreMapSource：门店绑定时的Source参数值
                return @"<?xml version=""1.0"" encoding=""utf-8""?>           
<r>
    <Source>317</Source>
    <Secret>e9f6c45ee588c</Secret>
<StoreMapSource>D8E7CE6CF9ECD7D5F396B1FD666A2282894D07</StoreMapSource>
</r>";
            }

            return null; 
        }