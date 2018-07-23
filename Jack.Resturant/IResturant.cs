using System;
using System.Collections.Generic;
using System.Text;

namespace Jack.Resturant
{
  
    public interface IResturant
    {
        /// <summary>
        /// 显示需要使用者自行设置的回调地址信息
        /// </summary>
        /// <param name="urlDomain">服务器域名，如：http://www.test.com</param>
        /// <returns></returns>
        IEnumerable<string> ShowCallbackUrlSetting(string urlDomain);

        /// <summary>
        /// 把Erp系统中的门店与接口平台的门店建立对应关系，结果会回调IResturantListener.OnStoreMapSuccess或者IResturantListener.OnCompanyMapSuccess
        /// 美团是一次只能绑定一个门店，所以会回调IResturantListener.OnStoreMapSuccess
        /// 饿了么是一次绑定所有门店，所以会回调IResturantListener.OnCompanyMapSuccess
        /// </summary>
        /// <param name="companyId">集团id</param>
        /// <param name="erpStoreId">门店在Erp系统中的id</param>
        /// <param name="erpStoreName">门店在Erp系统中的名称</param>
        string StoreMap(int companyId,int erpStoreId,string erpStoreName);

        /// <summary>
        /// 解除门店映射
        /// 结果会回调IResturantListener.OnReleaseStoreMapSuccess
        /// </summary>
        /// <param name="token"></param>
        /// <param name="erpStoreId"></param>
        /// <returns>返回值为url地址，如果是null，表示不需要跳转url</returns>
        string ReleaseStoreMap(string token, int erpStoreId);

        /// <summary>
        /// 确认接受订单
        /// </summary>
        /// <param name="parameter"></param>
        void ConfirmOrder(ConfirmOrderParameter parameter);

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="parameter"></param>
        void CancelOrder(CancelOrderParameter parameter);
        /// <summary>
        /// 获取订单平台服务费
        /// </summary>
        /// <param name="token"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        ServiceAmountInfo GetOrderServiceAmount(string token, string orderId);

        /// <summary>
        /// 同意取消订单，同意订单退款
        /// </summary>
        /// <param name="token"></param>
        /// <param name="orderId"></param>
        void ArgeeOrderRefund(string token,string orderId);
        /// <summary>
        /// 拒绝取消订单
        /// </summary>
        /// <param name="token"></param>
        /// <param name="orderId">订单号</param>
        /// <param name="reason">拒绝原因</param>
        void DisargeeOrderRefund(string token, string orderId, string reason);
        /// <summary>
        /// 商家自己开始配送
        /// </summary>
        /// <param name="parameter"></param>
        void Delivering(DeliverParameter parameter);
        /// <summary>
        /// 配送完成
        /// </summary>
        /// <param name="parameter"></param>
        void Delivered(DeliveredParameter parameter);
        /// <summary>
        /// 获取菜品列表
        /// </summary>
        /// <returns></returns>
        List<DishInfo> GetDishList(int erpStoreId,String token);

        /// <summary>
        /// 菜品映射
        /// </summary>
        /// <param name="parameter"></param>
        void DishMap(DishMapParameter parameter);
        /// <summary>
        /// 删除菜品
        /// </summary>
        /// <param name="erpStoreId"></param>
        /// <param name="token"></param>
        /// <param name="erpDishId">Erp的菜品id</param>
        /// <param name="dishId">外卖平台的菜品id</param>
        void DeleteDish(int erpStoreId,string token, string erpDishId,string dishId);
        ///// <summary>
        ///// 删除子菜品
        ///// </summary>
        ///// <param name="erpStoreId"></param>
        ///// <param name="token"></param>
        ///// <param name="erpDishId"></param>
        ///// <param name="erpSkuId"></param>
        //void DeleteDishSku(int erpStoreId, string token, string erpDishId, string erpSkuId);

        /// <summary>
        /// 删除菜品分类
        /// </summary>
        /// <param name="erpStoreId"></param>
        /// <param name="token"></param>
        /// <param name="categoryName"></param>
        void DeleteDishCategory(int erpStoreId, string token, string categoryName);
        /// <summary>
        /// 新建菜品分类
        /// </summary>
        /// <param name="parameter"></param>
        void CreateDishCategory(CreateDishCategoryParameter parameter);
        /// <summary>
        /// 更新菜品分类
        /// </summary>
        /// <param name="parameter"></param>
        void UpdateDishCategory(UpdateDishCategoryParameter parameter);
        /// <summary>
        /// 获取菜品分类list
        /// </summary>
        /// <param name="erpStoreId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        List<DishCategory> GetDishCategoryList(int erpStoreId, String token);
        /// <summary>
        /// 上传菜品图片，
        /// 注意：百度外卖使用这个方法，必须要求你当前运行的站点支持http访问，只支持https的话，会产生系统错误
        /// </summary>
        /// <param name="erpStoreId"></param>
        /// <param name="token"></param>
        /// <param name="imageName">美团要求：只能jpg文件，文件名只能是字母或数字,且必须以.jpg结尾</param>
        /// <param name="picContent"></param>
        /// <returns>返回图片id或路径</returns>
        string UploadDishPicture(int erpStoreId, String token,string imageName,byte[] picContent);
        /// <summary>
        /// 上传菜品信息，erp id相同的菜品会被更新，其余的则是新增
        /// </summary>
        /// <param name="erpStoreId"></param>
        /// <param name="token"></param>
        /// <param name="dishes"></param>
        void UploadDish(int erpStoreId, String token, List<DishInfo> dishes);
        /// <summary>
        /// 获取店铺信息，包括状态
        /// </summary>
        /// <returns></returns>
        StoreInfo GetStoreInfo(int erpStoreId, String token);
        /// <summary>
        /// 设置门店状态
        /// </summary>
        /// <param name="erpStoreId"></param>
        /// <param name="token"></param>
        /// <param name="status"></param>
        void SetStoreStatus(int erpStoreId, String token,StoreStatus status);
        /// <summary>
        /// 设置门店营业时间
        /// </summary>
        /// <param name="erpStoreId"></param>
        /// <param name="token"></param>
        /// <param name="dayOpenTimes">
        /// 对于支持从周一到周日的分别营业时间的，必须是7个DayOpenTime对象，如果某一天不营业，它的Times可以是null
        /// 对于每天时间一致的店铺，必须是1个DayOpenTime对象
        /// DayOpenTime格式：08:00-12:00
        /// </param>
        void SetStoreOpenTimes(int erpStoreId, String token, List<DayOpenTime> dayOpenTimes);
        /// <summary>
        /// 更新tokenm，此方法调用成功后，会触发IResturantListener.OnCompanyMapSuccess或者IResturantListener.OnStoreMapSuccess方法
        /// </summary>
        /// <param name="erpStoreId"></param>
        /// <param name="token"></param>
        /// <param name="refresh_token"></param>
        /// <returns></returns>
        TokenResult UpdateToken(int erpStoreId, string token, string refresh_token);
    }

    /// <summary>
    /// 平台服务费信息
    /// </summary>
    public class ServiceAmountInfo
    {
        /// <summary>
        /// 平台服务费
        /// </summary>
        public double? PlatformServiceAmount;
        /// <summary>
        /// 商家实际到账金额
        /// </summary>
        public double? SettleAmount;
    }

    public class ConfirmOrderParameter
    {
        public string OrderID;
        public string Token;
    }
    public class TokenResult
    {
        public string Token;
        public string RefreshToken;
        /// <summary>
        /// 有效期
        /// </summary>
        public DateTime? Expires;
    }
    public enum CancelOrderReason
    {
        /// <summary>
        /// 其他原因
        /// </summary>
        Other = 0,
        /// <summary>
        /// 地址无法配送
        /// </summary>
        AddressError = 1,
        /// <summary>
        /// 店铺太忙
        /// </summary>
        Busy = 2,
        /// <summary>
        /// 商品已售完
        /// </summary>
        SoldOut = 3,
        /// <summary>
        /// 店铺已打烊
        /// </summary>
        StoreClosed = 4,
        /// <summary>
        /// 联系不上用户
        /// </summary>
        CanNotContact = 5,
        /// <summary>
        /// 重复订单
        /// </summary>
        RepeatOrder = 6,
        /// <summary>
        /// 配送员取餐慢
        /// </summary>
        TakeFoodSlowly = 7,
        /// <summary>
        /// 配送员送餐慢
        /// </summary>
        DeliverSlowly = 8,
        /// <summary>
        /// 配送员丢餐、少餐、餐洒
        /// </summary>
        LostFood = 9,
        /// <summary>
        /// 用户要求取消
        /// </summary>
        CustomerCancel = 10,
        /// <summary>
        /// 配送延迟
        /// </summary>
        DeliverDelay = 11,
    }
    public class CancelOrderParameter
    {
        public string OrderID;
        public string Token;
        public CancelOrderReason Reason;
    }

    public class DeliverParameter
    {
        public string OrderID;
        public string Token;
        /// <summary>
        /// 配送员姓名
        /// </summary>
        public string CourierName;
        /// <summary>
        /// 配送员电话
        /// </summary>
        public string CourierPhone;
    }
    public class DeliveredParameter
    {
        public string OrderID;
        public string Token;
    }
    public class DishMapParameter
    {
        public class DishMapItem
        {
            /// <summary>
            /// Erp中的dishId
            /// </summary>
            public string ErpDishId;
            /// <summary>
            /// 外卖平台dishId
            /// </summary>
            public string DishId;
        }
        /// <summary>
        /// Erp系统中门店的id
        /// </summary>
        public int ErpStoreID;
        public string Token;
        public List<DishMapItem> DishMapItems = new List<DishMapItem>();
    }
    public class CreateDishCategoryParameter
    {
        public int ErpStoreId;
        public string Token;
        /// <summary>
        /// 分类新名称
        /// </summary>
        public string CategoryName;
        /// <summary>
        /// 菜品排序【数字越小，排名越靠前,不同分类顺序可以相同 】
        /// </summary>
        public uint Sequence;

    }
    public class UpdateDishCategoryParameter : CreateDishCategoryParameter
    {
        /// <summary>
        /// 需要更新的分类的原始名称
        /// </summary>
        public string OriginalCategoryName;
    }
}
