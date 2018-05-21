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
            Authority = "";
            PaySystemsClass system = new PaySystemsClass();
            var zarinpalSystem = system.Select(systemId: 1).FirstOrDefault();
            if (zarinpalSystem != null)
            {
                com.zarinpal.www.PaymentGatewayImplementationService zarinService = new com.zarinpal.www.PaymentGatewayImplementationService();

                string MerchantId = zarinpalSystem.UserName;
                int status = zarinService.PaymentRequest(MerchantId, amount, description, email, mobileNum, zarinpalSystem.CallBackUrl + callbacktail, out Authority);
                if (status == 100)
                {
                    //ایجاد رکورد پرداخت در جدول قبل از تکمیل پرداخت                    
                    PaysClass pay = new PaysClass();
                    long payId = pay.Add(amount, userId, DateTime.Now, description, zarinpalSystem.SystemId, userIP);
                    AddPaymentAuthority(payId, Authority);

                    return payId;
                }
                else
                {
                    new PayLogClass().Add("PaymentRequest Status!=100", DateTime.Now, paystatus: status);

                    return -999;
                }
            }
            else {
                return 2;

            }
        }

        /// <summary>
        /// درخواست تایید پرداخت 
        /// </summary>
        /// <param name="Authority">شناسه درخواست 36 کاراکتری</param>
        /// <param name="Status">وضعیت=100 موفقیت آمیز بودن پرداخت</param>
        /// <returns></returns>
        public int Verify(string Authority, string Status)
        {
            if (Authority != "" && Authority != null && Status != null && Status != "")
            {
                var pay = GetPaymentByAuthority(Authority);
                if (Status.ToLower().Equals("ok"))
                {
                    PaySystemsClass system = new PaySystemsClass();
                    var zarinpalSystem = system.Select(systemId: 1).FirstOrDefault();

                    if (zarinpalSystem != null)
                    {
                        string MerchantId = zarinpalSystem.UserName;
                        com.zarinpal.www.PaymentGatewayImplementationService zarinService = new com.zarinpal.www.PaymentGatewayImplementationService();
                        long RefId = 0;
                        int amnt = (int)pay.PayAmount;
                        int verifyStatus = zarinService.PaymentVerification(MerchantId, Authority, amnt, out RefId);

                        AddPaymentVerifyStatus(pay.PaymentId, verifyStatus);

                        if (verifyStatus == 100 || verifyStatus == 101)

                        {
                            new PaysClass().Update(pay.PaymentId, 1);
                            AddPaymentRefrenceId(pay.PaymentId, RefId);
                            new PayLogClass().Add("PaymentVerification Successful", DateTime.Now, payId: pay.PaymentId, paystatus: verifyStatus);
                            return verifyStatus;
                        }
                        else
                        {
                            new PaysClass().Update(pay.PaymentId, 3);
                            new PayLogClass().Add("PaymentVerification Error!=100", DateTime.Now, payId: pay.PaymentId, paystatus: verifyStatus);

                            //throw new Exception("Error!! Status: " + verifyStatus);
                            throw new Exception(verifyStatus.ToString());
                        }
                    }
                    else
                    {
                        new PayLogClass().Add("خطای اطلاعات پایگاه داده در جدول ،PaymentSystems پرداخت زرین پال موجود نمی باشد", DateTime.Now, payId: pay.PaymentId);

                        throw new Exception("سیستم پرداخت زرین پال موجود نمی باشد");
                    }

                }
                else
                // status = nok
                {
                    new PayLogClass().Add("تراکنش ناموفق بوده یا توسط کاربر لغو شده است،Status=NOK", DateTime.Now, payId: pay.PaymentId);

                    throw new Exception("تراکنش ناموفق بوده یا توسط کاربر لغو شده است" + "\n" + "Status:" + Status);
                }
            }
            else
            {
                new PayLogClass().Add("Invalid Input،Status and Authority are Empty", DateTime.Now);
                throw new Exception("ورودی ها نامعتبر می باشند");
            }
        }

        public Payment_Pays GetPaymentByAuthority(string Authority)
        {
            try
            {
                var payDet = new PayDetailsClass().Select(detailTitle: "Authority", detailValue: Authority).FirstOrDefault();
                var pay = new PaysClass().Select(payId: payDet.PaymentId).FirstOrDefault();
                return pay;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public Payment_Pays GetPaymentBypayId(int payId)
        {
            try
            {
                var pay = new PaysClass().Select(payId: payId).FirstOrDefault();
                return pay;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void AddPaymentAuthority(long payId, string Authority)
        {
            try
            {
                new PayDetailsClass().Add(payId, detailTitle: "Authority", detailVal: Authority);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void AddPaymentRefrenceId(long payId, long RefId)
        {
            try
            {
                new PayDetailsClass().Add(payId, detailTitle: "RefId", detailVal: RefId.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void AddPaymentVerifyStatus(long payId, int status)
        {
            try
            {
                new PayDetailsClass().Add(payId, detailTitle: "VerifyStatus", detailVal: status.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetWebGateAddress(string authority)
        {
            return string.Format("http://www.zarinpal.com/pg/StartPay/{0}", authority);
        }

        public string GetZarinGateAddress(string authority)
        {
            return string.Format("https://www.zarinpal.com/pg/StartPay/{0}/ZarinGate", authority);
        }
    }
}
