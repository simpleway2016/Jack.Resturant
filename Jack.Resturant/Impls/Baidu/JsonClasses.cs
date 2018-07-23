using System;
using System.Collections.Generic;
using System.Text;

namespace Jack.Resturant.Impls.Baidu
{
    class JOrderData
    {
        public string source { get; set; }

        public JOrderData_Shop shop { get; set; }

        public JOrderData_Order order { get; set; }

        public JOrderData_User user { get; set; }

        public JOrderData_Products[][] products { get; set; }

        public JOrderData_Discount[] discount { get; set; }

        public JOrderData_Part_refund_info[] part_refund_info { get; set; }

    }


    class JOrderData_Shop
    {
        public string id { get; set; }

        public string name { get; set; }

        public string baidu_shop_id { get; set; }

    }


    class JOrderData_Order
    {
        public int? expect_time_mode { get; set; }

        public int? pickup_time { get; set; }

        public int? atshop_time { get; set; }

        public int? delivery_time { get; set; }

        public string delivery_phone { get; set; }

        public string finished_time { get; set; }

        public string confirm_time { get; set; }

        public string order_id { get; set; }

        public string order_index { get; set; }

        public int? status { get; set; }

        public int? send_immediately { get; set; }

        public int send_time { get; set; }

        public double send_fee { get; set; }

        public double package_fee { get; set; }

        public int? discount_fee { get; set; }

        public double total_fee { get; set; }

        public double shop_fee { get; set; }

        public double user_fee { get; set; }

        public int? pay_type { get; set; }

        public int? pay_status { get; set; }

        public int? need_invoice { get; set; }

        public string invoice_title { get; set; }
        public string taxer_id { get; set; }

        public string remark { get; set; }

        public int? delivery_party { get; set; }

        public int create_time { get; set; }

        public string cancel_time { get; set; }

    }


    class JOrderData_User
    {
        public string name { get; set; }

        public string phone { get; set; }

        public int? gender { get; set; }

        public string address { get; set; }

        public string province { get; set; }

        public string city { get; set; }

        public string district { get; set; }

        public JOrderData_User_Coord coord { get; set; }

    }


    class JOrderData_User_Coord
    {
        public double? longitude { get; set; }

        public double? latitude { get; set; }

    }


    class JOrderData_Products
    {
        public string baidu_product_id { get; set; }

        public string other_dish_id { get; set; }

        public string upc { get; set; }

        public string product_name { get; set; }

        public int? product_type { get; set; }

        public double product_price { get; set; }

        public int product_amount { get; set; }

        public int? product_fee { get; set; }

        public double package_price { get; set; }

        public int package_amount { get; set; }

        public int? package_fee { get; set; }

        public int? total_fee { get; set; }

        public string product_custom_index { get; set; }

        public JOrderData_Products_Product_attr[] product_attr { get; set; }

        public JOrderData_Products_Product_features[] product_features { get; set; }

    }

    class JOrderData_Discount
    {
        public double baidu_rate { get; set; }
        public double shop_rate { get; set; }
        public double fee { get; set; }
        public string desc { get; set; }
    }

    class JOrderData_Products_Product_attr
    {
        public string baidu_attr_id { get; set; }

        public string attr_id { get; set; }

        public string name { get; set; }

        public string option { get; set; }

    }


    class JOrderData_Products_Product_features
    {
        public string baidu_feature_id { get; set; }

        public string name { get; set; }

        public string option { get; set; }

    }


    class JOrderData_Part_refund_info
    {
        public string status { get; set; }

        public int? total_price { get; set; }

        public int? shop_fee { get; set; }

        public int? order_price { get; set; }

        public int? package_fee { get; set; }

        public int? send_fee { get; set; }

        public int? discount_fee { get; set; }

        public int? refund_price { get; set; }

        public int? refund_box_price { get; set; }

        public int? refund_send_price { get; set; }

        public int? refund_discount_price { get; set; }

        public int? refuse_platform { get; set; }

        public int? commission { get; set; }

        public JOrderData_Part_refund_info_Order_detail[][] order_detail { get; set; }

        public JOrderData_Part_refund_info_Refund_detail[][] refund_detail { get; set; }

        public object discount { get; set; }

    }


    class JOrderData_Part_refund_info_Order_detail
    {
        public string baidu_product_id { get; set; }

        public string upc { get; set; }

        public string product_name { get; set; }

        public int? product_type { get; set; }

        public int? product_price { get; set; }

        public int? product_amount { get; set; }

        public int? product_fee { get; set; }

        public int? package_price { get; set; }

        public string package_amount { get; set; }

        public int? package_fee { get; set; }

        public int? total_fee { get; set; }

        public object product_attr { get; set; }

        public object product_features { get; set; }

        public string product_custom_index { get; set; }

    }


    class JOrderData_Part_refund_info_Refund_detail
    {
        public string baidu_product_id { get; set; }

        public string other_dish_id { get; set; }

        public string upc { get; set; }

        public string product_name { get; set; }

        public int? product_type { get; set; }

        public int? product_price { get; set; }

        public int? product_amount { get; set; }

        public int? product_fee { get; set; }

        public int? package_price { get; set; }

        public string package_amount { get; set; }

        public int? package_fee { get; set; }

        public int? total_fee { get; set; }

        public object product_attr { get; set; }

        public object product_features { get; set; }

        public string product_custom_index { get; set; }

    }



}
