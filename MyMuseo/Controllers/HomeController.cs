using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyMuseo.Models;
using MyMuseo.DataService;
using Microsoft.AspNet.Identity;
using System.Text;
using System.Net.Mail;
using System.IO;

namespace MyMuseo.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController() : base()
        {
        }

        public ActionResult Index()
        {
            if (Session["StartDateTime"] == null)
            {
                Session["StartDateTime"] = DateTime.Now;
            }
            CollectorRespository collectorRespository = new CollectorRespository();
            ViewBag.NewCollectors = collectorRespository.GetProfileCollectors(10000, "DESC");
            ViewBag.NewGroups = collectorRespository.GetGroups();
            CollectionsRespository collectionRespository = new CollectionsRespository();
            ViewBag.NewCollections = collectionRespository.GetAllCollectionsActive(10000, "DESC").Where(x => x.IsDraft == false);
            CollectiblesRespository collectibleRespository = new CollectiblesRespository();

            List<Collectible> newCollectibles = new List<Collectible>();
            foreach( Collectible item in collectibleRespository.GetAllCollectibles(10000, "DESC"))
            {
                if (String.IsNullOrEmpty(item.ThumbImage))
                {
                    item.ThumbImage = "/Content/images/place-holder.png";
                }
                newCollectibles.Add(item);
            }
            ViewBag.NewCollectibles = newCollectibles;

            if (Request.IsAuthenticated)
            {
                Session["FullName"] = GetCurrentCollectorName();
                Session["CurrentCollectorId"] = GetCurrentCollectorId();
            }
            else
            {
                Session["FullName"] = "";
                Session["CurrentCollectorId"] = 0;
            }
            ViewBag.GetUserName = new Func<int, string>(GetCollectorName);
            return View();
        }

        public ActionResult About()
        {
            CollectorRespository collectorRepository = new CollectorRespository();
            ContentModel content = collectorRepository.GetContentByName("About Us");
            if (content != null)
            {
                ViewBag.ContentText = content.ContentText;
            }
            else
            {
                ViewBag.ContentText = "";
            }
            return View();
        }

        public ActionResult Contact()
        {
            Session["CAPTCHA"] = this.GetRandomText();
            ViewBag.CaptchaError = "";
            return View();
        }

        [HttpPost]
        public ActionResult Contact(FormCollection form)
        {
            string clientCaptcha = form["clientCaptcha"];
            string serverCaptcha = Session["CAPTCHA"].ToString();
            if (!clientCaptcha.Equals(serverCaptcha))
            {
                ViewBag.CaptchaError = "Sorry, please write exact text as written above.";
                return View();
            }

            string fromEmail = form[0].ToString();
            string subjectText = form[1].ToString();
            string messageText = form[2].ToString();
            string fromName = form[3].ToString();
            // TODO
            CollectorRespository collectorRepository = new CollectorRespository();
            Message contactMessage = new Message();
            contactMessage.ToCollectorId = 0;
            if (Request.IsAuthenticated)
            {
                string userId = System.Web.HttpContext.Current.User.Identity.GetUserId();
                contactMessage.FromCollectorId = collectorRepository.GetCollector(userId).CollectorId;
            }
            else
            {
                contactMessage.FromCollectorId = -1;
            }
            contactMessage.MessageTopic = subjectText + " - " + fromName + " - " + fromEmail;
            contactMessage.MessageText = messageText;
            contactMessage.CreatedDate = DateTime.Now;
            collectorRepository.InsertMessage(contactMessage);
            ViewBag.Message = "Thanks " + fromName;
            // Send to mymuseo info
            try
            {
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress("postmaster@mymuseo.com");
                mailMessage.To.Add(new MailAddress("info@mymuseo.com"));
                mailMessage.CC.Add(_systemMails);
                mailMessage.Subject = contactMessage.MessageTopic;
                mailMessage.IsBodyHtml = false;
                mailMessage.Body = messageText;
                this.SendMail(mailMessage);
            }
            catch (Exception e)
            {
                string err = e.Message;
            }
            //
            // Send to user
            MailMessage mailMessage2 = new MailMessage();
            mailMessage2.From = new MailAddress("no-reply@mymuseo.com");
            mailMessage2.To.Add(new MailAddress(fromEmail));
            mailMessage2.CC.Add(_systemMails);
            mailMessage2.Subject = "Contact";
            mailMessage2.IsBodyHtml = true;
            TemplateModel content2 = _collectorRespository.GetTemplateByName("Info @ myMuseo Auto Responder");
            string mailTemplate2 = content2.TemplateText;
            string theMessage2 = String.Format(mailTemplate2, this.GetEmailImage0(), this.GetEmailImage1(), fromName);
            mailMessage2.Body = theMessage2;
            this.SendMail(mailMessage2);
            //
            return View();
        }

        public ActionResult Faqs()
        {
            CollectorRespository collectorRepository = new CollectorRespository();
            ContentModel content = collectorRepository.GetContentByName("FAQs");
            if (content != null)
            {
                ViewBag.ContentText = content.ContentText;
            }
            else
            {
                ViewBag.ContentText = "";
            }
            return View();
        }

        public ActionResult Terms()
        {
            CollectorRespository collectorRepository = new CollectorRespository();
            ContentModel content = collectorRepository.GetContentByName("Terms of Use");
            if (content != null)
            {
                ViewBag.ContentText = content.ContentText;
            }
            else
            {
                ViewBag.ContentText = "";
            }
            return View();
        }

        public ActionResult CollectingInformation()
        {
            return View();
        }

        [Authorize]
        public new ActionResult Profile()
        {
            CollectorRespository collectorRespository = new CollectorRespository();
            string userId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            Collector collector = collectorRespository.GetCollector(userId);
            ViewBag.Message = collector.FirstName;
            ViewBag.ProfileImage = collector.ProfileImage;
            ViewBag.FeaturedImage = collector.PageImage;
            ViewBag.CollectiblesCount = collectorRespository.GetCollectiblesCount(collector.CollectorId);
            ViewBag.CollectionsCount = collectorRespository.GetCollectionsCount(collector.CollectorId);
            ViewBag.FollowersCount = collectorRespository.GetFollowersCount(collector.CollectorId);
            ViewBag.FollowingCount = collectorRespository.GetFollowingCount(collector.CollectorId);
            ViewBag.FavoritesCount = collectorRespository.GetFavoritesCount(collector.CollectorId);
            if (collector.WebSites != null)
            {
                String[] sites = collector.WebSites.Split(',');
                StringBuilder sb = new StringBuilder();
                foreach (String site in sites)
                {
                    String[] info = site.Split(':');
                    sb.Append("<a href='http://").Append(info[1]).Append("'>").Append(info[0]).Append(" target='_blank'</a><BR />");
                }
                ViewBag.Sites = sb.ToString();
            }
            return View(collector);
        }

        [HttpGet]
        public JsonResult CollectibleLookup()
        {
            CollectiblesRespository respository = new CollectiblesRespository();
            var list = respository.GetAllCollectibles(50000, "");
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CollectorDetail(int id)
        {
            Session["ViewCollection"] = 0;
            if (Request.Cookies["ViewedPage"] != null)
            {
                if (Request.Cookies["ViewedPage"][string.Format("proId_{0}", id)] == null)
                {
                    HttpCookie cookie = (HttpCookie)Request.Cookies["ViewedPage"];
                    cookie[string.Format("proId_{0}", id)] = "1";
                    cookie.Expires = DateTime.Now.AddHours(2);
                    Response.Cookies.Add(cookie);
                    this.InsertCollectorViewLog(id);
                }
            }
            else
            {
                HttpCookie cookie = new HttpCookie("ViewedPage");
                cookie[string.Format("proId_{0}", id)] = "1";
                cookie.Expires = DateTime.Now.AddHours(2);
                Response.Cookies.Add(cookie);
                this.InsertCollectorViewLog(id);
            }
            ViewBag.IsMe = false;
            CollectorRespository collectorRespository = new CollectorRespository();
            ViewBag.CanFavorite = false;
            ViewBag.CanFollow = false;
            if (id == 0)
            {
                if (Request.IsAuthenticated)
                {
                    string userId = System.Web.HttpContext.Current.User.Identity.GetUserId();
                    id = collectorRespository.GetCollector(userId).CollectorId;
                    Session["FullName"] = GetCurrentCollectorName();
                    ViewBag.IsMe = true;
                    // Redirect to Admin Dashboard
                    if (Session["IsAdmin"].ToString() == "True")
                    {
                        return Redirect("~/Admin/Index");
                    }
                }
            }
            else
            {
                if (Request.IsAuthenticated)
                {
                    string userId = System.Web.HttpContext.Current.User.Identity.GetUserId();
                    if (collectorRespository.GetCollector(userId).CollectorId == id)
                    {
                        ViewBag.IsMe = true;
                    }
                    else
                    {
                        ViewBag.CanFavorite = this.UserCanFavoriteCollector(id);
                        ViewBag.CanFollow = this.UserCanFollowCollector(id);
                    }
                }
            }
            Collector collector = collectorRespository.GetCollector(id);
            ViewBag.Message = collector.FirstName;
            ViewBag.ProfileImage = collector.ProfileImage;
            ViewBag.FeaturedImage = collector.PageImage;
            ViewBag.CollectiblesCount = collectorRespository.GetCollectiblesCount(collector.CollectorId);
            //ViewBag.CollectionsCount = collectorRespository.GetCollectionsCount(collector.CollectorId);
            ViewBag.FollowersCount = collectorRespository.GetFollowersCount(collector.CollectorId);
            ViewBag.FollowingCount = collectorRespository.GetFollowingCount(collector.CollectorId);

            if (collector.AreasOfInterest != null)
            {
                String[] areas = collector.AreasOfInterest.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                ViewBag.AreasOfInterest = String.Join("<br/>", areas);
            }
            if (collector.WebSites != null && collector.WebSites.Contains(':'))
            {
                String[] sites = collector.WebSites.Split(',');
                StringBuilder sb = new StringBuilder();
                foreach (String site in sites)
                {
                    String[] info = site.Split(':');
                    sb.Append("<a href='http://").Append(info[1]).Append("'  target='_blank' >").Append(info[0]).Append("</a><BR />");
                }
                ViewBag.Sites = sb.ToString();
            }

            AddressRepository addressRepository = new AddressRepository();
            AddressInfo collectorAddress = addressRepository.GetCollectorAddress(collector.CollectorId);
            if (collectorAddress != null)
            {
                ViewBag.Location = collectorAddress.City;
            }
            else
            {
                ViewBag.Location = "";
            }
            // TODO
            CollectiblesRespository collectiblesRespository = new CollectiblesRespository();
            List<Collectible> myCollectibles = collectiblesRespository.GetCollectibles(collector.CollectorId);
            List<String> collectionThumbs = new List<String>();
            List<String> collectionLinks = new List<String>();
            for (int i = 0; i < myCollectibles.Count; i++)
            {
                if (i > 8)
                {
                    break;
                }
                collectionThumbs.Add(myCollectibles[i].ThumbImage);
                collectionLinks.Add("/Home/CollectibleDetail/" + myCollectibles[i].CollectibleId);
            }
            if (collectionThumbs.Count < 9)
            {
                for (int j = collectionThumbs.Count; j < 9; j++)
                {
                    collectionThumbs.Add("/Content/images/place-holder.png");
                    collectionLinks.Add("");
                }
            }
            ViewBag.CollectionThumbs = collectionThumbs;
            ViewBag.CollectionLinks = collectionLinks;

            List<int> followerIds = collectorRespository.GetFollowerIds(id);
            List<Collector> followerList = new List<Collector>();
            foreach (int collectorId in followerIds)
            {
                followerList.Add(collectorRespository.GetCollector(collectorId));
            }
            ViewBag.FollowerList = followerList;

            if (followerList.Count > 0)
            {
                ViewBag.FollowersTips = "<p>";
                foreach (Collector item in ViewBag.FollowerList)
                {
                    ViewBag.FollowersTips += item.FirstName + ", " + item.LastName + "<br/>";
                }
                ViewBag.FollowersTips += "</p>";
            }
            else
            {
                ViewBag.FollowersTips = "";
            }

            List<int> followingIds = collectorRespository.GetFollowingIds(id);
            List<Collector> followingList = new List<Collector>();
            foreach (int collectorId in followingIds)
            {
                followingList.Add(collectorRespository.GetCollector(collectorId));
            }
            ViewBag.FollowingList = followingList;

            if (followingList.Count > 0)
            {
                ViewBag.FollowingTips = "<p>";
                foreach (Collector item in ViewBag.FollowingList)
                {
                    ViewBag.FollowingTips += item.FirstName + ", " + item.LastName + "<br/>";
                }
                ViewBag.FollowingTips += "</p>";
            }
            else
            {
                ViewBag.FollowingTips = "";
            }

            List<int> favoritesIds = collectorRespository.GetFavoriteCollectorIds(id);
            List<Collector> favoritesList = new List<Collector>();
            foreach (int collectorId in favoritesIds)
            {
                favoritesList.Add(collectorRespository.GetCollector(collectorId));
            }
            ViewBag.FavoritesList = favoritesList;
            ViewBag.FavoritesCount = favoritesList.Count;
            if (collector.FeaturedItemId != 0)
            {
                Collectible featuredItem = collectiblesRespository.GetCollectible(collector.FeaturedItemId);
                if (featuredItem != null)
                {
                    ViewBag.FeaturedImage = featuredItem.NormalImage;
                }
            }

            //if (Request.IsAuthenticated)
            //{
            //    ViewBag.SideComments = this.GetSideComments();
            //}

            if ((bool)ViewBag.IsMe == true)
            {
                ViewBag.CollectionsCount = _collectorRespository.GetCollectionsCount(collector.CollectorId);
            }
            else
            {
                ViewBag.CollectionsCount = _collectorRespository.GetActiveCollectionsCount(collector.CollectorId);
            }

            return View(collector);
        }

        public ActionResult CollectionDetail(int id)
        {
            if (Request.Cookies["ViewedPage"] != null)
            {
                if (Request.Cookies["ViewedPage"][string.Format("colId_{0}", id)] == null)
                {
                    HttpCookie cookie = (HttpCookie)Request.Cookies["ViewedPage"];
                    cookie[string.Format("colId_{0}", id)] = "1";
                    cookie.Expires = DateTime.Now.AddHours(2);
                    Response.Cookies.Add(cookie);
                    this.InsertCollectionViewLog(id);
                }
            }
            else
            {
                HttpCookie cookie = new HttpCookie("ViewedPage");
                cookie[string.Format("colId_{0}", id)] = "1";
                cookie.Expires = DateTime.Now.AddHours(2);
                Response.Cookies.Add(cookie);
                this.InsertCollectionViewLog(id);
            }
            Session["ViewCollection"] = id;
            ViewBag.IsMe = false;
            CollectionsRespository collectionsRespository = new CollectionsRespository();
            Collection collection = collectionsRespository.GetCollection(id);
            ViewBag.CanFavorite = false;
            if (Request.IsAuthenticated)
            {
                string userId = System.Web.HttpContext.Current.User.Identity.GetUserId();
                CollectorRespository collectorRespository = new CollectorRespository();
                if (collectorRespository.GetCollector(userId).CollectorId == collection.CollectorId)
                {
                    ViewBag.IsMe = true;
                    ViewBag.CanFavorite = false;
                }
                else
                {
                    ViewBag.CanFavorite = this.UserCanFavoriteCollection(id);
                }
            }
            CommentsRespository commentsRespository = new CommentsRespository();
            List<Comment> commentList = commentsRespository.GetCollectionComments(id);
            ViewBag.CommentList = commentList;
            int commentCount = 0;
            foreach(Comment comment in commentList)
            {
                if (comment.FlagAsAbuse == false)
                {
                    commentCount++;
                }
            }
            ViewBag.CommentCount = commentCount;
            ViewBag.GetUserName = new Func<int, string>(GetCollectorName);
            ViewBag.GetItemSource = new Func<int, string>(GetItemSource);

            ViewBag.FeaturedItemId = 0;
            CollectiblesRespository collectiblesRespository = new CollectiblesRespository();
            List<Collectible> myCollectibles = collectiblesRespository.GetCollectiblesOfCollection(id);  
            ViewBag.MyCollectibles = myCollectibles;
            if (collection.FeaturedItemId > 0)
            {
                Collectible featuredItem = collectiblesRespository.GetCollectible(collection.FeaturedItemId);
                if (featuredItem != null)
                {
                    ViewBag.ItemTitle = featuredItem.Title;
                    ViewBag.ArtistId = featuredItem.ArtistId;
                    ViewBag.ArtistName = featuredItem.ArtistName;
                    ViewBag.Medium = featuredItem.Medium;
                    ViewBag.Width = featuredItem.Width;
                    ViewBag.Height = featuredItem.Height;
                    ViewBag.ItemDate = featuredItem.ItemDate;
                    ViewBag.NotForSale = featuredItem.NotForSale;
                    ViewBag.Price = featuredItem.Price;
                    ViewBag.AllowOffer = featuredItem.AllowOffers;
                    ViewBag.NoItem = false;
                    ViewBag.FeaturedItemId = featuredItem.CollectibleId;
                    collection.NormalImage = featuredItem.NormalImage;
                }
                else
                {
                    ViewBag.NoItem = true;
                    ViewBag.NotForSale = true;
                }
            }
            else if (myCollectibles.Count > 0)
            {
                Collectible item = myCollectibles[0];
                ViewBag.ItemTitle = item.Title;
                ViewBag.ArtistId = item.ArtistId;
                ViewBag.ArtistName = item.ArtistName;
                ViewBag.Medium = item.Medium;
                ViewBag.Width = item.Width;
                ViewBag.Height = item.Height;
                ViewBag.ItemDate = item.ItemDate;
                ViewBag.NotForSale = item.NotForSale;
                ViewBag.Price = item.Price;
                ViewBag.AllowOffer = item.AllowOffers;
                ViewBag.NoItem = false;
                ViewBag.FeaturedItemId = item.CollectibleId;
                collection.NormalImage = item.NormalImage;
            }
            else
            {
                ViewBag.NoItem = true;
                ViewBag.NotForSale = true;
            }

            ViewBag.FavoritesList = _collectorRespository.GetFavoriteCollectorsForACollection(id);
            ViewBag.FavoriteCounts = ViewBag.FavoritesList.Count;
            ViewBag.ViewCounts = _collectorRespository.GetViewsCountForACollection(id);
            ViewBag.FavoritesTips = "<p>";
            foreach (Collector item in ViewBag.FavoritesList)
            {
                ViewBag.FavoritesTips += item.FirstName + ", " + item.LastName + "<br/>";
            }
            ViewBag.FavoritesTips += "</p>";
            return View(collection);
        }


        [HttpPost]
        public ActionResult CollectionDetail(FormCollection form)
        {
            CollectionsRespository collectionsRespository = new CollectionsRespository();
            CommentsRespository commentsRespository = new CommentsRespository();
            int collectionId = Convert.ToInt32(form[0]);
            string comments = form[1];
            int commentId = 0;
            if (form[2].ToString() != String.Empty)
            {
                commentId = Convert.ToInt32(form[2]);
            }
            Comment commentModel = new Comment();
            commentModel.CollectionId = collectionId;
            commentModel.CollectibleId = 0;
            commentModel.PostByCollectorId = this.GetCurrentCollectorId();
            commentModel.ParentId = commentId;
            commentModel.CommentText = comments;
            commentModel.CreatedDate = DateTime.Now;
            commentsRespository.InsertComment(commentModel);
            Collection model = collectionsRespository.GetCollection(collectionId);
            List<Comment> commentList = commentsRespository.GetCollectionComments(collectionId);
            ViewBag.CommentList = commentList;
            ViewBag.GetUserName = new Func<int, string>(GetCollectorName);
            //return View(model);
            // TODO
            return RedirectToAction("CollectionDetail", "Home", new { id = collectionId });
        }

        public ActionResult Purchase(int id)
        {
            CollectiblesRespository collectiblesRespository = new CollectiblesRespository();
            Collectible model = collectiblesRespository.GetCollectible(id);
            ViewBag.GetUserName = new Func<int, string>(GetCollectorName);
            string buyerName = this.GetCollectorName(_collector.CollectorId);
            string buyerEmail = this.GetCollectorEmail(_collector.CollectorId);
            string sellerName = this.GetCollectorName(model.CollectorId);
            string sellerEmail = this.GetCollectorEmail(model.CollectorId);
            //
            var callbackUrl = Url.Action("CollectibleDetail", "Home", new { id = model.CollectibleId }, protocol: Request.Url.Scheme);
            // Send to buyer
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("no-reply@mymuseo.com");
            mailMessage.To.Add(new MailAddress(buyerEmail));
            mailMessage.CC.Add(_systemMails);
            mailMessage.Subject = "Purchase Confirmation";
            mailMessage.IsBodyHtml = true;
            TemplateModel content = _collectorRespository.GetTemplateByName("Email to buyer when an item is purchased");
            string mailTemplate = content.TemplateText;
            string theMessage = String.Format(mailTemplate, this.GetEmailImage0(), this.GetEmailImage1(), buyerName, sellerName, callbackUrl, model.Title, sellerName);
            mailMessage.Body = theMessage;
            this.SendMail(mailMessage);
            //
            // Send to seller
            MailMessage mailMessage2 = new MailMessage();
            mailMessage2.From = new MailAddress("no-reply@mymuseo.com");
            mailMessage2.To.Add(new MailAddress(sellerEmail));
            mailMessage2.CC.Add(_systemMails);
            mailMessage2.Subject = "Purchase Confirmation";
            mailMessage2.IsBodyHtml = true;
            TemplateModel content2 = _collectorRespository.GetTemplateByName("Email to seller when an item sells");
            string mailTemplate2 = content2.TemplateText;
            string theMessage2 = String.Format(mailTemplate2, this.GetEmailImage0(), this.GetEmailImage1(), sellerName, buyerName, callbackUrl, model.Title, buyerName);
            mailMessage2.Body = theMessage2;
            this.SendMail(mailMessage2);
            //
            return View(model);
        }

        public ActionResult Offer(int id)
        {
            Purchase purchaseModel = _collectorRespository.GetPurchaseById(id);
            ViewBag.OfferAmount = purchaseModel.Price;
            CollectiblesRespository collectiblesRespository = new CollectiblesRespository();
            Collectible model = collectiblesRespository.GetCollectible(purchaseModel.CollectibleId);
            ViewBag.GetUserName = new Func<int, string>(GetCollectorName);
            return View(model);
        }

        public ActionResult CollectibleDetail(int id)
        {
            CollectorRespository collectorRepository = new CollectorRespository();
            if (Request.Cookies["ViewedPage"] != null)
            {
                if (Request.Cookies["ViewedPage"][string.Format("cId_{0}", id)] == null)
                {
                    HttpCookie cookie = (HttpCookie)Request.Cookies["ViewedPage"];
                    cookie[string.Format("cId_{0}", id)] = "1";
                    cookie.Expires = DateTime.Now.AddDays(1);
                    Response.Cookies.Add(cookie);
                    this.InsertCollectibeViewLog(id);
                }
            }
            else
            {
                HttpCookie cookie = new HttpCookie("ViewedPage");
                cookie[string.Format("cId_{0}", id)] = "1";
                cookie.Expires = DateTime.Now.AddDays(1);
                Response.Cookies.Add(cookie);
                this.InsertCollectibeViewLog(id);
            }
            CollectiblesRespository collectiblesRespository = new CollectiblesRespository();
            Collectible model = collectiblesRespository.GetCollectible(id);
            if (String.IsNullOrEmpty(model.OriginalImage))
            {
                model.OriginalImage = model.NormalImage;
            }
            CommentsRespository commentsRespository = new CommentsRespository();
            List<Comment> commentList = commentsRespository.GetCollectibleComments(model.CollectibleId);
            ViewBag.CommentList = commentList;
            int commentCount = 0;
            foreach (Comment comment in commentList)
            {
                if (comment.FlagAsAbuse == false)
                {
                    commentCount++;
                }
            }
            ViewBag.CommentCount = commentCount;
            ViewBag.GetUserName = new Func<int, string>(GetCollectorName);
            ViewBag.GetItemSource = new Func<int, string>(GetItemSource);
            ViewBag.FavoriteCounts = collectorRepository.GetFavoritesCountForACollectible(id);
            ViewBag.ViewCounts = collectorRepository.GetViewsCountForACollectible(id);
            ViewBag.IsMe = false;
            CollectionsRespository collectionsRespository = new CollectionsRespository();
            ViewBag.CanFavorite = false;
            if (Request.IsAuthenticated)
            {
                string userId = System.Web.HttpContext.Current.User.Identity.GetUserId();
                CollectorRespository collectorRespository = new CollectorRespository();
                if (collectorRespository.GetCollector(userId).CollectorId == model.CollectorId)
                {
                    ViewBag.IsMe = true;
                    ViewBag.CanFavorite = false;
                }
                else
                {
                    ViewBag.CanFavorite = this.UserCanFavoriteItem(id);
                }
            }
            List<Collectible> myCollectibles = new List<Collectible>();
            if (Session["ViewCollection"] != null)
            {
                int collectionId = Convert.ToInt32(Session["ViewCollection"].ToString());
                if (collectionId == 0)
                {
                    myCollectibles = collectiblesRespository.GetCollectibles(model.CollectorId);
                }
                else
                {
                    myCollectibles = collectiblesRespository.GetCollectiblesOfCollection(collectionId);
                }
            }
            else
            {
                myCollectibles = collectiblesRespository.GetCollectibles(model.CollectorId);
            }

            foreach(Collectible item in myCollectibles)
            {
                if (String.IsNullOrEmpty(item.ThumbImage))
                {
                    item.ThumbImage = "/Content/images/place-holder.png";
                }
            }

            ViewBag.MyCollectibles = myCollectibles;

            ViewBag.IsPending = false;
            ViewBag.IsSold = false;
            Purchase purchaseItem = collectorRepository.GetPurchaseByCollectible(model.CollectibleId);
            if (purchaseItem != null)
            {
                if (String.IsNullOrEmpty(purchaseItem.TrackingNumber))
                {
                    ViewBag.IsPending = true;
                }
                else
                {
                    ViewBag.IsSold = true;
                }
            }

            // Check offers
            List<Purchase> purchaseList = collectorRepository.GetPurchasesByCollectible(model.CollectibleId);
            ViewBag.OffersCount = purchaseList.Count((item) => { return item.TypeId == 2; });

            if (String.IsNullOrEmpty(model.OriginalImage))
            {
                model.OriginalImage = "/Content/images/place-holder.png";
            }

            if (String.IsNullOrEmpty(model.NormalImage))
            {
                model.NormalImage = "/Content/images/place-holder.png";
            }

            if (String.IsNullOrEmpty(model.ThumbImage))
            {
                model.ThumbImage = "/Content/images/place-holder.png";
            }

            ViewBag.FavoritesList = collectorRepository.GetFavoriteCollectorsForACollectible(id);

            ViewBag.FavoritesTips = "<p>";
            foreach (Collector item in ViewBag.FavoritesList)
            {
                ViewBag.FavoritesTips += item.FirstName + ", " + item.LastName + "<br/>";
            }
            ViewBag.FavoritesTips += "</p>";

            return View(model);
        }

        [HttpPost]
        public ActionResult CollectibleDetail(FormCollection form)
        {
            CollectiblesRespository collectiblesRespository = new CollectiblesRespository();
            CommentsRespository commentsRespository = new CommentsRespository();
            int collectibleId = Convert.ToInt32(form[0]);
            string comments = form[1];
            int commentId = 0;
            if (form[2].ToString() != String.Empty)
            {
                commentId = Convert.ToInt32(form[2]);
            }
            Comment commentModel = new Comment();
            commentModel.CollectionId = 0;
            commentModel.CollectibleId = collectibleId;
            commentModel.PostByCollectorId = this.GetCurrentCollectorId();
            commentModel.ParentId = commentId;
            commentModel.CommentText = comments;
            commentModel.CreatedDate = DateTime.Now;
            commentsRespository.InsertComment(commentModel);
        
            return RedirectToAction("CollectibleDetail", "Home", new { id = collectibleId });
        }

        [HttpPost]
        public ActionResult FlagAsAbuseCollection(FormCollection form)
        {
            int collectionId = Convert.ToInt32(form[0]);
            int commentId = 0;
            if (form[1].ToString() != String.Empty)
            {
                commentId = Convert.ToInt32(form[1]);
            }
            CommentsRespository commentsRespository = new CommentsRespository();
            commentsRespository.FlagComment(commentId);
            return Redirect("~/Home/CollectionDetail/" + collectionId);
        }

        [HttpPost]
        public ActionResult FlagAsAbuseCollectible(FormCollection form)
        {
            int collectibleId = Convert.ToInt32(form[0]);
            int commentId = 0;
            if (form[1].ToString() != String.Empty)
            {
                commentId = Convert.ToInt32(form[1]);
            }
            CommentsRespository commentsRespository = new CommentsRespository();
            commentsRespository.FlagComment(commentId);
            return Redirect("~/Home/CollectibleDetail/" + collectibleId);
        }

        [HttpPost]
        public ActionResult FlagAsAbuseCollector(FormCollection form)
        {
            // TODO
            int collectorId = Convert.ToInt32(form[0]);
            CollectorRespository collectorRepository = new CollectorRespository();
            int fromCollectorId = 0;
            if (Request.IsAuthenticated)
            {
                string userId = System.Web.HttpContext.Current.User.Identity.GetUserId();
                fromCollectorId = collectorRepository.GetCollector(userId).CollectorId;
            }
            Message message = new Message();
            message.MessageTopic = "Report Abuse - " + GetCollectorName(collectorId);
            message.FromCollectorId = fromCollectorId;
            message.ToCollectorId = 0;
            message.MessageText = form[1].ToString();
            message.ParentId = 0;
            message.CreatedDate = DateTime.Now;
            collectorRepository.InsertMessage(message);
            //
            //
            var linkUrl = Url.Action("CollectorDetail", "Home", new { id = collectorId }, protocol: Request.Url.Scheme);
            string reportEmail = this.GetCollectorEmail(_collector.CollectorId);
            string reportName = this.GetCollectorName(_collector.CollectorId);
            string abuseName = this.GetCollectorName(collectorId);
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("no-reply@mymuseo.com");
            mailMessage.To.Add(new MailAddress(reportEmail));
            mailMessage.CC.Add(new MailAddress("tsonla@yahoo.com"));
            mailMessage.CC.Add(new MailAddress("gjamroski@mac.com"));
            mailMessage.Subject = "Report Abuse";
            mailMessage.IsBodyHtml = true;
            var imagePath0 = Path.Combine(Server.MapPath("~/Content/images"), "myMuseo.png");
            String image0 = Convert.ToBase64String(System.IO.File.ReadAllBytes(imagePath0));
            var imagePath1 = Path.Combine(Server.MapPath("~/Content/images"), "mymuseo-img.jpg");
            String image1 = Convert.ToBase64String(System.IO.File.ReadAllBytes(imagePath1));
            TemplateModel content = _collectorRespository.GetTemplateByName("Flagged as abuse reply");
            string mailTemplate = content.TemplateText;
            string theMessage = String.Format(mailTemplate, image0, image1, reportName, linkUrl, abuseName);
            mailMessage.Body = theMessage;
            this.SendMail(mailMessage);
            //
            return Redirect("~/Home/CollectorDetail/" + collectorId);
        }

        [HttpPost]
        public ActionResult ContactMe(FormCollection form)
        {
            CollectorRespository collectorRepository = new CollectorRespository();
            int fromCollectorId = 0;
            if (Request.IsAuthenticated)
            {
                string userId = System.Web.HttpContext.Current.User.Identity.GetUserId();
                fromCollectorId = collectorRepository.GetCollector(userId).CollectorId;
            }
            Message message = new Message();
            message.MessageTopic = "Contact";
            message.FromCollectorId = fromCollectorId;
            message.ToCollectorId = Convert.ToInt32(form[0]);
            message.MessageText = form[1].ToString();
            message.ParentId = 0;
            message.CreatedDate = DateTime.Now;
            collectorRepository.InsertMessage(message);
            //
            var linkUrl = Url.Action("MessageCenter", "Dashboard", null, protocol: Request.Url.Scheme);
            string toEmail = this.GetCollectorEmail(message.ToCollectorId);
            string toName = this.GetCollectorName(message.ToCollectorId);
            string fromName = this.GetCollectorName(fromCollectorId);
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("no-reply@mymuseo.com");
            mailMessage.To.Add(new MailAddress(toEmail));
            mailMessage.CC.Add(new MailAddress("tsonla@yahoo.com"));
            mailMessage.CC.Add(new MailAddress("gjamroski@mac.com"));
            mailMessage.Subject = "New Message";
            mailMessage.IsBodyHtml = true;
            var imagePath0 = Path.Combine(Server.MapPath("~/Content/images"), "myMuseo.png");
            String image0 = Convert.ToBase64String(System.IO.File.ReadAllBytes(imagePath0));
            var imagePath1 = Path.Combine(Server.MapPath("~/Content/images"), "mymuseo-img.jpg");
            String image1 = Convert.ToBase64String(System.IO.File.ReadAllBytes(imagePath1));
            TemplateModel content = _collectorRespository.GetTemplateByName("New Message");
            string mailTemplate = content.TemplateText;
            string theMessage = String.Format(mailTemplate, image0, image1, toName, fromName, linkUrl);
            mailMessage.Body = theMessage;
            this.SendMail(mailMessage);
            //
            return Redirect("~/Home/CollectorDetail/" + message.ToCollectorId);
        }

        public ActionResult Itemslisting(int id, int order)
        {
            Session["ViewCollection"] = 0;
            ViewBag.CategoryId = id;
            ViewBag.Order = order;
            ViewBag.CategoryName = GetCategoryName(id);
            CollectiblesRespository collectiblesRespository = new CollectiblesRespository();
            if (order == 0)
            {
                ViewBag.CollectiblesList = collectiblesRespository.GetCollectiblesByCategory(id, "CreatedDate DESC");
            }
            if (order == 1)
            {
                ViewBag.CollectiblesList = collectiblesRespository.GetCollectiblesByCategory(id, "Price ASC");
            }
            if (order == 2)
            {
                ViewBag.CollectiblesList = collectiblesRespository.GetCollectiblesByCategory(id, "Price DESC");
            }
            if (order == 3)
            {
                ViewBag.CollectiblesList = collectiblesRespository.GetCollectiblesByCategory(id, "Title");
            }
            if (order == 4)
            {
                ViewBag.CollectiblesList = collectiblesRespository.GetCollectiblesByCategory(id, "ItemDate ASC");
            }
            if (order == 5)
            {
                ViewBag.CollectiblesList = collectiblesRespository.GetCollectiblesByCategory(id, "ItemDate DESC");
            }
            return View();
        }

        public ActionResult ItemsGallery(int id, int order)
        {
            Session["ViewCollection"] = 0;
            ViewBag.CategoryId = id;
            ViewBag.Order = order;
            ViewBag.CategoryName = GetCategoryName(id);
            CollectiblesRespository collectiblesRespository = new CollectiblesRespository();
            if (order == 0)
            {
                ViewBag.CollectiblesList = collectiblesRespository.GetCollectiblesByCategory(id, "CreatedDate DESC");
            }
            if (order == 1)
            {
                ViewBag.CollectiblesList = collectiblesRespository.GetCollectiblesByCategory(id, "Price ASC");
            }
            if (order == 2)
            {
                ViewBag.CollectiblesList = collectiblesRespository.GetCollectiblesByCategory(id, "Price DESC");
            }
            if (order == 3)
            {
                ViewBag.CollectiblesList = collectiblesRespository.GetCollectiblesByCategory(id, "Title");
            }
            if (order == 4)
            {
                ViewBag.CollectiblesList = collectiblesRespository.GetCollectiblesByCategory(id, "ItemDate ASC");
            }
            if (order == 5)
            {
                ViewBag.CollectiblesList = collectiblesRespository.GetCollectiblesByCategory(id, "ItemDate DESC");
            }
            return View();
        }

        public ActionResult AllCategories(int id)
        {
            Session["ViewCollection"] = 0;
            ViewBag.SelectOrderId = id;
            CollectiblesRespository collectiblesRespository = new CollectiblesRespository();
            List<Collectible> collectibles = collectiblesRespository.GetAllCollectibles(10000, "DESC");
            List<Collectible> selectedList = new List<Collectible>();
            SortedDictionary<String, List<Collectible>> workDictionnary = new SortedDictionary<string, List<Collectible>>();
            if (id == 1)
            {
                workDictionnary = new SortedDictionary<String, List<Collectible>>(Comparer<string>.Create((x, y) => y.CompareTo(x)));
            }
            foreach (Collectible item in collectibles)
            {
                if (item.CategoryId != 0)
                {
                    string key = GetCategoryName(item.CategoryId);
                    if (workDictionnary.ContainsKey(key))
                    {
                        workDictionnary[key].Add(item);
                    }
                    else
                    {
                        List<Collectible> value = new List<Collectible>();
                        value.Add(item);
                        workDictionnary.Add(key, value);
                    }
                }
            }
            ViewBag.DictItems = workDictionnary;
            return View();
        }

        public ActionResult AllMembers()
        {
            SortedDictionary<String, List<Collector>> workDictionnary = new SortedDictionary<string, List<Collector>>();
            var allCollectors = _collectorRespository.GetProfileCollectors(90000, "DESC");
            allCollectors = allCollectors.OrderBy(x => (x.LastName + x.FirstName)).ToList();
            foreach (Collector item in allCollectors)
            {
                string key = item.LastName.Substring(0, 1).ToUpper();
                if (workDictionnary.ContainsKey(key))
                {
                    workDictionnary[key].Add(item);
                }
                else
                {
                    List<Collector> value = new List<Collector>();
                    value.Add(item);
                    workDictionnary.Add(key, value);
                }
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("<div class='row'>");
            foreach(string key in workDictionnary.Keys)
            {
                sb.Append("<div class='col-md-3 col-xs-12'>");
                sb.Append("<h4>" + key + "</h4>");
                foreach(Collector item in workDictionnary[key])
                {
                    sb.Append("<a href='/Home/CollectorDetail/"+ item.CollectorId +"'>" + item.LastName + ", " + item.FirstName + "</a><br/>");
                }
                sb.Append("</div>");
            }
            sb.Append("</div>");
            ViewBag.AllMembers = sb.ToString();
            return View();
        }

        public ActionResult TipsForCollectors()
        {
            CollectorRespository collectorRepository = new CollectorRespository();
            ContentModel content = collectorRepository.GetContentByName("Tips For Collectors");
            if (content != null)
            {
                ViewBag.ContentText = content.ContentText;
            }
            else
            {
                ViewBag.ContentText = "";
            }
            return View();
        }

        public ActionResult TipsForArtists()
        {
            CollectorRespository collectorRepository = new CollectorRespository();
            ContentModel content = collectorRepository.GetContentByName("Tips For Artists");
            if (content != null)
            {
                ViewBag.ContentText = content.ContentText;
            }
            else
            {
                ViewBag.ContentText = "";
            }
            return View();
        }

        public ActionResult PrivatePolicy()
        {
            CollectorRespository collectorRepository = new CollectorRespository();
            ContentModel content = collectorRepository.GetContentByName("Privacy Policy");
            if (content != null)
            {
                ViewBag.ContentText = content.ContentText;
            }
            else
            {
                ViewBag.ContentText = "";
            }
            return View();
        }

        public ActionResult SellersFee()
        {
            CollectorRespository collectorRepository = new CollectorRespository();
            ContentModel content = collectorRepository.GetContentByName("Sellers Fee");
            if (content != null)
            {
                ViewBag.ContentText = content.ContentText;
            }
            else
            {
                ViewBag.ContentText = "";
            }
            return View();
        }

        public ActionResult HelpPage()
        {
            CollectorRespository collectorRepository = new CollectorRespository();
            ContentModel content = collectorRepository.GetContentByName("Help");
            if (content != null)
            {
                ViewBag.ContentText = content.ContentText;
            }
            else
            {
                ViewBag.ContentText = "";
            }
            return View();
        }


        public ActionResult ItemsSearch( string q)
        {
            ViewBag.CollectiblesList = _collectiblesRespository.SearchCollectibles(q);
            return View();
        }

        public FileContentResult GetCaptchaImage()
        {
            string text = Session["CAPTCHA"].ToString();
            return this.CreateCaptchaImage(text);
        }

   }
}