using System;
using System.Collections.Generic;
using System.Text;

namespace Jack.Resturant
{
    class CallbackDescAttribute:Attribute
    {
        public string Desc
        {
            get;
            private set;
        }
        public CallbackDescAttribute(string desc)
        {
            this.Desc = desc;
        }
    }
}
