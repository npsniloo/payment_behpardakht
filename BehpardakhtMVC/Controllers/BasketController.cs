using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace BehpardakhtMVC.Controllers
{
    public class BasketController : Controller
    {
        // GET: Basket
        public ActionResult Index()
        {
            Models.Basket basket = new Models.Basket();
            using (Models.ShopEntities1 db = new Models.ShopEntities1())
            {

                basket.Amount = 1000;
                basket.Id = 1;
                basket.Registerdate = DateTime.Now;
                basket.Status = 0;
                db.Baskets.Add(basket);
                db.SaveChanges();
            }

            return View(basket);
        }

        [HttpPost]
        public void ConnectToBank(BehpardakhtMVC.Models.Basket basket)
        {
            DateTime date = DateTime.Now.Date;
            TimeSpan time = DateTime.Now.TimeOfDay;
            long amount = 1000;
            string additionalInfo = "";
            long payId = 0;
            using (Models.ShopEntities1 db = new Models.ShopEntities1())
            {
                var pay = new Models.Payment();
                pay.Amount = basket.Amount;
                pay.BasketId = basket.Id;
                pay.PayMethod = "online-mellat";
                pay.PayStatus = 0;
                db.Payments.Add(pay);
                db.SaveChanges();
                payId = pay.Id;
            }


            int? payType = null;
            BypassCertificateError();
            string ip = "";
            var ressult = new Behpardakht().bpPayRequest(amount, date, time, additionalInfo, payId, ip, payType);


        }

        public ActionResult AfterBank(int id)
        {
            BypassCertificateError();
            if (string.IsNullOrEmpty(Request.Params["RefId"]) || string.IsNullOrEmpty(Request.Params["SaleOrderId"]) || string.IsNullOrEmpty(Request.Params["SaleOrderId"]))
            {
                ViewBag.Error = "خطا رخ داده است";
                return View();
            }
            ViewBag.RefIdLabel = Request.Params["RefId"];
            ViewBag.ResCodeLabel = Request.Params["ResCode"];
            ViewBag.SaleOrderIdLabel = Request.Params["SaleOrderId"];
            ViewBag.SaleReferenceIdLabel = Request.Params["SaleOrderId"];
            int userId = 0;
            if (Request.Params["ResCode"] != "0")
            {
                ViewBag.Error = new Behpardakht().ResCodeFarsi(ViewBag.ResCodeLabel);
                return View();
            }
            else
            {
                long saleOrderId = long.Parse(ViewBag.SaleOrderIdLabel);
                long saleReferenceId = long.Parse(ViewBag.SaleReferenceIdLabel);
                var result = new Behpardakht().bpVerifyRequest(saleOrderId, saleReferenceId, ViewBag.RefIdLabel, userId);
                if(result.key =="0" || result.key == "43" || result.key == "45"){
                    // به روز رسانی رکورد پرداخت مربوطه
                    using (Models.ShopEntities1 db = new Models.ShopEntities1())
                    {
                        var pay = db.Payments.Where(p => p.Id == saleOrderId).FirstOrDefault();
                        pay.PayStatus = 1;//پرداخت موفق
                        db.SaveChanges();
                        
                    }
                    ViewBag.Success = result.Value; 

                }
                else
                {
                    using (Models.ShopEntities1 db = new Models.ShopEntities1())
                    {  // به روز رسانی رکورد پرداخت مربوطه
                        var pay = db.Payments.Where(p => p.Id == saleOrderId).FirstOrDefault();
                        pay.PayStatus = -1;//پرداخت ناموفق
                        db.SaveChanges();
                    }
                    ViewBag.Error = result.Value;

                }
               
            }

            return View();

        }

        private void BypassCertificateError()
        {
            ServicePointManager.ServerCertificateValidationCallback +=
               delegate (
                   Object sender1,
                   X509Certificate certificate,
                   X509Chain chain,
                   SslPolicyErrors sslPolicyErrors)
               {
                   return true;
               };
        }

    }
}