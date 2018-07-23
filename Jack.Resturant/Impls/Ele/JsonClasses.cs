using System;
using System.Collections.Generic;
using System.Text;

namespace Jack.Resturant.Impls.Ele
{
    class JOrder
    {
        public string id { get; set; }

        public string orderId { get; set; }

        public string address { get; set; }

        public DateTime? createdAt { get; set; }

        public DateTime? activeAt { get; set; }

        public double? deliverFee { get; set; }

        public DateTime? deliverTime { get; set; }

        public string description { get; set; }

        public JOrder_Groups[] groups { get; set; }

        public string invoice { get; set; }

        public bool book { get; set; }

        public bool onlinePaid { get; set; }

        public object railwayAddress { get; set; }

        public string[] phoneList { get; set; }

        public int shopId { get; set; }

        public string shopName { get; set; }

        public int daySn { get; set; }

        public string status { get; set; }

        public string refundStatus { get; set; }

        public int userId { get; set; }

        public double? totalPrice { get; set; }

        public double? originalPrice { get; set; }

        public string consignee { get; set; }

        public string deliveryGeo { get; set; }

        public string deliveryPoiAddress { get; set; }

        public bool invoiced { get; set; }

        public double? income { get; set; }

        public double? serviceRate { get; set; }

        public double? serviceFee { get; set; }

        public double? hongbao { get; set; }

        public double? packageFee { get; set; }

        public double? activityTotal { get; set; }

        public double? shopPart { get; set; }

        public double? elemePart { get; set; }

        public bool downgraded { get; set; }

        public double? vipDeliveryFeeDiscount { get; set; }

        public int? openId { get; set; }

        public object secretPhoneExpireTime { get; set; }

        public JOrder_OrderActivities[] orderActivities { get; set; }

        public object invoiceType { get; set; }

        public string taxpayerId { get; set; }

        public double? coldBoxFee { get; set; }

        public object cancelOrderDescription { get; set; }

        public object cancelOrderCreatedAt { get; set; }

    }
    class JOrder_Groups_Items_NewSpecs
    {
        public string name { get; set; }

        public string value { get; set; }
        public string extendCode { get; set; }
        public double? price { get; set; }
        public int? quantity { get; set; }
    }


    class JOrder_Groups_Items_Attributes
    {
        public string name { get; set; }

        public string value { get; set; }

    }

    class JOrder_Groups
    {
        public string name { get; set; }

        public string type { get; set; }

        public JOrder_Groups_Items[] items { get; set; }

    }


    class JOrder_Groups_Items
    {
        public int id { get; set; }

        public string skuId { get; set; }

        public string name { get; set; }

        public int categoryId { get; set; }

        public double? price { get; set; }

        public int quantity { get; set; }

        public double? total { get; set; }

        public object additions { get; set; }

        public JOrder_Groups_Items_NewSpecs[] newSpecs { get; set; }

        public JOrder_Groups_Items_Attributes[] attributes { get; set; }

        public string extendCode { get; set; }

        public string barCode { get; set; }

        public double? weight { get; set; }

        public double? userPrice { get; set; }

        public double? shopPrice { get; set; }

        public int vfoodId { get; set; }

    }


    class JOrder_OrderActivities
    {
        public int categoryId { get; set; }

        public string name { get; set; }

        public double? amount { get; set; }

        public double? elemePart { get; set; }

        public double? restaurantPart { get; set; }

        public int id { get; set; }

    }


    class JSettlement
    {
        public string orderId { get; set; }

        public string state { get; set; }

        public int? shopId { get; set; }

        public int? updateTime { get; set; }

        public int? role { get; set; }

    }




}
