using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RePushOrder.Impls
{
    class Ele : IPush
    {
        public void Push(string content, string url)
        {
            var ms = typeof(Jack.Resturant.IResturant).Assembly.GetType("Jack.Resturant.Helper").GetMethods().Where(m => m.Name == "PostJson" && m.GetParameters().Length == 3);
            foreach (var method in ms)
            {
                try
                {
                    method.Invoke(null, new object[] { $"{url}/Jack_Resturant_ELE_Push", content, 8000 });
                }
                catch (System.ArgumentException ex)
                {

                }
            }
        }
    }
}
