using BehpardakhtMVC.ir.shaparak.bpm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BehpardakhtMVC
{
    public class Behpardakht
    {
        private int SystemId = 2;
        public static readonly string PgwSite = "https://bpm.shaparak.ir/pgwchannel/startpay.mellat";
        // وقتی درگاه بانکی را دریافت می کنید این کدها برای شما ارسال میشود
        public const int OtherId = 1;
        public const string behpardakht_username = "username";
        public const string behpardakht_password = "password";

        //آدرسی صفحه ای از پیج شما که پس از پرداخت به آن برمیگردید
        public const string callbackurl = "";
        public enum payStatusEnum
        {
            Success = 1,
            Fail = 3
        }

        public KeyValuePair<string, string> errorCode1 = new KeyValuePair<string, string>("-900", "خطا در  تطبیق اطلاعات پرداخت با به پرداخت رخ داد");
        public KeyValuePair<string, string> errorCode2 = new KeyValuePair<string, string>("-901", "وضعیت خرید نامشخص می باشد");
        public KeyValuePair<string, string> errorCode3 = new KeyValuePair<string, string>("-902", "وضعیت نا‌مشخص در‌اتصال به‌پرداخت رخ داده است، مجددا امتحان کنید");
        public KeyValuePair<string, string> errorCode4 = new KeyValuePair<string, string>("-999", "خطای سیستمی رخ داد");


        /// <summary>
        /// استارت ارتباط با یانک 
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="payDate"></param>
        /// <param name="time"></param>
        /// <param name="additionalInfo"></param>
        /// <param name="payerId"></param>
        /// <returns>اگر اکی باشد، خروجی RefCode خواهد بود 
        /// وگرنه خروجی Null خواهد بود که نشان دهنده رخ دادن خطا است</returns>
        public KeyValuePair<string, string> bpPayRequest(long amount, DateTime payDate, TimeSpan time, string additionalInfo, long payId, string userIP, int? payType)
        {
            long payerId = 0;//این باید طبق قوانین به پرداخت صفر باشد!
            payId = 0;
            KeyValuePair<string, string> result;
            try
            {
                //گرفتن رکورد پرداخت قبل از تایید پرداخت                    
                //PaysClass payClass = new PaysClass();
                // payId = payClass.Add(amount, payerId, DateTime.Now, additionalInfo, behpardakhtSystem.SystemId, userIP,payType:payType);

                PaymentGatewayImplService bpService = new PaymentGatewayImplService();
                //ارسال درخواست اتصال به بانک
                var response = bpService.bpPayRequest(OtherId,
                  behpardakht_username,
                  behpardakht_password,
                  payId,
                  amount,
                  payDate.Year.ToString() + payDate.Month.ToString().PadLeft(2, '0') + payDate.Day.ToString().PadLeft(2, '0'),
                  payDate.Hour.ToString().PadLeft(2, '0') + payDate.Minute.ToString().PadLeft(2, '0') + payDate.Second.ToString().PadLeft(2, '0'),
                  additionalInfo,
                  callbackurl,
                  payerId);

                String[] resultArray = response.Split(',');
                if (resultArray[0] == "0")
                {
                   
                    result = new KeyValuePair<string, string>(resultArray[0], resultArray[1]);
                    RunPostScript(resultArray[1]);
                    return result;

                }
                else if (string.IsNullOrEmpty(response))
                {
                    return errorCode3;
                }
                else
                {
                    result = new KeyValuePair<string, string>(resultArray[0], ResCodeFarsi(resultArray[0]));
                    return result;
                }




            }
            catch (Exception ex)
            {

                throw ex;

            }
        }



        public KeyValuePair<string, string> bpVerifyRequest(long saleOrderId, long saleReferenceId, string refId, long userId)
        {
            userId = 0;//این باید طبق قوانین به پرداخت صفر باشد!
            long payId = 0;
            string response1 = "";
            string response2 = "";

            try
            {

                PaymentGatewayImplService bpService = new PaymentGatewayImplService();

                response1 = bpService.bpVerifyRequest(OtherId,
                behpardakht_username,
                behpardakht_password,
                payId,
                saleOrderId,
                saleReferenceId);


                if (response1 == "0" || response1 == "43")
                {
                    try
                    {
                        response2 = bpSettleRequest(saleOrderId, saleReferenceId, refId, userId);
                        return new KeyValuePair<string, string>(response2, ResCodeFarsi(response2));
                    }
                    catch
                    {
                        return errorCode4;//خطای سیستمی
                    }
                }
                else if (string.IsNullOrEmpty(response1))
                {
                    try
                    {
                        response2 = bpInqiryRequest(saleOrderId, saleReferenceId, refId, userId);
                        return new KeyValuePair<string, string>(response2, ResCodeFarsi(response2));
                    }
                    catch
                    {
                        return errorCode4;//خطای سیستمی
                    }
                }
                else
                {
                    return new KeyValuePair<string, string>(response1, ResCodeFarsi(response1));
                }
            }
            catch
            {
                return errorCode4;//خطای سیستمی

            }

        }


        private string bpSettleRequest(long saleOrderId, long saleReferenceId, string refId, long userId)
        {
            userId = 0;//این باید طبق قوانین به پرداخت صفر باشد!

            string response = "";
            long payId = 0;



            PaymentGatewayImplService bpService = new PaymentGatewayImplService();
            try
            {
                //درخواست واریز وجه
                response = bpService.bpSettleRequest(OtherId,
                behpardakht_username,
                behpardakht_password,
                  payId,
                  saleOrderId,
                  saleReferenceId);
            }
            catch
            {
                return "-999";//خطای سیستمی
            }

            if (response == "0" || response == "45")
            {

                return response;

            }
            else if (string.IsNullOrEmpty(response))
            {

                return "-901";//وضعیت نامشخص
            }
            else
            {
                return response;
            }



        }

        private string bpInqiryRequest(long saleOrderId, long saleReferenceId, string refId, long userId)
        {
            userId = 0;//این باید طبق قوانین به پرداخت صفر باشد!

            string response1 = "", response2 = "";
            long payId = 0;


            PaymentGatewayImplService bpService = new PaymentGatewayImplService();
            try
            {
                //برداشت وجه از حساب
                response1 = bpService.bpInquiryRequest(OtherId,
                behpardakht_username,
                behpardakht_password,
                  payId,
                  saleOrderId,
                  saleReferenceId);
            }
            catch
            {

                return "-999";//خطای سیستمی
            }

            if (response1 == "0" || response1 == "43")
            {
                response2 = bpSettleRequest(saleOrderId, saleReferenceId, refId, userId);
                return response2;
            }
            else if (string.IsNullOrEmpty(response1))
            {
                return "-901";//وصضعیت نامشخص
            }
            else
            {
                return response1;
            }

        }




        public string ResCodeFarsi(string ResCode)
        {
            switch (ResCode)
            {
                //خطاهای به‌پرداخت
                case "0":
                    return "تراکنش با موفقیت انجام شد";
                case "11":
                    return "شماره کارت نامعتبر است";
                case "12":
                    return "موجودی کافی نیست";
                case "13":
                    return "رمز نادرست است";
                case "14":
                    return "تعداد دفعات وارد کردن رمز بیش از حد مجاز است";
                case "15":
                    return "کارت نامعتبر است";
                case "16":
                    return "دفعات برداشت وجه بیش از حد مجاز است";
                case "17":
                    return "کاربر از انجام تراکنش منصرف شده است";
                case "18":
                    return "تاریخ انقضای کارت گذشته است";
                case "19":
                    return "مبلغ برداشت وجه بیش از حد مجاز است";
                case "111":
                    return "صادر کننده کارت نامعتبر است";
                case "112":
                    return "خطای سوییچ صادر کننده کارت";
                case "113":
                    return "پاسخی از صادر کننده کارت دریافت نشد";
                case "114":
                    return "دارنده کارت مجاز به انجام این تراکنش نیست";
                case "21":
                    return "پذیرنده نامعتبر است";
                case "23":
                    return "خطای امنیتی رخ داده است";

                case "24":
                    return "اطلاعات کاربری پذیرنده نامعتبر است";

                case "25":
                    return "مبلغ نامعتبر است";


                case "31":
                    return "پاسخ نامعتبر است";
                case "32":
                    return "فرمت اطلاعات وارد شده صحیح نمی باشد";

                case "33":
                    return "حساب نامعتبر است";
                case "34":
                    return "خطای سیستمی";
                case "35":
                    return "تاریخ نامعتبر است";
                case "41":
                    return "شماره درخواست تکراری است";
                case "42":
                    return "تراکنش sale یافت نشد";
                case "43":
                    return "قبلا درخواست verify داده شده است";
                case "44":
                    return "درخواست verify یافت نشد";
                case "45":
                    return "تراکنش settle شده است";
                case "46":
                    return "تراکنش settle نشده است";
                case "47":
                    return "تراکنش settle یافت نشد";
                case "48":
                    return "تراکنش reverse شده است";
                case "49":
                    return "تراکنش refund یافت نشد";
                case "412":
                    return "شناسه قبض نادرست است";
                case "413":
                    return "شناسه پرداخت نادرست است";
                case "414":
                    return "سازمان صادر کننده قبض نامعتبر است";
                case "415":
                    return "زمان جلسه کاری به پایان رسیده است";
                case "416":
                    return "خطا در ثبت اطلاعات";
                case "417":
                    return "شناسه پرداخت کننده نادرست است";
                case "418":
                    return "اشکال در تعریف اطلاعات مشتری";
                case "419":
                    return "تعداد دفعات ورود اطلاعات از حد مجاز گذشته است";
                case "421":
                    return "IP نا معتبر است";
                case "51":
                    return "تراکنش تکراری است";
                case "54":
                    return "تراکنش مرجع موجود نیست";
                case "55":
                    return "تراکنش نامعتبر است";
                case "61":
                    return "خطا در واریز";

                //خطاهای سیستمی

                case "-900":
                    return "خطا در  تطبیق اطلاعات پرداخت با به پرداخت رخ داد";
                case "-901":
                    return "وضعیت خرید نامشخص می باشد";
                case "-902":
                    return "وضعیت نا‌مشخص در‌اتصال به‌پرداخت رخ داده است، مجددا امتحان کنید";
                case "-999":
                    return "خطای سیستمی رخ داد";

                default:
                    return "";
            }
        }

        /// <summary>
        /// متد برای اتصال به درگاه به‌پرداخت
        /// </summary>
        /// <param name="RefId"></param>
        private void RunPostScript(string RefId)
        {
            System.Web.HttpContext.Current.Response.Clear();
            var sb = new System.Text.StringBuilder();
            sb.Append("<html>");
            sb.AppendFormat("<body onload='document.forms[0].submit()'>");
            sb.AppendFormat("<form action='{0}' method='post' target='_self'>", PgwSite);
            sb.AppendFormat("<input type='hidden' name='RefId' value='{0}'>", RefId);
            sb.Append("</form>");
            sb.Append("</body>");
            sb.Append("</html>");
            System.Web.HttpContext.Current.Response.Write(sb.ToString());
            System.Web.HttpContext.Current.Response.End();

        }



    }
}
