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
using System.Globalization;
using System.Text.RegularExpressions;

namespace MyMuseo.Controllers
{
    public class XtraController : BaseController
    {
        public XtraController() : base()
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
            foreach (Collectible item in collectibleRespository.GetAllCollectibles(10000, "DESC"))
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
                ViewBag.CollectorImage = _collector.ProfileImage;
            }
            else
            {
                Session["FullName"] = "";
                Session["CurrentCollectorId"] = 0;
            }
            ViewBag.GetUserName = new Func<int, string>(GetCollectorName);
            ViewBag.GetUserImage = new Func<int, string>(GetCollectorImage);

            ThreadRepository threadRepo = new ThreadRepository();
            var threads = threadRepo.GetAllThreads();
            foreach(Thread thread in threads)
            {
                thread.ThreadTopic = this.ConvertUrlsToLinks(thread.ThreadTopic);
                thread.LikesCount = threadRepo.GetThreadLikesCount(thread.ThreadId);
                thread.InterestedCount = threadRepo.GetThreadInterestedCount(thread.ThreadId);
                thread.GoingCount = threadRepo.GetThreadGoingCount(thread.ThreadId);
                if (Request.IsAuthenticated)
                {
                    thread.IsOwner = (thread.PostByCollectorId == _collector.CollectorId);
                }
                else
                {
                    thread.IsOwner = false;
                }
                var posts = threadRepo.GetThreadPosts(thread.ThreadId);
                foreach(Post post in posts)
                {
                    post.PostTopic = this.ConvertUrlsToLinks(post.PostTopic);
                    var replies = threadRepo.GetPostReplies(post.PostId);
                    post.Replies = replies;
                }
                thread.Posts = posts;
            }
            ViewBag.Threads = threads;
            return View();
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

            if ((bool)ViewBag.IsMe == true)
            {
                ViewBag.CollectorImage = _collector.ProfileImage;
                ViewBag.GetUserName = new Func<int, string>(GetCollectorName);
                ViewBag.GetUserImage = new Func<int, string>(GetCollectorImage);

                ThreadRepository threadRepo = new ThreadRepository();
                var threads = threadRepo.GetAllThreads();
                foreach (Thread thread in threads)
                {
                    thread.ThreadTopic = this.ConvertUrlsToLinks(thread.ThreadTopic);
                    thread.LikesCount = threadRepo.GetThreadLikesCount(thread.ThreadId);
                    thread.InterestedCount = threadRepo.GetThreadInterestedCount(thread.ThreadId);
                    thread.GoingCount = threadRepo.GetThreadGoingCount(thread.ThreadId);
                    if (Request.IsAuthenticated)
                    {
                        thread.IsOwner = (thread.PostByCollectorId == _collector.CollectorId);
                    }
                    else
                    {
                        thread.IsOwner = false;
                    }
                    var posts = threadRepo.GetThreadPosts(thread.ThreadId);
                    foreach (Post post in posts)
                    {
                        post.PostTopic = this.ConvertUrlsToLinks(post.PostTopic);
                        var replies = threadRepo.GetPostReplies(post.PostId);
                        post.Replies = replies;
                    }
                    thread.Posts = posts;
                }
                ViewBag.Threads = threads;
            }

            return View(collector);
        }

        [HttpPost]
        public ActionResult StartThread(FormCollection form)
        {
            ThreadRepository threadRepo = new ThreadRepository();
            Thread threadModel = new Thread();
            threadModel.ThreadTopic = form[0].ToString();
            threadModel.ThreadText = "";
            threadModel.ThreadTypeId = 0;
            threadModel.PostByCollectorId = _collector.CollectorId;
            threadModel.ThreadStartDate = DateTime.Now;
            threadModel.ThreadEndDate = DateTime.Now;
            threadModel.CreatedDate = DateTime.Now;
            threadRepo.InsertThread(threadModel);
            return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
        }

        [HttpPost]
        public ActionResult UpdateThread(FormCollection form)
        {
            ThreadRepository threadRepo = new ThreadRepository();
            Thread threadModel = new Thread();
            threadModel.ThreadTopic = form[0].ToString();
            threadModel.ThreadId = Convert.ToInt32(form[1].ToString());
            threadRepo.UpdateThread(threadModel);
            return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
        }


        [HttpPost]
        public ActionResult ReplyPost(FormCollection form)
        {
            ThreadRepository threadRepo = new ThreadRepository();
            Reply replyModel = new Reply();
            replyModel.ReplyTopic = form[0].ToString();
            replyModel.ReplyText = "";
            replyModel.PostId = Convert.ToInt32(form[1].ToString());
            replyModel.ReplyByCollectorId = _collector.CollectorId;
            replyModel.CreatedDate = DateTime.Now;
            threadRepo.InsertReply(replyModel);
            return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
        }

        [HttpPost]
        public ActionResult ReplyThread(FormCollection form)
        {
            ThreadRepository threadRepo = new ThreadRepository();
            Post postModel = new Post();
            postModel.PostTopic = form[0].ToString();
            postModel.PostText = "";
            postModel.ThreadId = Convert.ToInt32(form[1].ToString());
            postModel.PostByCollectorId = _collector.CollectorId;
            postModel.CreatedDate = DateTime.Now;
            threadRepo.InsertPost(postModel);
            //
            //if (_userSettings.NotifyWhenCommented)
            //{
                Thread model = _threadRepository.GetThreadById(postModel.ThreadId);
                var callbackUrl = Url.Action("CollectorDetail", "Home", new { id = _collector.CollectorId }, protocol: Request.Url.Scheme);
                string fromName = this.GetCollectorName(_collector.CollectorId);
                string fromEmail = this.GetCollectorEmail(_collector.CollectorId);
                string toName = this.GetCollectorName(model.PostByCollectorId);
                string toEmail = this.GetCollectorEmail(model.PostByCollectorId);
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress("no-reply@mymuseo.com");
                mailMessage.To.Add(new MailAddress(toEmail));
                mailMessage.CC.Add(_systemMails);
                mailMessage.Subject = "Commented on post";
                mailMessage.IsBodyHtml = true;
                TemplateModel content = _collectorRespository.GetTemplateByName("Commented on post");
                string mailTemplate = content.TemplateText;
                string theMessage = String.Format(mailTemplate, this.GetEmailImage0(), this.GetEmailImage1(), toName, fromName, "", callbackUrl);
                mailMessage.Body = theMessage;
                this.SendMail(mailMessage);
            //}
            //
            return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
        }

        [HttpPost]
        public ActionResult CreateEvent(FormCollection form, HttpPostedFileBase file)
        {
            try
            {
                string eventPath = "";
                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    string newFileName = DateTime.Now.ToFileTimeUtc().ToString() + "_" + fileName;
                    var path = Path.Combine(Server.MapPath("~/Content/uploads"), "event_" + newFileName);
                    file.SaveAs(path);
                    eventPath = "/Content/uploads/event_" + newFileName;
                }
                ThreadRepository threadRepo = new ThreadRepository();
                Thread threadModel = new Thread();
                threadModel.ThreadTypeId = 2;
                threadModel.ThreadImage = eventPath;
                threadModel.PostByCollectorId = _collector.CollectorId;
                threadModel.ThreadTopic = form["event-name"].ToString();
                threadModel.ThreadText = form["event-location"].ToString();
                string startTime = String.Format("{0}:{1} {2}", form["StartHour"], form["StartMinute"], form["StartAMPM"]);
                TimeSpan ts = DateTime.Parse(startTime).TimeOfDay;
                threadModel.ThreadStartDate = new DateTime(Convert.ToInt32(form["StartYear"].ToString()), Convert.ToInt32(form["StartMonth"].ToString()), Convert.ToInt32(form["StartDay"].ToString()), ts.Hours, ts.Minutes, ts.Seconds);
                string endTime = String.Format("{0}:{1} {2}", form["EndHour"], form["EndMinute"], form["EndAMPM"]);
                TimeSpan te = DateTime.Parse(endTime).TimeOfDay;
                threadModel.ThreadEndDate = new DateTime(Convert.ToInt32(form["EndYear"].ToString()), Convert.ToInt32(form["EndMonth"].ToString()), Convert.ToInt32(form["EndDay"].ToString()), te.Hours, te.Minutes, te.Seconds);
                threadModel.CreatedDate = DateTime.Now;
                threadRepo.InsertThread(threadModel);
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }
            catch(Exception e)
            {
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }
        }

        [HttpPost]
        public ActionResult CreatePhoto(FormCollection form, HttpPostedFileBase file)
        {
            try
            {
                string eventPath = "";
                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    string newFileName = DateTime.Now.ToFileTimeUtc().ToString() + "_" + fileName;
                    var path = Path.Combine(Server.MapPath("~/Content/uploads"), "photo_" + newFileName);
                    file.SaveAs(path);
                    eventPath = "/Content/uploads/photo_" + newFileName;
                }
                ThreadRepository threadRepo = new ThreadRepository();
                Thread threadModel = new Thread();
                threadModel.ThreadTypeId = 1;
                threadModel.ThreadImage = eventPath;
                threadModel.PostByCollectorId = _collector.CollectorId;
                threadModel.ThreadTopic = form["photo-message"].ToString();
                threadModel.ThreadText = "";
                threadModel.ThreadStartDate = DateTime.Now;
                threadModel.ThreadEndDate = DateTime.Now;
                threadModel.CreatedDate = DateTime.Now;
                threadRepo.InsertThread(threadModel);
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }
            catch (Exception e)
            {
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }
        }

        public int LikeMsg(int input, int thread)
        {
            ThreadRepository threadRepo = new ThreadRepository();
            ThreadLike likeModel = new ThreadLike();
            likeModel.ThreadId = thread;
            likeModel.LikeByCollectorId = _collector.CollectorId;
            likeModel.CreatedDate = DateTime.Now;
            threadRepo.InsertThreadLike(likeModel);
            return input + 1;
        }
        public int InterestedMsg(int input, int thread)
        {
            ThreadRepository threadRepo = new ThreadRepository();
            ThreadResponse respModel = new ThreadResponse();
            respModel.ThreadId = thread;
            respModel.ThreadResponseTypeId = 1;
            respModel.ResponseByCollectorId = _collector.CollectorId;
            respModel.CreatedDate = DateTime.Now;
            threadRepo.InsertThreadResponse(respModel);
            return input + 1;
        }

        public int GoingMsg(int input, int thread)
        {
            ThreadRepository threadRepo = new ThreadRepository();
            ThreadResponse respModel = new ThreadResponse();
            respModel.ThreadId = thread;
            respModel.ThreadResponseTypeId = 2;
            respModel.ResponseByCollectorId = _collector.CollectorId;
            respModel.CreatedDate = DateTime.Now;
            threadRepo.InsertThreadResponse(respModel);
            return input + 1;
        }

        public void DeleteThread(int thread)
        {
            ThreadRepository threadRepo = new ThreadRepository();
            threadRepo.DeleteThread(thread);
        }

        private string ConvertUrlsToLinks(string msg)
        {
            string regex = @"((www\.|(http|https|ftp|news|file)+\:\/\/)[&#95;.a-z0-9-]+\.[a-z0-9\/&#95;:@=.+?,##%&~-]*[^.|\'|\# |!|\(|?|,| |>|<|;|\)])";
            Regex r = new Regex(regex, RegexOptions.IgnoreCase);
            return r.Replace(msg, "<a href=\"$1\" title=\"Click to open in a new window or tab\" target=\"&#95;blank\">$1</a>").Replace("href=\"www", "href=\"http://www");
        }

    }


    public static class Month
    {
        public static int ToInt(this string month)
        {
            return Array.IndexOf(
                CultureInfo.CurrentCulture.DateTimeFormat.MonthNames,
                month.ToLower(CultureInfo.CurrentCulture))
                + 1;
        }
    }

}