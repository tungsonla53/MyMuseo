using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyMuseo.Models;
using MyMuseo.DataService;
using System.IO;
using Microsoft.AspNet.Identity;
using System.Net.Mail;

namespace MyMuseo.Controllers
{
    [Authorize]
    public class CollectorsController : BaseController
    {
        public CollectorsController() : base()
        {
        }

        public ActionResult AccountInfo()
        {
            string userId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            Collector collector = _collectorRespository.GetCollector(userId);
            ViewBag.TypeId = new SelectList(
               new List<SelectListItem>
               {
                    new SelectListItem {Text = "A Collector", Value = "0"},
                    new SelectListItem {Text = "An Artist or Craftsperson", Value = "1"},
                    new SelectListItem {Text = "An Artist/Craftsperson and collector", Value = "2"},
               }, "Value", "Text", collector.TypeId);
            return View(collector);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult AccountInfo(Collector model)
        {
            _collectorRespository.UpdateCollectorAccountInfo(model);
            return RedirectToAction("AccountInfo");
        }

        public ActionResult ProfileInfo(int? id)
        {
            if (id.HasValue)
            {
                if (id.Value == 1)
                {
                    Session["ProfileInfo"] = "CollectorDetail";
                }
            }
            string userId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            Collector collector = _collectorRespository.GetCollector(userId);
            ProfileInfo viewModel = new Models.ProfileInfo();
            viewModel.ProfileImage = collector.ProfileImage;
            viewModel.AboutMe = collector.AboutMe;
            viewModel.InterestList = new List<string>();
            if (collector.AreasOfInterest != null)
            {
                string[] areas = collector.AreasOfInterest.Split(',');
                for (int i = 0; i < areas.Length; i++)
                {
                    viewModel.InterestList.Add(areas[i]);
                }
                for (int i = 0; i < 6 - areas.Length; i++)
                {
                    viewModel.InterestList.Add(String.Empty);
                }
            }
            else
            {
                for (int i = 0; i < 6; i++)
                {
                    viewModel.InterestList.Add(String.Empty);
                }
            }

            viewModel.LinkUrlList = new List<LinkUrl>();
            if (collector.WebSites != null && collector.WebSites.Contains(':'))
            {
                string[] websites = collector.WebSites.Split(',');
                for (int i = 0; i < websites.Length; i++)
                {
                    string[] linkurl = websites[i].Split(':');
                    viewModel.LinkUrlList.Add(new LinkUrl { Name = linkurl[0], Url = linkurl[1] });
                }
                for (int i = 0; i < 6 - websites.Length; i++)
                {
                    viewModel.LinkUrlList.Add(new LinkUrl { Name = string.Empty, Url = String.Empty });
                }
            }
            else
            {
                for (int i = 0; i < 6; i++)
                {
                    viewModel.LinkUrlList.Add(new LinkUrl { Name = string.Empty, Url = String.Empty });
                }
            }
            return View(viewModel);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult ProfileInfo(ProfileInfo profileInfo, IEnumerable<HttpPostedFileBase> files)
        {
            if (ModelState.IsValid)
            {
                string userId = System.Web.HttpContext.Current.User.Identity.GetUserId();
                Collector collector = _collectorRespository.GetCollector(userId);
                collector.AboutMe = profileInfo.AboutMe;
                System.Text.StringBuilder sbAreas = new System.Text.StringBuilder();
                foreach (string area in profileInfo.InterestList)
                {
                    if (area != null)
                    {
                        if (sbAreas.Length == 0)
                        {
                            sbAreas.Append(area);
                        }
                        else
                        {
                            sbAreas.Append(",").Append(area);
                        }
                    }
                }
                collector.AreasOfInterest = sbAreas.ToString();

                System.Text.StringBuilder sbLinks = new System.Text.StringBuilder();
                foreach (LinkUrl link in profileInfo.LinkUrlList)
                {
                    if (link.Name != null)
                    {
                        if (sbLinks.Length == 0)
                        {
                            sbLinks.Append(link.Name).Append(":").Append(link.Url);
                        }
                        else
                        {
                            sbLinks.Append(",").Append(link.Name).Append(":").Append(link.Url);
                        }
                    }
                }

                collector.WebSites = sbLinks.ToString();

                _collectorRespository.UpdateCollector(collector);

                if (Session["ProfileInfo"] != null)
                {
                    return Redirect("~/Home/CollectorDetail/" + _collector.CollectorId);
                }
                return RedirectToAction("ProfileInfo");
            }
            return View(profileInfo);
        }

        public ActionResult AddressInfo()
        {
            AddressInfo viewModel = _addressRepository.GetCollectorAddress(_collector.CollectorId);
            if (viewModel == null)
            {
                viewModel = new AddressInfo();
                viewModel.CollectorId = 0;
            }
            ViewBag.CountryId = new SelectList(_countryRepository.GetAllCountries(), "CountryId", "Name", viewModel.CountryId);
            return View(viewModel);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult AddressInfo(AddressInfo addressInfo)
        {
            if (ModelState.IsValid)
            {
                if (addressInfo.CollectorId == 0)
                {
                    addressInfo.CreatedDate = DateTime.Now;
                    addressInfo.UpdatedDate = DateTime.Now;
                    addressInfo.CollectorId = _collector.CollectorId;
                    _addressRepository.InsertAddress(addressInfo);
                }
                else
                {
                    addressInfo.UpdatedDate = DateTime.Now;
                    _addressRepository.UpdateAddress(addressInfo);
                }
                return RedirectToAction("AddressInfo");
            }
            return View(addressInfo);
        }

        [AllowAnonymous]
        public ActionResult MyCollections(int id)
        {
            ViewBag.IsMe = false;
            if (Request.IsAuthenticated)
            {
                string userId = System.Web.HttpContext.Current.User.Identity.GetUserId();
                if (_collectorRespository.GetCollector(userId).CollectorId == id)
                {
                    ViewBag.IsMe = true;
                }
            }
            Collector collector = _collectorRespository.GetCollector(id);
            ViewBag.CollectiblesCount = _collectorRespository.GetCollectiblesCount(collector.CollectorId);
            ViewBag.FavoritesCount = _collectorRespository.GetFavoriteCollectibleIds(collector.CollectorId).Count;
            if ((bool)ViewBag.IsMe == true)
            {
                ViewBag.MyCollections = _collectionsRespository.GetCollections(collector.CollectorId);
                ViewBag.CollectionsCount = _collectorRespository.GetCollectionsCount(collector.CollectorId);
            }
            else
            {
                ViewBag.MyCollections = _collectionsRespository.GetCollections(collector.CollectorId).Where(x => x.IsDraft == false);
                ViewBag.CollectionsCount = _collectorRespository.GetActiveCollectionsCount(collector.CollectorId);
            }
            return View(collector);
        }

        [AllowAnonymous]
        public ActionResult MyCollectibles(int id)
        {
            Session["ViewCollection"] = 0;
            ViewBag.IsMe = false;
            if (Request.IsAuthenticated)
            {
                string userId = System.Web.HttpContext.Current.User.Identity.GetUserId();
                if (_collectorRespository.GetCollector(userId).CollectorId == id)
                {
                    ViewBag.IsMe = true;
                }
            }
            Collector collector = _collectorRespository.GetCollector(id);
            ViewBag.CollectiblesCount = _collectorRespository.GetCollectiblesCount(collector.CollectorId);
            ViewBag.FavoritesCount = _collectorRespository.GetFavoriteCollectibleIds(collector.CollectorId).Count;
            ViewBag.MyCollectibles = _collectiblesRespository.GetCollectibles(collector.CollectorId);
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

        [AllowAnonymous]
        public ActionResult MyFavorites(int id)
        {
            Session["ViewCollection"] = 0;
            ViewBag.IsMe = false;
            if (Request.IsAuthenticated)
            {
                string userId = System.Web.HttpContext.Current.User.Identity.GetUserId();
                if (id == 0)
                {
                    id = _collectorRespository.GetCollector(userId).CollectorId;
                    ViewBag.IsMe = true;
                }
                else
                {
                    if (_collectorRespository.GetCollector(userId).CollectorId == id)
                    {
                        ViewBag.IsMe = true;
                    }
                }
            }
            Collector collector = _collectorRespository.GetCollector(id);
            ViewBag.CollectiblesCount = _collectorRespository.GetCollectiblesCount(collector.CollectorId);
     
            List<int> favoriteIds = _collectorRespository.GetFavoriteCollectibleIds(collector.CollectorId);
            List<Collectible> myFavoriteItems = new List<Collectible>();
            foreach(int favoriteId in favoriteIds)
            {
                if (favoriteId != 0)
                {
                    Collectible item = _collectiblesRespository.GetCollectible(favoriteId);
                    if (item != null)
                    {
                        myFavoriteItems.Add(item);
                    }
                }
            }
            ViewBag.MyFavorites = myFavoriteItems;
            ViewBag.FavoritesCount = myFavoriteItems.Count;
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

        public ActionResult TaxForm()
        {
            ViewBag.FileDetailsList = _fileDetailsRepository.GetFileDetails(_collector.CollectorId);
            return View();
        }

        [HttpPost]
        public ActionResult TaxForm(HttpPostedFileBase files)
        {
            if (files != null)
            {
                String FileExt = Path.GetExtension(files.FileName).ToUpper();

                if (FileExt == ".PDF")
                {
                    Stream str = files.InputStream;
                    BinaryReader binaryReader = new BinaryReader(str);
                    Byte[] fileContent = binaryReader.ReadBytes((Int32)str.Length);

                    FileDetails fileDetails = new FileDetails();
                    fileDetails.FileName = Path.GetFileName(files.FileName);
                    fileDetails.FileContent = fileContent;
                    fileDetails.CollectorId = _collector.CollectorId;
                    fileDetails.UploadedDate = DateTime.Now;
                    _fileDetailsRepository.InsertFileDetails(fileDetails);
                    return RedirectToAction("TaxForm");
                }
                else
                {
                    ViewBag.FileStatus = "Invalid file format.";
                    ViewBag.FileDetailsList = _fileDetailsRepository.GetFileDetails(_collector.CollectorId);
                    return View();
                }
            }
            else
            {
                ViewBag.FileStatus = "No file selected";
                ViewBag.FileDetailsList = _fileDetailsRepository.GetFileDetails(_collector.CollectorId);
                return View();
            }

        }

        public ActionResult PrivacyPreferences()
        {
            UserSettings userSettings = _collectorRespository.GetUserSettings(_collector.CollectorId);
            if (userSettings == null)
            {
                userSettings = new UserSettings();
                userSettings.CollectorId = _collector.CollectorId;
                userSettings.CreatedDate = DateTime.Now;
                _collectorRespository.InsertUserSettings(userSettings);
            }
            ViewBag.AllowFollowId = userSettings.AllowFollowId;
            ViewBag.AllowUnfollowId = userSettings.AllowUnfollowId;
            ViewBag.FeaturePrivacyId = userSettings.FeaturePrivacyId;
            ViewBag.FollowPrivacyId = userSettings.FollowPrivacyId;
            ViewBag.LocationPrivacyId = userSettings.LocationPrivacyId;
            ViewBag.ProfilePrivacyId = userSettings.ProfilePrivacyId;
            return View(userSettings);
        }

        public ActionResult ViewPDF(int id)
        {
            FileDetails fd = _fileDetailsRepository.GetSingleFileDetails(id);
            ViewData["PDF"] = fd.FileContent;
            return View();
        }

        [HttpGet]
        public FileResult DownLoadPDF(int id)
        {
            FileDetails fd = _fileDetailsRepository.GetSingleFileDetails(id);
            return File(fd.FileContent, "application/pdf", fd.FileName);
        }

        [HttpPost]
        public ActionResult FollowCollector(int id)
        {
            Follow followModel = new Follow();
            followModel.CollectorId = _collector.CollectorId;
            followModel.FollowCollectorId = id;
            followModel.CreatedDate = DateTime.Now;
            _collectorRespository.FollowCollector(followModel);
            //
            var linkUrl = Url.Action("CollectorDetail", "Home", new { id = _collector.CollectorId }, protocol: Request.Url.Scheme);
            string followEmail = this.GetCollectorEmail(id);
            string followName = this.GetCollectorName(id);
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("no-reply@mymuseo.com");
            mailMessage.To.Add(new MailAddress(followEmail));
            mailMessage.CC.Add(new MailAddress("tsonla@yahoo.com"));
            mailMessage.CC.Add(new MailAddress("gjamroski@mac.com"));
            mailMessage.Subject = "New Follower";
            mailMessage.IsBodyHtml = true;
            var imagePath0 = Path.Combine(Server.MapPath("~/Content/images"), "myMuseo.png");
            String image0 = Convert.ToBase64String(System.IO.File.ReadAllBytes(imagePath0));
            var imagePath1 = Path.Combine(Server.MapPath("~/Content/images"), "mymuseo-img.jpg");
            String image1 = Convert.ToBase64String(System.IO.File.ReadAllBytes(imagePath1));
            TemplateModel content = _collectorRespository.GetTemplateByName("New Follower");
            string mailTemplate = content.TemplateText;
            string theMessage = String.Format(mailTemplate, image0, image1, followName, this.GetCurrentCollectorName(), linkUrl);
            mailMessage.Body = theMessage;
            this.SendMail(mailMessage);

            return Redirect("~/Home/CollectorDetail/" + id);
        }

        [HttpPost]
        public ActionResult FavoriteCollection(int id)
        {
            Collection collection = _collectionsRespository.GetCollection(id);
            Favorite favoriteModel = new Favorite();
            favoriteModel.CollectorId = _collector.CollectorId;
            favoriteModel.FavoriteCollectorId = collection.CollectorId;
            favoriteModel.FavoriteCollectionId = id;
            favoriteModel.FavoriteCollectibleId = 0;
            favoriteModel.CreatedDate = DateTime.Now;
            _collectorRespository.AddFavorite(favoriteModel);
            //
            var linkUrl = Url.Action("CollectorDetail", "Home", new { id = _collector.CollectorId }, protocol: Request.Url.Scheme);
            string followEmail = this.GetCollectorEmail(collection.CollectorId);
            string followName = this.GetCollectorName(collection.CollectorId);
            string favoriteItem = " your collection " + collection.Name;
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("no-reply@mymuseo.com");
            mailMessage.To.Add(new MailAddress(followEmail));
            mailMessage.CC.Add(new MailAddress("tsonla@yahoo.com"));
            mailMessage.CC.Add(new MailAddress("gjamroski@mac.com"));
            mailMessage.Subject = "New Favorite";
            mailMessage.IsBodyHtml = true;
            var imagePath0 = Path.Combine(Server.MapPath("~/Content/images"), "myMuseo.png");
            String image0 = Convert.ToBase64String(System.IO.File.ReadAllBytes(imagePath0));
            var imagePath1 = Path.Combine(Server.MapPath("~/Content/images"), "mymuseo-img.jpg");
            String image1 = Convert.ToBase64String(System.IO.File.ReadAllBytes(imagePath1));
            TemplateModel content = _collectorRespository.GetTemplateByName("New Favorite");
            string mailTemplate = content.TemplateText;
            string theMessage = String.Format(mailTemplate, image0, image1, followName, this.GetCurrentCollectorName(), favoriteItem, linkUrl);
            mailMessage.Body = theMessage;
            this.SendMail(mailMessage);
            //
            return Redirect("~/Home/CollectionDetail/" + id);
        }

        [HttpPost]
        public ActionResult FavoriteCollectible(int id)
        {
            Collectible collectible = _collectiblesRespository.GetCollectible(id);
            Favorite favoriteModel = new Favorite();
            favoriteModel.CollectorId = _collector.CollectorId;
            favoriteModel.FavoriteCollectorId = collectible.CollectorId;
            favoriteModel.FavoriteCollectionId = 0;
            favoriteModel.FavoriteCollectibleId = id;
            favoriteModel.CreatedDate = DateTime.Now;
            _collectorRespository.AddFavorite(favoriteModel);
            //
            var linkUrl = Url.Action("CollectorDetail", "Home", new { id = _collector.CollectorId }, protocol: Request.Url.Scheme);
            string followEmail = this.GetCollectorEmail(collectible.CollectorId);
            string followName = this.GetCollectorName(collectible.CollectorId);
            string favoriteItem = " your item " + collectible.Title;
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("no-reply@mymuseo.com");
            mailMessage.To.Add(new MailAddress(followEmail));
            mailMessage.CC.Add(new MailAddress("tsonla@yahoo.com"));
            mailMessage.CC.Add(new MailAddress("gjamroski@mac.com"));
            mailMessage.Subject = "New Favorite";
            mailMessage.IsBodyHtml = true;
            var imagePath0 = Path.Combine(Server.MapPath("~/Content/images"), "myMuseo.png");
            String image0 = Convert.ToBase64String(System.IO.File.ReadAllBytes(imagePath0));
            var imagePath1 = Path.Combine(Server.MapPath("~/Content/images"), "mymuseo-img.jpg");
            String image1 = Convert.ToBase64String(System.IO.File.ReadAllBytes(imagePath1));
            TemplateModel content = _collectorRespository.GetTemplateByName("New Favorite");
            string mailTemplate = content.TemplateText;
            string theMessage = String.Format(mailTemplate, image0, image1, followName, this.GetCurrentCollectorName(), favoriteItem, linkUrl);
            mailMessage.Body = theMessage;
            this.SendMail(mailMessage);
            //
            return Redirect("~/Home/CollectibleDetail/" + id);
        }

        [HttpPost]
        public ActionResult FavoriteCollector(int id)
        {
            Favorite favoriteModel = new Favorite();
            favoriteModel.CollectorId = _collector.CollectorId;
            favoriteModel.FavoriteCollectorId = id;
            favoriteModel.FavoriteCollectionId = 0;
            favoriteModel.FavoriteCollectibleId = 0;
            favoriteModel.CreatedDate = DateTime.Now;
            _collectorRespository.AddFavorite(favoriteModel);
            //
            var linkUrl = Url.Action("CollectorDetail", "Home", new { id = _collector.CollectorId }, protocol: Request.Url.Scheme);
            string followEmail = this.GetCollectorEmail(id);
            string followName = this.GetCollectorName(id);
            string favoriteItem = " your profile";
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("no-reply@mymuseo.com");
            mailMessage.To.Add(new MailAddress(followEmail));
            mailMessage.CC.Add(new MailAddress("tsonla@yahoo.com"));
            mailMessage.CC.Add(new MailAddress("gjamroski@mac.com"));
            mailMessage.Subject = "New Favorite";
            mailMessage.IsBodyHtml = true;
            var imagePath0 = Path.Combine(Server.MapPath("~/Content/images"), "myMuseo.png");
            String image0 = Convert.ToBase64String(System.IO.File.ReadAllBytes(imagePath0));
            var imagePath1 = Path.Combine(Server.MapPath("~/Content/images"), "mymuseo-img.jpg");
            String image1 = Convert.ToBase64String(System.IO.File.ReadAllBytes(imagePath1));
            TemplateModel content = _collectorRespository.GetTemplateByName("New Favorite");
            string mailTemplate = content.TemplateText;
            string theMessage = String.Format(mailTemplate, image0, image1, followName, this.GetCurrentCollectorName(), favoriteItem, linkUrl);
            mailMessage.Body = theMessage;
            this.SendMail(mailMessage);
            //
            return Redirect("~/Home/CollectorDetail/" + id);
        }

        public ActionResult AddCollection()
        {
            Collection model = new Collection();
            // TODO
            ViewBag.NewCollectibles = _collectiblesRespository.GetAvailableCollectibles(_collector.CollectorId);
            return View(model);
        }

        [HttpPost]
        public ActionResult AddCollection(Collection collection)
        {
            if (ModelState.IsValid)
            {
                // is private 
                collection.IsForSale = !collection.IsForSale;
                if (collection.FeaturedItemId == 0)
                {
                    collection.NormalImage = "/Content/images/placeholder-collections-collectibles-favorites.jpg";
                    collection.ThumbImage = "/Content/images/thumbnail-placeholder.jpg";
                }
                else
                {
                    Collectible collectible = _collectiblesRespository.GetCollectible(collection.FeaturedItemId);
                    if (collectible != null)
                    {
                        collection.NormalImage = collectible.NormalImage;
                        collection.ThumbImage = collectible.ThumbImage;
                    }
                }
                collection.CreatedDate = DateTime.Now;
                collection.CollectorId = _collector.CollectorId;
                collection.Price = Decimal.Zero;
                int collectionId = _collectionsRespository.InsertCollection(collection);
                // TODO
                if (collection.FeaturedItemId != 0)
                {
                    Collectible collectible = _collectiblesRespository.GetCollectible(collection.FeaturedItemId);
                    if (collectible != null)
                    {
                        if (collectible.CollectionId == 0)
                        {
                            collectible.CollectionId = collectionId;
                            _collectiblesRespository.UpdateCollectible(collectible);
                        }
                    }
                }
                //
                return Redirect("~/Home/CollectionDetail/" + collectionId);
            }
            ViewBag.NewCollectibles = _collectiblesRespository.GetAvailableCollectibles(_collector.CollectorId);
            return View(collection);
        }


        public ActionResult UpdateCollection(int id)
        {
            Collection model = _collectionsRespository.GetCollection(id);
            CollectiblesRespository collectiblesRespository = new CollectiblesRespository();
            ViewBag.NewCollectibles = collectiblesRespository.GetAvailableCollectibles(_collector.CollectorId);
            return View(model);
        }

        [HttpPost]
        public ActionResult UpdateCollection(Collection collection)
        {
            if (ModelState.IsValid)
            {
                _collectionsRespository.UpdateCollectionHeader(collection);
                return Redirect("~/Home/CollectionDetail/" + collection.CollectionId);
            }

            return View(collection);
        }


        [HttpGet]
        public ActionResult _ProfilePrivacy()
        {
            return PartialView("_ProfilePrivacy");
        }

        [HttpPost]
        public ActionResult _ProfilePrivacy(FormCollection form)
        {
            ViewBag.ProfilePrivacyId = form[0];
            int profilePrivacyId = Convert.ToInt16(form[0]);
            UserSettings model = new UserSettings();
            model.CollectorId = _collector.CollectorId;
            model.ProfilePrivacyId = profilePrivacyId;
            _collectorRespository.UpdateProfilePrivacy(model);
            return PartialView("_ProfilePrivacy");
        }

        [HttpGet]
        public ActionResult _FollowPrivacy()
        {
            return PartialView("_FollowPrivacy");
        }

        [HttpPost]
        public ActionResult _FollowPrivacy(FormCollection form)
        {
            ViewBag.FollowPrivacyId = form[0];
            int followPrivacyId = Convert.ToInt16(form[0]);
            UserSettings model = new UserSettings();
            model.CollectorId = _collector.CollectorId;
            model.FollowPrivacyId = followPrivacyId;
            _collectorRespository.UpdateFollowPrivacy(model);
            return PartialView("_FollowPrivacy");
        }

        [HttpGet]
        public ActionResult _FeaturePrivacy()
        {
            return PartialView("_FeaturePrivacy");
        }

        [HttpPost]
        public ActionResult _FeaturePrivacy(FormCollection form)
        {
            ViewBag.FeaturePrivacyId = form[0];
            int featurePrivacyId = Convert.ToInt16(form[0]);
            UserSettings model = new UserSettings();
            model.CollectorId = _collector.CollectorId;
            model.FeaturePrivacyId = featurePrivacyId;
            _collectorRespository.UpdateFeaturePrivacy(model);
            return PartialView("_FeaturePrivacy");
        }

        [HttpGet]
        public ActionResult _LocationPrivacy()
        {
            return PartialView("_LocationPrivacy");
        }

        [HttpPost]
        public ActionResult _LocationPrivacy(FormCollection form)
        {
            ViewBag.LocationPrivacyId = form[0];
            int locationPrivacyId = Convert.ToInt16(form[0]);
            UserSettings model = new UserSettings();
            model.CollectorId = _collector.CollectorId;
            model.LocationPrivacyId = locationPrivacyId;
            _collectorRespository.UpdateLocationPrivacy(model);
            return PartialView("_LocationPrivacy");
        }

        [HttpGet]
        public ActionResult _AllowFollow()
        {
            return PartialView("_AllowFollow");
        }

        [HttpPost]
        public ActionResult _AllowFollow(FormCollection form)
        {
            ViewBag.AllowFollowId = form[0];
            int allowFollowId = Convert.ToInt16(form[0]);
            UserSettings model = new UserSettings();
            model.CollectorId = _collector.CollectorId;
            model.AllowFollowId = allowFollowId;
            _collectorRespository.UpdateAllowFollow(model);
            return PartialView("_AllowFollow");
        }

        [HttpGet]
        public ActionResult _AllowUnfollow()
        {
            return PartialView("_AllowUnfollow");
        }

        [HttpPost]
        public ActionResult _AllowUnfollow(FormCollection form)
        {
            ViewBag.AllowUnfollowId = form[0];
            int allowUnfollowId = Convert.ToInt16(form[0]);
            UserSettings model = new UserSettings();
            model.CollectorId = _collector.CollectorId;
            model.AllowUnfollowId = allowUnfollowId;
            _collectorRespository.UpdateAllowUnfollow(model);
            return PartialView("_AllowUnfollow");
        }

        [HttpPost]
        public ActionResult EmailPreferences(UserSettings model)
        {
            model.CollectorId = _collector.CollectorId;
            _collectorRespository.UpdateEmailPreferences(model);
            return Redirect("~/Collectors/PrivacyPreferences/");
        }

        public ActionResult AddCollectionDetail(int id)
        {
            Collectible model = new Collectible();
            model.CollectionId = id;
            ViewBag.CategoryId = this.GetCategories();
            return View(model);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult AddCollectionDetail(Collectible model)
        {
            model.CollectorId = _collector.CollectorId;
            model.CreatedDate = DateTime.Now;
            CollectiblesRespository collectiblesRepository = new CollectiblesRespository();
            collectiblesRepository.InsertCollectible(model);
            return View();
        }

        public ActionResult _AddDetail(int id)
        {
            Collectible model = new Collectible();
            model.CollectionId = id;
            return PartialView("_AddDetail",model);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult _AddDetail(Collectible model)
        {
            model.CollectorId = _collector.CollectorId;
            model.CreatedDate = DateTime.Now;
            CollectiblesRespository collectiblesRepository = new CollectiblesRespository();
            collectiblesRepository.InsertCollectible(model);
            return PartialView();
        }

        public ActionResult EditCollectibleDetail(int id)
        {
            CollectiblesRespository collectiblesRepository = new CollectiblesRespository();
            CollectionsRespository collectionsRespository = new CollectionsRespository();
            CategoriesRespository categoriesRespository = new CategoriesRespository();
            Collectible model = collectiblesRepository.GetCollectible(id);
            ViewBag.CategoryId = new SelectList(categoriesRespository.GetAllCategories(), "CategoryId", "Name", model.CategoryId);
            ViewBag.CollectionId = new SelectList(collectionsRespository.GetCollections(model.CollectorId), "CollectionId", "Name", model.CollectionId);
            ViewBag.ArtistId = new SelectList(
               new List<SelectListItem>
               {
                    new SelectListItem {Text = "Artist", Value = "0"},
                    new SelectListItem {Text = "Studio", Value = "1"},
                    new SelectListItem {Text = "Designer", Value = "2"},
                    new SelectListItem {Text = "Brand", Value = "3"},
                    new SelectListItem {Text = "Manufacturer", Value = "4"},
                    new SelectListItem {Text = "Publisher", Value = "5"},
                    new SelectListItem {Text = "Author", Value = "6"},
               }, "Value", "Text", model.ArtistId);
            return View(model);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditCollectibleDetail(Collectible model)
        {
            CollectiblesRespository collectiblesRepository = new CollectiblesRespository();
            collectiblesRepository.UpdateCollectible(model);
            return Redirect("~/Home/CollectibleDetail/" + model.CollectibleId);
        }


        [HttpPost]
        public ActionResult PurchaseCollectible(FormCollection form)
        {
            int id = Convert.ToInt16(form[0]);
            Collectible collectible = _collectiblesRespository.GetCollectible(id);
            Purchase purchaseModel = new Purchase();
            purchaseModel.CustomerId = _collector.CollectorId;
            purchaseModel.CollectibleId = id;
            purchaseModel.CollectorId = collectible.CollectorId;
            purchaseModel.Title = collectible.Title;
            purchaseModel.Image = collectible.ThumbImage;
            purchaseModel.Price = collectible.Price;
            purchaseModel.Count = 1;
            purchaseModel.TypeId = 1;
            purchaseModel.StatusId = 1;
            purchaseModel.CreatedDate = DateTime.Now;
            int purchaseId = _collectorRespository.InsertPurchase(purchaseModel);
            return Redirect("~/Home/Purchase/" + id);
        }

        [HttpPost]
        public ActionResult OfferCollectible(FormCollection form)
        {
            int id = Convert.ToInt16(form[0]);
            Decimal amount = Convert.ToDecimal(form[1]);
            Collectible collectible = _collectiblesRespository.GetCollectible(id);
            Purchase purchaseModel = new Purchase();
            purchaseModel.CustomerId = _collector.CollectorId;
            purchaseModel.CollectibleId = id;
            purchaseModel.CollectorId = collectible.CollectorId;
            purchaseModel.Title = collectible.Title;
            purchaseModel.Image = collectible.ThumbImage;
            purchaseModel.Price = amount;
            purchaseModel.Count = 1;
            purchaseModel.TypeId = 2;
            purchaseModel.StatusId = 1;
            purchaseModel.CreatedDate = DateTime.Now;
            int purchaseId = _collectorRespository.InsertPurchase(purchaseModel);
            return Redirect("~/Home/Offer/" + purchaseId);
        }

        public SelectList GetCategories ()
        {
            CategoriesRespository categoriesRespository = new CategoriesRespository();
            List<Category> categories = categoriesRespository.GetAllCategories();
            return new SelectList(categories, "CategoryId", "Name");
        }

        public SelectList GetCollections(int collectorId)
        {
            CollectionsRespository collectionsRespository = new CollectionsRespository();
            return new SelectList(collectionsRespository.GetCollections(collectorId), "CollectionId", "Name");
        }

        [HttpPost]
        public ActionResult DeleteCollectible(FormCollection form)
        {
            int id = Convert.ToInt16(form[0]);
            _collectiblesRespository.DeleteCollectible(id);
            return Redirect("~/Collectors/MyCollectibles/" + _collector.CollectorId);
        }

        [HttpPost]
        public ActionResult DeleteCollection(FormCollection form)
        {
            int id = Convert.ToInt16(form[0]);
            _collectionsRespository.DeleteCollection(id);
            _collectiblesRespository.DetachItemsFromCollection(id);
            return Redirect("~/Collectors/MyCollections/" + _collector.CollectorId);
        }

        [HttpPost]
        public ActionResult ShippingCollectible(FormCollection form)
        {
            int id = Convert.ToInt16(form[0]);
            string trackingNumber = form[1].ToString();
            string shipper = form[2].ToString();
            _collectorRespository.UpdateShipping(id, trackingNumber, shipper);
            //
            //var linkUrl = Url.Action("CollectorDetail", "Home", new { id = _collector.CollectorId }, protocol: Request.Url.Scheme);
            Purchase purchaseModel = _collectorRespository.GetPurchase(id);
            string toEmail = this.GetCollectorEmail(purchaseModel.CustomerId);
            string toName = this.GetCollectorName(purchaseModel.CustomerId);
            string fromName = this.GetCurrentCollectorName();
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("no-reply@mymuseo.com");
            mailMessage.To.Add(new MailAddress(toEmail));
            mailMessage.CC.Add(new MailAddress("tsonla@yahoo.com"));
            mailMessage.CC.Add(new MailAddress("gjamroski@mac.com"));
            mailMessage.Subject = "Shipped Item Details";
            mailMessage.IsBodyHtml = true;
            var imagePath0 = Path.Combine(Server.MapPath("~/Content/images"), "myMuseo.png");
            String image0 = Convert.ToBase64String(System.IO.File.ReadAllBytes(imagePath0));
            var imagePath1 = Path.Combine(Server.MapPath("~/Content/images"), "mymuseo-img.jpg");
            String image1 = Convert.ToBase64String(System.IO.File.ReadAllBytes(imagePath1));
            TemplateModel content = _collectorRespository.GetTemplateByName("Shipped item Details");
            string mailTemplate = content.TemplateText;
            string theMessage = String.Format(mailTemplate, image0, image1, toName, fromName, shipper, trackingNumber);
            mailMessage.Body = theMessage;
            this.SendMail(mailMessage);
            //
            return Redirect("~/Dashboard/Sales");
        }

        

        [HttpPost]
        public ActionResult RemovePendingCollectible(FormCollection form)
        {
            int id = Convert.ToInt16(form[0]);
            _collectorRespository.RemovePendingCollectible(id);
            return Redirect("~/Home/CollectibleDetail/" + id);
        }


    }
}