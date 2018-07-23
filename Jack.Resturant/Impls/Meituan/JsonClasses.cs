using System;
using System.Collections.Generic;
using System.Text;

namespace Jack.Resturant.Impls.Meituan
{
    class JCreateOrder
    {
        public double? avgSendTime { get; set; }

        public string caution { get; set; }

        public int? cityId { get; set; }

        public int ctime { get; set; }

        public string daySeq { get; set; }

        public int? deliveryTime { get; set; }

        public string detail { get; set; }

        public int? dinnersNumber { get; set; }

        public string ePoiId { get; set; }

        public string extras { get; set; }

        public int? hasInvoiced { get; set; }

        public string invoiceTitle { get; set; }

        public bool isFavorites { get; set; }

        public bool isPoiFirstOrder { get; set; }

        public int? isThirdShipping { get; set; }

        public double? latitude { get; set; }

        public string logisticsCode { get; set; }

        public double? longitude { get; set; }

        public string orderId { get; set; }

        public int? orderIdView { get; set; }

        public double? originalPrice { get; set; }

        public int? payType { get; set; }

        public string poiAddress { get; set; }

        public bool poiFirstOrder { get; set; }

        public int? poiId { get; set; }

        public string poiName { get; set; }

        public string poiPhone { get; set; }

        public string poiReceiveDetail { get; set; }

        public string recipientAddress { get; set; }

        public string recipientName { get; set; }

        public string recipientPhone { get; set; }

        public string shipperPhone { get; set; }

        public double? shippingFee { get; set; }

        public int? status { get; set; }

        public string taxpayerId { get; set; }

        public double? total { get; set; }

        public int? utime { get; set; }

    }

    class JCreateOrder_Extra
    {
        public int act_detail_id { get; set; }

        public double mt_charge { get; set; }

        public double poi_charge { get; set; }

        public double reduce_fee { get; set; }

        public string remark { get; set; }

        public int type { get; set; }

    }

    class JCreateOrder_PoiReceiveDetail
    {
        public JCreateOrder_PoiReceiveDetail_ActOrderChargeByMt[] actOrderChargeByMt { get; set; }

        public JCreateOrder_PoiReceiveDetail_ActOrderChargeByPoi[] actOrderChargeByPoi { get; set; }

        public int? foodShareFeeChargeByPoi { get; set; }

        public int? logisticsFee { get; set; }

        public int? onlinePayment { get; set; }

        public int? wmPoiReceiveCent { get; set; }

    }


    class JCreateOrder_PoiReceiveDetail_ActOrderChargeByMt
    {
        public string comment { get; set; }

        public string feeTypeDesc { get; set; }

        public int? feeTypeId { get; set; }

        public int? moneyCent { get; set; }

    }


    class JCreateOrder_PoiReceiveDetail_ActOrderChargeByPoi
    {
        public string comment { get; set; }

        public string feeTypeDesc { get; set; }

        public int? feeTypeId { get; set; }

        public int? moneyCent { get; set; }

    }

    class JCreateOrder_Detail
    {
        public string app_food_code { get; set; }

        public int box_num { get; set; }

        public double box_price { get; set; }

        public int cart_id { get; set; }

        public double food_discount { get; set; }

        public string food_name { get; set; }

        public string food_property { get; set; }

        public double price { get; set; }

        public int quantity { get; set; }

        public string sku_id { get; set; }

        public string spec { get; set; }

        public string unit { get; set; }

    }




}
