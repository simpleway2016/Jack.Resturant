using System;
using System.Collections.Generic;
using System.Text;

namespace Jack.Resturant
{
    
    public class DishAttribute
    {
        public string Name;
        public List<string> Values = new List<string>();
    }
    public class DishInfo
    {
        /// <summary>
        /// 餐盒数量
        /// </summary>
        public int BoxNumber;
        /// <summary>
        /// 餐盒单价
        /// </summary>
        public double BoxPrice;
        /// <summary>
        /// 分类名称
        /// </summary>
        public string CategoryName;
        /// <summary>
        /// 描述
        /// </summary>
        public string Description;
        /// <summary>
        /// 菜品名称
        /// </summary>
        public string DishName;
        /// <summary>
        /// 菜品id，目前美团这个值为空
        /// </summary>
        public string DishId;
        /// <summary>
        /// erp中菜品Id，目前饿了么，这个值为空
        /// </summary>
        public string ErpDishId;
        /// <summary>
        /// 图片地址，此地址为调用IResturant.UploadDishPicture获取的外卖平台图片地址
        /// </summary>
        public string Picture;
        /// <summary>
        /// 单价
        /// </summary>
        public double Price;
        /// <summary>
        /// 排序，从0开始，越小越靠前
        /// </summary>
        public int Sequence;
        /// <summary>
        /// 单位
        /// </summary>
        public string Unit;
        /// <summary>
        /// 菜品规格
        /// </summary>
        public List<DishSkuInfo> Skus = new List<DishSkuInfo>();
        /// <summary>
        /// 菜品属性，如：{ name:"辣度" , values:["微辣","劲辣"] }
        /// </summary>
        public List<DishAttribute> Attributes = new List<DishAttribute>();
        /// <summary>
        /// 菜品可售时间，一共7个值（必须是1个或者7个值），表示星期一到星期日，如果只有1个值，表示每天时间段是同样的
        /// </summary>
        public List<DayOpenTime> AvailableTimes = new List<DayOpenTime>();
    }
    public class DishSkuInfo
    {
        internal object SkuId;
        /// <summary>
        /// Erp系统中SkuId，有些外卖接口此字段可能为空
        /// </summary>
        internal string ErpSkuId;
        /// <summary>
        /// 规格，如：500ml瓶装
        /// </summary>
        public string Spec;
        /// <summary>
        /// 库存数量，null表示无限库存
        /// </summary>
        public uint? Stock;
        /// <summary>
        /// 单价
        /// </summary>
        public double Price;
    }
}
