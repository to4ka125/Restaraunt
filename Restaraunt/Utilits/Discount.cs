using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaraunt.Utilits
{
    public static class Discount
    {
        public static double DiscountFun(double price)
        {
            if (price >=5000)
            {
                return price - (price * 0.05);
            }
            else
            {
                return price;
            }
        }
    }
}
