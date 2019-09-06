using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyMuseo.Models;
using System.Net.Mail;
using System.IO;
using MyMuseo.Gateway;

namespace MyMuseo.Controllers
{
    [Authorize]
    public class DashboardController : BaseController
    {
        // GET: Dashboard
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MessageCenter()
        {
            ViewBag.FollowingCount = _collectorRespository.GetFollowingCount(_collector.CollectorId);
            ViewBag.FollowersCount = _collectorRespository.GetFollowersCount(_collector.CollectorId);
            ViewBag.Messages = _collectorRespository.GetMessages(_collector.CollectorId);
            ViewBag.GetUserName = new Func<int, string>(GetCollectorName);
            List<Collectible> myCollectibles = _collectiblesRespository.GetCollectibles(_collector.CollectorId);
            ViewBag.ItemsList = myCollectibles;
            ViewBag.ItemsCount = myCollectibles.Count;
            ViewBag.GetCategoryName = new Func<int, string>(GetCategoryName);
            return View(_collector);
        }

        public ActionResult Notifications()
        {
            ViewBag.FollowingCount = _collectorRespository.GetFollowingCount(_collector.CollectorId);
            ViewBag.FollowersCount = _collectorRespository.GetFollowersCount(_collector.CollectorId);
            ViewBag.Messages = _collectorRespository.GetMessages(_collector.CollectorId);
            ViewBag.GetUserName = new Func<int, string>(GetCollectorName);
            List<int> followerIds = _collectorRespository.GetFollowerIds(_collector.CollectorId);
            List<Collector> followerList = new List<Collector>();
            foreach (int collectorId in followerIds)
            {
                var theCollector = _collectorRespository.GetCollector(collectorId);
                if (theCollector != null)
                {
                    followerList.Add(theCollector);
                }
            }
            ViewBag.FollowerList = followerList;
            List<int> followingIds = _collectorRespository.GetFollowingIds(_collector.CollectorId);
            List<Collector> followingList = new List<Collector>();
            foreach (int collectorId in followingIds)
            {
                var theCollector = _collectorRespository.GetCollector(collectorId);
                if (theCollector != null)
                {
                    followingList.Add(theCollector);
                }
            }
            ViewBag.FollowingList = followingList;
            List<Collectible> myCollectibles = _collectiblesRespository.GetCollectibles(_collector.CollectorId);
            ViewBag.ItemsList = myCollectibles;
            ViewBag.ItemsCount = myCollectibles.Count;
            ViewBag.GetCategoryName = new Func<int, string>(GetCategoryName);
            List<int> favoritesIds = _collectorRespository.GetFavoriteCollectorIds(_collector.CollectorId);
            List<Collector> favoritesList = new List<Collector>();
            foreach (int collectorId in favoritesIds)
            {
                favoritesList.Add(_collectorRespository.GetCollector(collectorId));
            }
            ViewBag.FavoritesList = favoritesList;
            return View(_collector);
        }

        public ActionResult Sales()
        {
            ViewBag.FollowingCount = _collectorRespository.GetFollowingCount(_collector.CollectorId);
            ViewBag.FollowersCount = _collectorRespository.GetFollowersCount(_collector.CollectorId);
            ViewBag.Messages = _collectorRespository.GetMessages(_collector.CollectorId);
            ViewBag.GetUserName = new Func<int, string>(GetCollectorName);
            List<Purchase> mySales = _collectorRespository.GetSalesByCollector(_collector.CollectorId);
            ViewBag.SalesList = mySales;
            List<Collectible> myCollectibles = _collectiblesRespository.GetCollectibles(_collector.CollectorId);
            ViewBag.ItemsList = myCollectibles;
            ViewBag.ItemsCount = myCollectibles.Count;
            ViewBag.GetCategoryName = new Func<int, string>(GetCategoryName);
            return View(_collector);
        }

        public ActionResult Purchases()
        {
            ViewBag.FollowingCount = _collectorRespository.GetFollowingCount(_collector.CollectorId);
            ViewBag.FollowersCount = _collectorRespository.GetFollowersCount(_collector.CollectorId);
            ViewBag.Messages = _collectorRespository.GetMessages(_collector.CollectorId);
            ViewBag.GetUserName = new Func<int, string>(GetCollectorName);
            List<Purchase> myPurchases = _collectorRespository.GetPurchasesByCollector(_collector.CollectorId);
            ViewBag.PurchasesList = myPurchases;
            List<Collectible> myCollectibles = _collectiblesRespository.GetCollectibles(_collector.CollectorId);
            ViewBag.ItemsList = myCollectibles;
            ViewBag.ItemsCount = myCollectibles.Count;
            ViewBag.GetCategoryName = new Func<int, string>(GetCategoryName);
            return View(_collector);
        }

        public ActionResult Following()
        {
            ViewBag.FollowingCount = _collectorRespository.GetFollowingCount(_collector.CollectorId);
            ViewBag.FollowersCount = _collectorRespository.GetFollowersCount(_collector.CollectorId);
            ViewBag.Messages = _collectorRespository.GetMessages(_collector.CollectorId);
            ViewBag.GetUserName = new Func<int, string>(GetCollectorName);
            List<int> followingIds = _collectorRespository.GetFollowingIds(_collector.CollectorId);
            List<Collector> followingList = new List<Collector>();
            foreach (int collectorId in followingIds)
            {
                var theCollector = _collectorRespository.GetCollector(collectorId);
                if (theCollector != null)
                {
                    followingList.Add(theCollector);
                }
            }
            ViewBag.FollowingList = followingList;
            List<Collectible> myCollectibles = _collectiblesRespository.GetCollectibles(_collector.CollectorId);
            ViewBag.ItemsList = myCollectibles;
            ViewBag.ItemsCount = myCollectibles.Count;
            ViewBag.GetCategoryName = new Func<int, string>(GetCategoryName);
            return View(_collector);
        }

        public ActionResult Followers()
        {
            ViewBag.FollowingCount = _collectorRespository.GetFollowingCount(_collector.CollectorId);
            ViewBag.FollowersCount = _collectorRespository.GetFollowersCount(_collector.CollectorId);
            ViewBag.Messages = _collectorRespository.GetMessages(_collector.CollectorId);
            ViewBag.GetUserName = new Func<int, string>(GetCollectorName);
            List<int> followerIds = _collectorRespository.GetFollowerIds(_collector.CollectorId);
            List<Collector> followerList = new List<Collector>();
            foreach (int collectorId in followerIds)
            {
                var theCollector = _collectorRespository.GetCollector(collectorId);
                if (theCollector != null)
                {
                    followerList.Add(theCollector);
                }
            }
            ViewBag.FollowerList = followerList;
            List<Collectible> myCollectibles = _collectiblesRespository.GetCollectibles(_collector.CollectorId);
            ViewBag.ItemsList = myCollectibles;
            ViewBag.ItemsCount = myCollectibles.Count;
            ViewBag.GetCategoryName = new Func<int, string>(GetCategoryName);
            return View(_collector);
        }

        public ActionResult MyItems()
        {
            ViewBag.FollowingCount = _collectorRespository.GetFollowingCount(_collector.CollectorId);
            ViewBag.FollowersCount = _collectorRespository.GetFollowersCount(_collector.CollectorId);
            ViewBag.Messages = _collectorRespository.GetMessages(_collector.CollectorId);
            ViewBag.GetUserName = new Func<int, string>(GetCollectorName);
            List<Collectible> myCollectibles = _collectiblesRespository.GetCollectibles(_collector.CollectorId);
            ViewBag.ItemsList = myCollectibles;
            ViewBag.ItemsCount = myCollectibles.Count;
            ViewBag.GetCategoryName = new Func<int, string>(GetCategoryName);
            return View(_collector);
        }

        public ActionResult Subscription()
        {
            ViewBag.FollowingCount = _collectorRespository.GetFollowingCount(_collector.CollectorId);
            ViewBag.FollowersCount = _collectorRespository.GetFollowersCount(_collector.CollectorId);
            ViewBag.Messages = _collectorRespository.GetMessages(_collector.CollectorId);
            ViewBag.GetUserName = new Func<int, string>(GetCollectorName);
            List<Collectible> myCollectibles = _collectiblesRespository.GetCollectibles(_collector.CollectorId);
            ViewBag.ItemsList = myCollectibles;
            ViewBag.ItemsCount = myCollectibles.Count;
            ViewBag.GetCategoryName = new Func<int, string>(GetCategoryName);
            return View(_collector);
        }

        public ActionResult Plans()
        {
            return View();
        }

        public ActionResult Payment()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Payment(FormCollection form)
        {
            var gateway = new PayeezyGateway(
                apiKey: "QP5wF8isH6yS4XB3qAbkDZ6tyvW9tFEH",
                apiSecret: "eb5adad2582b7d71724b9dacea9025bb85f5ce704cee247392fbe81332a583d6",
                token: "fdoa-dc32e6ae52409b8c5476bf5bf725fbaddc32e6ae52409b8c"
            );

            string cardHolderName = "Bubba Smith";
            string validVisa = "4111111111111111";
            //string validDiners = "36438936438936";
            //string validAmex = "373953192351004";
            //string invalidCard = "1234567812345678";
            //string invalidVisa = "4111111111111112";
            string month = "01";
            string twoDigitYear = "20";
            string dollarAmount1 = "1.23";
            //string dollarAmount2 = "3.21";
            //string dollarAmountDeclined = "5000.42";
            string cvv = "123";
            var referenceNumber = Guid.NewGuid().ToString();
            var response = gateway.CreditCardPurchase(validVisa, month, twoDigitYear, dollarAmount1, cardHolderName, cvv, referenceNumber);

            return View();
        }

        [HttpPost]
        public ActionResult ReplyMessage(FormCollection form)
        {
            int messageId = Convert.ToInt16(form[0]);
            int toCollectorId = Convert.ToInt16(form[1]);
            string replyMessage = form[2].ToString();
            Message messageModel = new Message();
            messageModel.MessageTopic = "Reply";
            messageModel.MessageText = replyMessage;
            messageModel.ToCollectorId = toCollectorId;
            messageModel.FromCollectorId = _collector.CollectorId;
            messageModel.ParentId = messageId;
            messageModel.CreatedDate = DateTime.Now;
            _collectorRespository.InsertMessage(messageModel);
            return Redirect("~/Dashboard/MessageCenter");
        }

        [HttpPost]
        public ActionResult DeleteMessage(FormCollection form)
        {
            int messageId = Convert.ToInt16(form[0]);
            _collectorRespository.DeleteMessage(messageId);
            return Redirect("~/Dashboard/MessageCenter");
        }

        [HttpPost]
        public ActionResult DeleteAllMessages(FormCollection form)
        {
            _collectorRespository.DeleteAllMessages(_collector.CollectorId);
            return Redirect("~/Dashboard/MessageCenter");
        }

        [HttpPost]
        public ActionResult CancelPurchase(FormCollection form)
        {
            int purchaseId = Convert.ToInt16(form[0]);
            //
            this.SendCancelOrderByBuyerEmail(purchaseId);
            _collectorRespository.DeletePurchase(purchaseId);
            //
            return Redirect("~/Dashboard/Purchases");
        }

        [HttpPost]
        public ActionResult CancelSales(FormCollection form)
        {
            int purchaseId = Convert.ToInt16(form[0]);
            //
            this.SendCancelOrderBySellerEmail(purchaseId);
            _collectorRespository.DeletePurchase(purchaseId);
            //
            return Redirect("~/Dashboard/Sales");
        }

        public void SendCancelOrderByBuyerEmail(int purchaseId)
        {
            Purchase purchaseItem = _collectorRespository.GetPurchase(purchaseId);
            Collectible model = _collectiblesRespository.GetCollectible(purchaseItem.CollectibleId);
            string buyerName = this.GetCollectorName(purchaseItem.CustomerId);
            string buyerEmail = this.GetCollectorEmail(purchaseItem.CustomerId);
            string sellerName = this.GetCollectorName(purchaseItem.CollectorId);
            string sellerEmail = this.GetCollectorEmail(purchaseItem.CollectorId);
            // Send email to seller
            var callbackUrl = Url.Action("CollectorDetail", "Home", new { id = purchaseItem.CustomerId }, protocol: Request.Url.Scheme);
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("no-reply@mymuseo.com");
            mailMessage.To.Add(new MailAddress(sellerEmail));
            mailMessage.CC.Add(_systemMails);
            mailMessage.Subject = "Order Cancelled";
            mailMessage.IsBodyHtml = true;
            TemplateModel content = _collectorRespository.GetTemplateByName("Order cancelled by buyer");
            string mailTemplate = content.TemplateText;
            string theMessage = String.Format(mailTemplate, this.GetEmailImage0(), this.GetEmailImage1(), sellerName, buyerName, model.Title, buyerName, callbackUrl);
            mailMessage.Body = theMessage;
            this.SendMail(mailMessage);
        }

        public void SendCancelOrderBySellerEmail(int purchaseId)
        {
            Purchase purchaseItem = _collectorRespository.GetPurchase(purchaseId);
            Collectible model = _collectiblesRespository.GetCollectible(purchaseItem.CollectibleId);
            string buyerName = this.GetCollectorName(purchaseItem.CustomerId);
            string buyerEmail = this.GetCollectorEmail(purchaseItem.CustomerId);
            string sellerName = this.GetCollectorName(purchaseItem.CollectorId);
            string sellerEmail = this.GetCollectorEmail(purchaseItem.CollectorId);
            // Send email to Buyer
            var callbackUrl = Url.Action("CollectorDetail", "Home", new { id = purchaseItem.CollectorId }, protocol: Request.Url.Scheme);
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("no-reply@mymuseo.com");
            mailMessage.To.Add(new MailAddress(sellerEmail));
            mailMessage.CC.Add(_systemMails);
            mailMessage.Subject = "Order Cancelled";
            mailMessage.IsBodyHtml = true;
            TemplateModel content = _collectorRespository.GetTemplateByName("Order cancelled by Seller");
            string mailTemplate = content.TemplateText;
            string theMessage = String.Format(mailTemplate, this.GetEmailImage0(), this.GetEmailImage1(), buyerName, sellerName, model.Title, sellerName, callbackUrl);
            mailMessage.Body = theMessage;
            this.SendMail(mailMessage);
        }

        [HttpPost]
        public ActionResult DeleteFollowing(FormCollection form)
        {
            int followingId = Convert.ToInt16(form[0]);
            _collectorRespository.DeleteFollowing(_collector.CollectorId, followingId);           
            return Redirect("~/Dashboard/Following");
        }

        [HttpPost]
        public ActionResult DeleteFollower(FormCollection form)
        {
            int followerId = Convert.ToInt16(form[0]);
            _collectorRespository.DeleteFollower(_collector.CollectorId, followerId);
            return Redirect("~/Dashboard/Followers");
        }

    }
}