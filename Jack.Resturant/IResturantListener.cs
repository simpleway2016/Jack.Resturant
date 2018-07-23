using System;
using System.Collections.Generic;
using System.Text;

namespace Jack.Resturant
{
    public interface IResturantListener
    {
        /// <summary>
        /// 获取平台xml配置字符串
        /// </summary>
        /// <param name="platformType"></param>
        /// <returns></returns>
        string OnGetPlatformConfigXml(ResturantPlatformType platformType);
        /// <summary>
        /// 获取平台店铺的token
        /// </summary>
        /// <param name="platformType"></param>
        /// <param name="orderId">订单Id</param>
        /// <returns></returns>
        string OnGetPlatformToken(ResturantPlatformType platformType, string orderId);
        /// <summary>
        /// 门店映射成功
        /// </summary>
        /// <param name="platformType"></param>
        /// <param name="mapinfo"></param>
        void OnStoreMapSuccess(ResturantPlatformType platformType, StoreMapInfo mapinfo);

        /// <summary>
        /// 解除门店映射成功
        /// </summary>
        /// <param name="platformType"></param>
        /// <param name="erpStoreId">Erp门店id</param>
        void OnReleaseStoreMapSuccess(ResturantPlatformType platformType, int erpStoreId);

        /// <summary>
        /// 集团门店映射成功，像“饿了么”会调这个方法，而不是调OnStoreMapSuccess
        /// </summary>
        /// <param name="platformType"></param>
        /// <param name="companyId"></param>
        /// <param name="mapinfos"></param>
        void OnCompanyMapSuccess(ResturantPlatformType platformType, int companyId, StoreMapInfo[] mapinfos);

        /// <summary>
        /// 获取erp系统里的店铺列表
        /// </summary>
        /// <param name="platformType"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        List<Shop> OnGetShopList(ResturantPlatformType platformType, int companyId);

        /// <summary>
        /// 接收到新订单
        /// </summary>
        /// <param name="platformType"></param>
        /// <param name="orderInfo"></param>
        void OnReceiveNewOrder(ResturantPlatformType platformType, OrderInfo orderInfo);
        /// <summary>
        /// 订单已经被取消，主动调用AgreeOrderRefund取消的订单，或者主动调用CancelOrder取消的订单，一般不会回调此方法
        /// </summary>
        /// <param name="platformType"></param>
        /// <param name="orderInfo"></param>
        void OnOrderCancel(ResturantPlatformType platformType, OrderCancelInfo orderInfo);
        /// <summary>
        /// 退单通知（退款通知），需要在里面写代码调用相关方法，同意、不同意退款
        /// </summary>
        /// <param name="platformType"></param>
        /// <param name="info"></param>
        void OnOrderRefund(ResturantPlatformType platformType, OrderRefundInfo info);
        /// <summary>
        /// 用户取消退款申请
        /// </summary>
        /// <param name="platformType"></param>
        /// <param name="info"></param>
        void OnCancelOrderRefund(ResturantPlatformType platformType, OrderRefundInfo info);
        /// <summary>
        /// 订单退单成功
        /// </summary>
        /// <param name="platformType"></param>
        /// <param name="info">退单信息</param>
        void OnOrderRefundCompleted(ResturantPlatformType platformType, OrderRefundInfo info);
        /// <summary>
        /// 订单已完成通知
        /// </summary>
        /// <param name="platformType"></param>
        /// <param name="orderId"></param>
        void OnOrderFinish(ResturantPlatformType platformType, string orderId);
        /// <summary>
        /// 订单收到结算信息
        /// 同一个订单可能会多次接收结算信息，以最后一次的为准
        /// </summary>
        /// <param name="platformType"></param>
        /// <param name="orderId"></param>
        /// <param name="serviceAmountInfo">费用信息</param>
        void OnReceiveOrderSettlement(ResturantPlatformType platformType, string orderId,ServiceAmountInfo serviceAmountInfo);
    }

    public enum DeliveryTimeTag
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0,
        /// <summary>
        /// 按时
        /// </summary>
        OnTime = 10,
        /// <summary>
        /// 尽快
        /// </summary>
        ASAP = 20,
        /// <summary>
        /// 预订
        /// </summary>
        InAdvance = 30,
    }
    public enum PayType
    {
        /// <summary>
        /// 货到付款
        /// </summary>
        PayOnDelivery = 1,
        /// <summary>
        /// 在线支付
        /// </summary>
        PayOnline = 2,
    }
    public class OrderCancelInfo
    {
        public string OrderID;
        /// <summary>
        /// Erp系统门店id，可能是空
        /// </summary>
        public int? ErpStoreID;
        /// <summary>
        /// 取消原因
        /// </summary>
        public string Reason;
    }
    /// <summary>
    /// 退款菜品信息
    /// </summary>
    public class RefundDishInfo
    {
        /// <summary>
        /// erp中菜品id，可能为null
        /// </summary>
        public string ErpDishId;
        /// <summary>
        /// 菜品名称
        /// </summary>
        public string DishName;
        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity;
        /// <summary>
        /// 菜品原单价
        /// </summary>
        public double Price;
        /// <summary>
        /// 退款单价
        /// </summary>
        public double RefundPrice;
    }
    public class OrderRefundInfo
    {
        public string OrderID;
        /// <summary>
        /// Erp系统门店id，可能是空
        /// </summary>
        public int? ErpStoreID;
        /// <summary>
        /// 退单原因
        /// </summary>
        public string Reason;
        /// <summary>
        /// 退款金额，如果是null，可以肯定是整单退
        /// </summary>
        public double? Money;
        /// <summary>
        /// 退款菜品列表
        /// </summary>
        public List<RefundDishInfo> RefundDishInfos = new List<RefundDishInfo>();
    }
    /// <summary>
    /// 订单信息
    /// </summary>
    public class OrderInfo
    {
        public class Detail
        {
            /// <summary>
            /// Erp系统中的菜品SkuID
            /// </summary>
            public string ErpSkuId;

            /// <summary>
            /// 餐盒数量
            /// </summary>
            public int BoxNumber;
            /// <summary>
            /// 餐盒单价
            /// </summary>
            public double BoxPrice;
            /// <summary>
            /// 菜品名称
            /// </summary>
            public string FoodName;
            /// <summary>
            /// 单价
            /// </summary>
            public double Price;
            /// <summary>
            /// 数量
            /// </summary>
            public int Quantity;
            /// <summary>
            /// 单位
            /// </summary>
            public string Unit;
            /// <summary>
            /// 菜品折扣
            /// </summary>
            public double FoodDiscount;
            /// <summary>
            /// 菜品属性
            /// </summary>
            public string FoodProperty = "";
            /// <summary>
            /// 菜品规格
            /// </summary>
            public string FoodSpec = "";

        }
        /// <summary>
        /// 优惠活动信息
        /// </summary>
        public class DiscountInfo
        {
            /// <summary>
            /// 该活动中平台承担的费用
            /// </summary>
            public double PlatformCharge;
            /// <summary>
            /// 该活动中商家承担的费用
            /// </summary>
            public double PoiCharge;
            /// <summary>
            /// 活动优惠金额
            /// </summary>
            public double ReduceFee;
            /// <summary>
            /// 优惠说明
            /// </summary>
            public string Remark;
        }

        public string ThirdOrderId;
        /// <summary>
        /// Erp系统门店id
        /// </summary>
        public int? ErpStoreID;
        /// <summary>
        /// 订单创建时间
        /// </summary>
        public DateTime? CreateTime;
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark;
        /// <summary>
        /// 门店当天的订单流水号，每天流水号从1开始
        /// </summary>
        public string DayOrderNumber;
        /// <summary>
        /// 用户预计送达时间，“立即送达”时为null
        /// </summary>
        public DateTime? DeliveryTime;
        public DeliveryTimeTag DeliveryTimeTag;
        /// <summary>
        /// 是否需要发票
        /// </summary>
        public bool HasInvoiced;
        /// <summary>
        /// 发票抬头
        /// </summary>
        public string InvoiceTitle;
        /// <summary>
        /// 发票税号
        /// </summary>
        public string TaxpayerId;
        public PayType? PayType;

        /// <summary>
        /// 收货人地址
        /// </summary>
        public string DeliveryAddr;
        /// <summary>
        /// 收货人姓名
        /// </summary>
        public string CustomerName;
        /// <summary>
        /// 收货人电话
        /// </summary>
        public string Cellphone;
        /// <summary>
        /// 配送费
        /// </summary>
        public double ShippingFee;
        /// <summary>
        /// 餐盒费
        /// </summary>
        public double PackageFee;
        /// <summary>
        /// 订单原价
        /// </summary>
        public double OriginalPrice;
        /// <summary>
        /// 用户实际支付金额
        /// </summary>
        public double ReceiptsMoney;
        /// <summary>
        /// 店铺实收金额
        /// </summary>
        public double? IncomeMoney;
        public List<Detail> Details = new List<Detail>();
        public List<DiscountInfo> DiscountInfos = new List<DiscountInfo>();
        /// <summary>
        /// 备注信息，优惠信息
        /// </summary>
        public List<string> Comments = new List<string>();
    }
    public class StoreMapInfo
    {
        public int ErpStoreId;
        /// <summary>
        /// token值，有些外卖平台这个值会是null，因为不需要token
        /// </summary>
        public string Token;
        /// <summary>
        /// 用于update Token
        /// </summary>
        public string Refresh_token;
        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime? Expires;
    }

}
