using System;
using System.Collections.Generic;
using System.Text;

namespace Jack.Resturant
{
    class ResturantTypeAttribute : Attribute
    {
        public ResturantPlatformType PlatformType
        {
            get;
            private set;
        }
        public ResturantTypeAttribute(ResturantPlatformType platformtype)
        {
            this.PlatformType = platformtype;
        }
    }
}
