using PaymentLib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentLib
{
    public class ZarinPal
    {
        private int SystemId = 1;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="amount">مقدار پرداختی</param>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="description">توضیحات</param>
        /// <param name="email">ایمیل کاربر(اختیاری)</param>
        /// <param name="mobileNum">موبایل کاربر(اختیاری)</param>
        /// <returns>1=اتصال موفق
        ///          2=عدم وجود سیستم پرداختی مورد نظر در پایگاه داده
        ///          عدد منفی=اتصال به درگاه ناموفق باشد
        ///          </returns>
        public long SendRequest(int amount, int userId, string description, string email, string mobileNum, string userIP, out string Authority, string callbacktail)
        {
        }
    }
}
