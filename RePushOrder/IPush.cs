using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RePushOrder
{
    interface IPush
    {
        void Push(string content, string url);
    }
}
