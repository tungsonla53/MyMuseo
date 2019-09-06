using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyMuseo.Models;
using MyMuseo.DataService;
using System.IO;
using Microsoft.AspNet.Identity;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Drawing;
using System.Text.RegularExpressions;

namespace MyMuseo.Controllers
{
    public abstract class BaseController : Controller
    {
        internal CollectorRespository _collectorRespository = new CollectorRespository();
        internal CollectionsRespository _collectionsRespository = new CollectionsRespository();
        internal CollectiblesRespository _collectiblesRespository = new CollectiblesRespository();
        internal CategoriesRespository _categoriesRespository = new CategoriesRespository();
        internal CommentsRespository _commentsRepository = new CommentsRespository();
        internal CountryRepository _countryRepository = new CountryRepository();
        internal AddressRepository _addressRepository = new AddressRepository();
        internal FileDetailsRepository _fileDetailsRepository = new FileDetailsRepository();
        internal ThreadRepository _threadRepository = new ThreadRepository();
        internal Collector _collector;
        internal UserSettings _userSettings;
        internal String _systemMails = "tsonla@yahoo.com,gjamroski@mac.com,susan@mymuseo.com";
        private String _myMuseoEmailImage0;
        private String _myMuseoEmailImage1;

        public BaseController()
        {
            string userId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            if (userId != null)
            {
                _collector = _collectorRespository.GetCollector(userId);
                _userSettings = _collectorRespository.GetUserSettings(_collector.CollectorId);
                ViewBag.CurrentCollectorId = _collector.CollectorId;
                ViewBag.SideComments = GetSideComments();
                ViewBag.SideFavorites = GetSideFavorites();
                ViewBag.SideViews = GetSideViews();
                ViewBag.CollectorImage = _collector.ProfileImage;
                ViewBag.ThreadComments = _threadRepository.GetThreadPostsForACollector(_collector.CollectorId);
                ViewBag.FollowingIds = _collectorRespository.GetFollowingIds(_collector.CollectorId);
            }
            ViewBag.Threads = GetAllThread();
            ViewBag.GetUserName = new Func<int, string>(GetCollectorName);
            ViewBag.GetUserImage = new Func<int, string>(GetCollectorImage);
        }

        public string GetEmailImage0()
        {
            if (String.IsNullOrEmpty(_myMuseoEmailImage0))
            {
                var imagePath0 = Path.Combine(Server.MapPath("~/Content/images"), "myMuseo.png");
                _myMuseoEmailImage0 = Convert.ToBase64String(System.IO.File.ReadAllBytes(imagePath0));
            }
            return _myMuseoEmailImage0;
        }

        public string GetEmailImage1()
        {
            if (String.IsNullOrEmpty(_myMuseoEmailImage1))
            {
                var imagePath1 = Path.Combine(Server.MapPath("~/Content/images"), "mymuseo-img.jpg");
                _myMuseoEmailImage1 = Convert.ToBase64String(System.IO.File.ReadAllBytes(imagePath1));
            }
            return _myMuseoEmailImage1;
        }

        public string GetItemSource(int artistId)
        {
            if (artistId == 0)
            {
                return "Artist";
            }
            if (artistId == 1)
            {
                return "Studio";
            }
            if (artistId == 2)
            {
                return "Designer";
            }
            if (artistId == 3)
            {
                return "Brand";
            }
            if (artistId == 4)
            {
                return "Manufacturer";
            }
            if (artistId == 5)
            {
                return "Publisher";
            }
            if (artistId == 6)
            {
                return "Author";
            }
            return "Artist";
        }

        public string GetCurrentCollectorName()
        {
            Session["IsAdmin"] = _collector.IsAdmin;
            Session["FullName"] = String.Format("{0} {1}", _collector.FirstName, _collector.LastName);
            return String.Format("{0} {1}", _collector.FirstName, _collector.LastName);
        }

        public int GetCurrentCollectorId()
        {
            Session["IsAdmin"] = _collector.IsAdmin;
            return _collector.CollectorId;
        }

        public string GetContentText(string contentName)
        {
            ContentModel content = _collectorRespository.GetContentByName(contentName);
            if (content != null)
            {
                return content.ContentText;
            }
            else
            {
                return "";
            }
        }

        public void SendMail(MailMessage mailMessage)
        {
            SmtpClient smtpClient = new SmtpClient("smtp.sendgrid.###");
            smtpClient.Port = 587;
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;
            string gmailUser = "#######";
            string gmailPass = "@@@@@@@@@@";
            smtpClient.Credentials = new NetworkCredential(gmailUser, gmailPass);
            smtpClient.Send(mailMessage);
        }

        public void SendMailVia365(MailMessage mailMessage)
        {
            try
            {
                SmtpClient smtpClient = new SmtpClient();
                smtpClient.Host = "smtp.office365.com";
                smtpClient.Port = 25;
                smtpClient.EnableSsl = true;
                //smtpClient.UseDefaultCredentials = false;
                string mailUser = "no-reply@mymuseo.com";
                string mailPass = "Raj69356";
                smtpClient.Credentials = new NetworkCredential(mailUser, mailPass);
                smtpClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                smtpClient.Timeout = 600000;
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public string GetCollectorName(int collectorId)
        {
            if (collectorId == -1)
            {
                return "Visitor";
            }
            else if (collectorId == 0)
            {
                return "myMuseo";
            }
            else
            {
                CollectorRespository collectorRespository = new CollectorRespository();
                Collector collector = collectorRespository.GetCollector(collectorId);
                if (collector == null)
                {
                    return "User Deleted";
                }
                else
                {
                    return String.Format("{0} {1}", collector.FirstName, collector.LastName);
                }

            }
        }

        public string GetCollectorImage(int collectorId)
        {
            if (collectorId == -1)
            {
                return "";
            }
            else if (collectorId == 0)
            {
                return "/Content/images/mymuseo-thumb.png";
            }
            else
            {
                CollectorRespository collectorRespository = new CollectorRespository();
                Collector collector = collectorRespository.GetCollector(collectorId);
                if (collector == null)
                {
                    return "";
                }
                else
                {
                    return collector.ProfileImage;
                }

            }
        }

        public string GetCategoryName(int categoryId)
        {
            if (categoryId == 0)
            {
                return "Other";
            }
            else
            {
                Category category = _categoriesRespository.GetCategory(categoryId);
                if (category == null)
                {
                    return "Unknown";
                }
                return category.Name;
            }
        }

        public string GetCountryName(int countryId)
        {
            return _collectorRespository.GetCountryName(countryId);
        }

        public string GetUserEmail(string userId)
        {
            return _collectorRespository.GetUserEmail(userId);
        }

        public string GetCollectorEmail(int collectorId)
        {
            return _collectorRespository.GetUserEmail(_collectorRespository.GetCollector(collectorId).UserId);
        }

        public List<string> GetSideComments()
        {
            List<Comment> allComments =  _commentsRepository.GetCommentsRepliesForCollector(_collector.CollectorId);
            List<string> commentItems = new List<string>();
            foreach (Comment comment in allComments)
            {
                if (comment.FlagAsAbuse == false)
                {
                    if (comment.CollectibleId != 0)
                    {
                        Collectible item = _collectiblesRespository.GetCollectible(comment.CollectibleId);
                        if (item != null)
                        {
                            if (comment.ParentId == 0)
                            {
                                commentItems.Add(String.Format("New Comment on <a style='color: white;' href='../../Home/CollectibleDetail/{0}'>{1}</a>", item.CollectibleId, item.Title));
                            }
                            else
                            {
                                Comment parentComment = _commentsRepository.GetCommentById(comment.ParentId);
                                if (parentComment != null && parentComment.FlagAsAbuse == false)
                                {
                                    commentItems.Add(String.Format("New Reply on <a style='color: white;' href='../../Home/CollectibleDetail/{0}'>{1}</a>", item.CollectibleId, item.Title));
                                }
                            }
                        }
                    }
                }
            }
            return commentItems;
        }

        public List<string> GetSideFavorites()
        {
            List<Favorite> allFavorites = _collectorRespository.GetAllFavorites();
            List<string> myFavoriteItems = new List<string>();
            DateTime minDate = DateTime.Now.AddDays(-30);
            foreach (Favorite favorite in allFavorites)
            {   
                if (favorite.FavoriteCollectibleId != 0)
                {
                    Collectible item = _collectiblesRespository.GetCollectible(favorite.FavoriteCollectibleId);
                    if (item != null)
                    {
                        if (item.CollectorId == _collector.CollectorId)
                        {
                            Collector collector = _collectorRespository.GetCollector(favorite.CollectorId);
                            if (collector != null)
                            {
                                myFavoriteItems.Add(String.Format("<a style='color: white;' href='../../Home/CollectorDetail/{0}'>{1} {2}</a> favorited <a style='color: white;' href='../../Home/CollectibleDetail/{3}'>{4}</a>", collector.CollectorId, collector.FirstName, collector.LastName, item.CollectibleId, item.Title));
                            }
                        }
                    }
                }
            }
            return myFavoriteItems;
        }

        public List<string> GetSideViews()
        {
            List<string> myViewItems = new List<string>();
            List<ViewLog> viewLogs = _collectorRespository.GetViewLogsForCollector(_collector.CollectorId);
            DateTime startDateTime = DateTime.Now.AddHours(-1);
            foreach (ViewLog viewLog in viewLogs)
            {
                if (viewLog.CreatedDate > startDateTime)
                {

                    if (viewLog.ViewCollectibleId != 0)
                    {
                        Collectible item = _collectiblesRespository.GetCollectible(viewLog.ViewCollectibleId);
                        if (item != null)
                        {
                            myViewItems.Add(String.Format("<a style='color: white;' href='../../Home/CollectibleDetail/{0}'>{1}</a>", item.CollectibleId, item.Title));
                        }
                    }
                    if (viewLog.ViewCollectionId != 0)
                    {
                        Collection item = _collectionsRespository.GetCollection(viewLog.ViewCollectionId);
                        if (item != null)
                        {
                            myViewItems.Add(String.Format("<a style='color: white;' href='../../Home/CollectionDetail/{0}'>{1}</a>", item.CollectionId, item.Name));
                        }
                    }
                    if (viewLog.ViewCollectorId != 0)
                    {
                        Collector item = _collectorRespository.GetCollector(viewLog.ViewCollectorId);
                        if (item != null)
                        {
                            myViewItems.Add(String.Format("<a style='color: white;' href='../../Home/CollectorDetail/{0}'>{1} {2}</a>", item.CollectorId,item.FirstName, item.LastName));
                        }
                    }
                }
            }
            return myViewItems;
        }

        public void InsertCollectibeViewLog(int collectibeId)
        {
            ViewLog viewLog = new ViewLog();
            viewLog.ViewCollectionId = 0;
            viewLog.ViewCollectibleId = collectibeId;
            viewLog.ViewCollectorId = 0;
            viewLog.ViewPageName = String.Empty;
            viewLog.CreatedDate = DateTime.Now;
            if (_collector == null)
            {
                viewLog.ViewByCollectorId = 0;
            }
            else
            {
                viewLog.ViewByCollectorId = _collector.CollectorId;
            }
            _collectorRespository.InsertViewLog(viewLog);
        }

        public void InsertCollectionViewLog(int collectionId)
        {
            ViewLog viewLog = new ViewLog();
            viewLog.ViewCollectionId = collectionId;
            viewLog.ViewCollectibleId = 0;
            viewLog.ViewCollectorId = 0;
            viewLog.ViewPageName = String.Empty;
            viewLog.CreatedDate = DateTime.Now;
            if (_collector == null)
            {
                viewLog.ViewByCollectorId = 0;
            }
            else
            {
                viewLog.ViewByCollectorId = _collector.CollectorId;
            }
            _collectorRespository.InsertViewLog(viewLog);
        }

        public void InsertCollectorViewLog(int collectorId)
        {
            ViewLog viewLog = new ViewLog();
            viewLog.ViewCollectionId = 0;
            viewLog.ViewCollectibleId = 0;
            viewLog.ViewCollectorId = collectorId;
            viewLog.ViewPageName = String.Empty;
            viewLog.CreatedDate = DateTime.Now;
            if (_collector == null)
            {
                viewLog.ViewByCollectorId = 0;
            }
            else
            {
                viewLog.ViewByCollectorId = _collector.CollectorId;
            }
            _collectorRespository.InsertViewLog(viewLog);
        }

        public void InsertPageViewLog(string pageName)
        {
            ViewLog viewLog = new ViewLog();
            viewLog.ViewCollectionId = 0;
            viewLog.ViewCollectibleId = 0;
            viewLog.ViewCollectorId = 0;
            viewLog.ViewPageName = pageName;
            viewLog.CreatedDate = DateTime.Now;
            if (_collector == null)
            {
                viewLog.ViewByCollectorId = 0;
            }
            else
            {
                viewLog.ViewByCollectorId = _collector.CollectorId;
            }
            _collectorRespository.InsertViewLog(viewLog);
        }

        public bool UserCanFavoriteItem(int collectibleId)
        {
            List<int> collectibleIds = _collectorRespository.GetFavoriteCollectibleIds(_collector.CollectorId);
            return !collectibleIds.Contains(collectibleId);
        }

        public bool UserCanFavoriteCollection(int collectionId)
        {
            List<int> collectionIds = _collectorRespository.GetFavoriteCollectionIds(_collector.CollectorId);
            return !collectionIds.Contains(collectionId);
        }

        public bool UserCanFavoriteCollector(int collectorId)
        {
            List<int> collectorIds = _collectorRespository.GetFavoriteCollectorIds(collectorId);
            return !collectorIds.Contains(_collector.CollectorId);
        }

        public bool UserCanFollowCollector(int collectorId)
        {
            List<int> collectorIds = _collectorRespository.GetFollowerIds(collectorId);
            return !collectorIds.Contains(_collector.CollectorId);
        }

        public string GetCustomContent(string contentName)
        {
            ContentModel content = _collectorRespository.GetContentByName(contentName);
            if (content != null)
            {
                return content.ContentText;
            }
            else
            {
                return "";
            }
        }

        public string GetRandomText()
        {
            StringBuilder randomText = new StringBuilder();
            string alphabets = "012345679ACEFGHKLMNPRSWXZabcdefghijkhlmnopqrstuvwxyz";
            Random r = new Random();
            for (int j = 0; j <= 5; j++)
            {
                randomText.Append(alphabets[r.Next(alphabets.Length)]);
            }
            return randomText.ToString();
        }

        public FileContentResult CreateCaptchaImage(string text)
        {
            //first, create a dummy bitmap just to get a graphics object
            Image img = new Bitmap(1, 1);
            Graphics drawing = Graphics.FromImage(img);

            Font font = new Font("Arial", 15);
            //measure the string to see how big the image needs to be
            SizeF textSize = drawing.MeasureString(text, font);

            //free up the dummy image and old graphics object
            img.Dispose();
            drawing.Dispose();

            //create a new image of the right size
            img = new Bitmap((int)textSize.Width + 40, (int)textSize.Height + 20);
            drawing = Graphics.FromImage(img);

            Color backColor = Color.SeaShell;
            Color textColor = Color.Red;
            //paint the background
            drawing.Clear(backColor);

            //create a brush for the text
            Brush textBrush = new SolidBrush(textColor);

            drawing.DrawString(text, font, textBrush, 20, 10);

            drawing.Save();

            font.Dispose();
            textBrush.Dispose();
            drawing.Dispose();

            MemoryStream ms = new MemoryStream();
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            img.Dispose();

            return File(ms.ToArray(), "image/png");
        }

        public List<Thread> GetAllThread()
        {
            ThreadRepository threadRepo = new ThreadRepository();
            var threads = threadRepo.GetAllThreads();
            foreach (Thread thread in threads)
            {
                thread.ThreadTopic = this.ConvertUrlsToLinks(thread.ThreadTopic);
                thread.LikesCount = threadRepo.GetThreadLikesCount(thread.ThreadId);
                thread.InterestedCount = threadRepo.GetThreadInterestedCount(thread.ThreadId);
                thread.GoingCount = threadRepo.GetThreadGoingCount(thread.ThreadId);
                if (_collector != null)
                {
                    thread.IsOwner = (thread.PostByCollectorId == _collector.CollectorId);
                }
                else
                {
                    thread.IsOwner = false;
                }
                thread.HasComments = false;
                var posts = threadRepo.GetThreadPosts(thread.ThreadId);
                foreach (Post post in posts)
                {
                    post.PostTopic = this.ConvertUrlsToLinks(post.PostTopic);
                    var replies = threadRepo.GetPostReplies(post.PostId);
                    post.Replies = replies;
                    if (_collector != null)
                    {
                        if (post.PostByCollectorId == _collector.CollectorId)
                        {
                            thread.HasComments = true;
                        }
                    }
                }
                thread.Posts = posts;
            }
            return threads;
        }
        private string ConvertUrlsToLinks(string msg)
        {
            string regex = @"((www\.|(http|https|ftp|news|file)+\:\/\/)[&#95;.a-z0-9-]+\.[a-z0-9\/&#95;:@=.+?,##%&~-]*[^.|\'|\# |!|\(|?|,| |>|<|;|\)])";
            Regex r = new Regex(regex, RegexOptions.IgnoreCase);
            return r.Replace(msg, "<a href=\"$1\" title=\"Click to open in a new window or tab\" target=\"&#95;blank\">$1</a>").Replace("href=\"www", "href=\"http://www");
        }
    }
}
