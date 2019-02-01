using Spice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.Utility
{
    public static class SD
    {
        public const String DEFAULT_IMAGE = "default_food.png";
        public const String Manager = "Manager";
        public const String Kitchen = "Kitchen";
        public const String FrontDesk = "FrontDesk";
        public const String Customer = "Customer";
        public const String CartCountSession = "ssCartCount";
        public const String CouponCodeSession = "ssCouponCode";
        public const String OrderSubmitted = "Submitted";
        public const String OrderProcess = "Working on it";
        public const String OrderReady = "You Can Pick it now";
        public const String OrderDone = "We hope you enjoyed it";
        public const String OrderCancel = "Sorry to see it canceled";
        public const String PayementPending = "Pending";
        public const String PayementApproved = "We took your hard earned money";
        public const String PayementRejected = "We rejected your money";

        public static string ConvertToRawHtml(string source)
        {
            char[] array = new char[source.Length];
            int arrayIndex = 0;
            bool inside = false;

            for (int i = 0; i < source.Length; i++)
            {
                char let = source[i];
                if (let == '<')
                {
                    inside = true;
                    continue;
                }
                if (let == '>')
                {
                    inside = false;
                    continue;
                }
                if (!inside)
                {
                    array[arrayIndex] = let;
                    arrayIndex++;
                }
            }
            return new string(array, 0, arrayIndex);
        }

        public static double CalculateDiscount(Coupon coupon, double orderPrice)
        {
            if (coupon == null)
            {
                return orderPrice;
            }
            else
            {
                if (coupon.MinAmount > orderPrice)
                {
                    return orderPrice;
                }
                else
                {
                    if (Convert.ToInt32(coupon.Type) == (int)Coupon.ECouponType.Dollar)
                    {
                        return Math.Round(orderPrice - coupon.Discount,2);
                    }
                    else
                    {
                        if (Convert.ToInt32(coupon.Type) == (int)Coupon.ECouponType.Percent)
                        {
                            return Math.Round(orderPrice - (coupon.Discount*coupon.Discount/100), 2);
                        }
                    }
                }
            }

            return orderPrice;

        }

    }
}
