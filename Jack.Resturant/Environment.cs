using System;
using System.Collections.Generic;
using System.Text;

namespace Jack.Resturant
{
    /// <summary>
    /// 环境设置
    /// </summary>
    public class Environment
    {
        /// <summary>
        /// 是否采用测试账号（或者沙箱模式）
        /// </summary>
        public static bool IsTestMode
        {
            get;
            set;
        }
    }
}
