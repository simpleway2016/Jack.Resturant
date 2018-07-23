using System;
using System.Collections.Generic;
using System.Text;

namespace Jack.Resturant
{
    public enum StoreStatus
    {
        /// <summary>
        /// 营业中
        /// </summary>
        Opened = 1,
        /// <summary>
        /// 休业
        /// </summary>
        Closed = 2
    }
    /// <summary>
    /// 一天的营业时间段
    /// </summary>
    public class DayOpenTime
    {
        /// <summary>
        /// 格式：08:00-14:00
        /// 如果当天不销售，只要保持0个元素就可以
        /// </summary>
        public List<string> Times = new List<string>();
    }
    public class StoreInfo
    {
        /// <summary>
        /// 状态
        /// </summary>
        public StoreStatus Status;
        /// <summary>
        /// 门店名称
        /// </summary>
        public string Name;
        /// <summary>
        /// 门店地址
        /// </summary>
        public string Address;
        /// <summary>
        /// 公告信息
        /// </summary>
        public string NoticeInfo;
        /// <summary>
        /// 门店电话
        /// </summary>
        public List<string> Phones = new List<string>();
        /// <summary>
        /// 营业时间
        /// 支持 周一到周日分别设置的店铺，返回7个DayOpenTime对象
        /// 每天营业时间一致的店铺，返回1个DayOpenTime对象
        /// </summary>
        public List<DayOpenTime> DayOpenTimes = new List<DayOpenTime>();
    }
}
