using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using MyMuseo.DataService;
using MyMuseo.Models;
using Microsoft.AspNet.Identity;

namespace MyMuseo.Controllers
{
    public class AvatarController : Controller
    {
        private const string TempFolder = "/Temp";
        private const string MapTempFolder = "~" + TempFolder;
        private const string AvatarPath = "/Content/uploads/profile";
        private const string OriginalPath = "/Content/uploads/original";
        private const string ThumbPath = "/Content/uploads/thumb";

        private readonly string[] _imageFileExtensions = { ".jpg", ".png", ".gif", ".jpeg" };
            
        [HttpGet]
        public ActionResult Upload()
        {
            Session["UploadImage"] = "ProfileInfo";
            return View();
        }

        [HttpGet]
        public ActionResult _Upload()
        {
            return PartialView();
        }

        [HttpGet]
        public ActionResult UploadProfileImage()
        {
            Session["UploadImage"] = "ProfileImage";
            return View();
        }

        [HttpGet]
        public ActionResult _UploadProfileImage()
        {
            return PartialView();
        }

        [HttpGet]
        public ActionResult UploadFeaturedImage()
        {
            Session["UploadImage"] = "FeaturedImage";
            return View();
        }

        [HttpGet]
        public ActionResult _UploadFeaturedImage()
        {
            return PartialView();
        }

        [HttpGet]
        public ActionResult UploadCollectionImage(int id)
        {
            Session["UploadImage"] = "CollectionImage";
            Session["UploadCollectionId"] = id;
            ViewBag.CollectionId = id;
            return View();
        }

        [HttpGet]
        public ActionResult _UploadCollectionImage()
        {
            return PartialView();
        }

        [HttpGet]
        public ActionResult UploadCollectibleImage(int id)
        {
            Session["UploadImage"] = "CollectibleImage";
            Session["UploadCollectionId"] = id;
            CategoriesRespository categoriesRespository = new CategoriesRespository();
            List<Category> categories = categoriesRespository.GetAllCategories();
            ViewBag.CategoryId = new SelectList(categories, "CategoryId", "Name");
            CollectorRespository collectorRespository = new CollectorRespository();
            Collector collector = collectorRespository.GetCollector(User.Identity.GetUserId());
            CollectionsRespository collectionsRespository = new CollectionsRespository();
            ViewBag.CollectionId = new SelectList(collectionsRespository.GetCollections(collector.CollectorId), "CollectionId", "Name", id);
            Collectible model = new Collectible();
            model.CollectorId = collector.CollectorId;
            model.CollectionId = id;
            return View(model);
        }

        [HttpGet]
        public ActionResult _UploadCollectibleImage()
        {
            return PartialView();
        }

        [HttpGet]
        public ActionResult UpdateCollectibleImage(int id)
        {
            Session["UploadImage"] = "UpdateCollectibleImage";
            Session["UpdateCollectibleId"] = id;
            ViewBag.CollectibleId = id;
            return View();
        }

        [HttpGet]
        public ActionResult _UpdateCollectibleImage()
        {
            return PartialView();
        }

        [ValidateAntiForgeryToken]
        public ActionResult _Upload(IEnumerable<HttpPostedFileBase> files)
        {
            if (files == null || !files.Any()) return Json(new { success = false, errorMessage = "No file uploaded." });
            var file = files.FirstOrDefault();  // get ONE only
            if (file == null || !IsImage(file)) return Json(new { success = false, errorMessage = "File is of wrong format." });
            if (file.ContentLength <= 0) return Json(new { success = false, errorMessage = "File cannot be zero length." });
            var webPath = GetTempSavedFilePath(file);
            //mistertommat - 18 Nov '15 - replacing '\' to '//' results in incorrect image url on firefox and IE,
            //                            therefore replacing '\\' to '/' so that a proper web url is returned.            
            return Json(new { success = true, fileName = webPath.Replace("\\", "/") }); // success
        }

        [HttpPost]
        public ActionResult Save(string t, string l, string h, string w, string fileName)
        {
            try
            {
                int AvatarStoredWidth = 600;
                int AvatarStoredHeight = 400;

                if (Session["UploadImage"].ToString() == "ProfileImage")
                {
                    AvatarStoredWidth = 250;
                    AvatarStoredHeight = 250;
                }

                // Calculate dimensions
                var top = Convert.ToInt32(t.Replace("-", "").Replace("px", ""));
                var left = Convert.ToInt32(l.Replace("-", "").Replace("px", ""));
                var height = Convert.ToInt32(h.Replace("-", "").Replace("px", ""));
                var width = Convert.ToInt32(w.Replace("-", "").Replace("px", ""));

                // Get file from temporary folder
                var fn = Path.Combine(Server.MapPath(MapTempFolder), Path.GetFileName(fileName));
                // ...get image and resize it, ...
                var img = new WebImage(fn);
                img.Resize(width, height);
                // ... crop the part the user selected, ...
                try { 
                    img.Crop(top, left, img.Height - top - AvatarStoredHeight, img.Width - left - AvatarStoredWidth);
                }
                catch(Exception e)
                {
                    string s = e.Message;
                    //img.Crop(top, left, 0, 0);
                }
                // ... delete the temporary file,...
                System.IO.File.Delete(fn);
                // ... and save the new one.
                var newFileName = Path.Combine(AvatarPath, Path.GetFileName(fn));
                var newFileLocation = HttpContext.Server.MapPath(newFileName);
                if (Directory.Exists(Path.GetDirectoryName(newFileLocation)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(newFileLocation));
                }

                img.Save(newFileLocation);

                // Update collector profile image
                CollectorRespository repository = new CollectorRespository();
                string userId = System.Web.HttpContext.Current.User.Identity.GetUserId();
                Collector collector = repository.GetCollector(userId);
                string imageFileName = newFileName.Replace("\\", "/");

                Session["ImageFileName"] = imageFileName;

                if (Session["UploadImage"].ToString() == "ProfileInfo")
                {
                    repository.UpdateCollectorProfileImage(collector.CollectorId, imageFileName);
                }
                if (Session["UploadImage"].ToString() == "ProfileImage")
                {
                    repository.UpdateCollectorProfileImage(collector.CollectorId, imageFileName);
                }
                if (Session["UploadImage"].ToString() == "FeaturedImage")
                {
                    repository.UpdateCollectorFeaturedImage(collector.CollectorId, imageFileName);
                }
                if (Session["UploadImage"].ToString() == "CollectionImage")
                {
                    CollectionsRespository collectionRepository = new CollectionsRespository();
                    var originalFileName = Session["OriginalFileName"].ToString();
                    collectionRepository.UpdateCollectionImage(Convert.ToInt32(Session["UploadCollectionId"].ToString()), imageFileName, originalFileName);
                }
                if (Session["UploadImage"].ToString() == "UpdateCollectibleImage")
                {
                    CollectiblesRespository collectibleRepository = new CollectiblesRespository();
                    var originalFileName = Session["OriginalFileName"].ToString();
                    collectibleRepository.UpdateCollectibleImage(Convert.ToInt32(Session["UpdateCollectibleId"].ToString()), imageFileName, originalFileName);
                }
                //
                return Json(new { success = true, avatarFileLocation = newFileName });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMessage = "Unable to upload file.\nERRORINFO: " + ex.Message });
            }
        }


        [HttpPost]
        public ActionResult Save2(string t, string l, string h, string w, string fileName, string t2, string l2, string h2, string w2 )
        {
            try
            {
                int AvatarStoredWidth = 600;
                int AvatarStoredHeight = 400;

                int ThumbStoredWidth = 250;
                int ThumbStoredHeight = 250;


                // Calculate dimensions
                var top = Convert.ToInt32(t.Replace("-", "").Replace("px", ""));
                var left = Convert.ToInt32(l.Replace("-", "").Replace("px", ""));
                var height = Convert.ToInt32(h.Replace("-", "").Replace("px", ""));
                var width = Convert.ToInt32(w.Replace("-", "").Replace("px", ""));

                var top2 = Convert.ToInt32(t2.Replace("-", "").Replace("px", ""));
                var left2 = Convert.ToInt32(l2.Replace("-", "").Replace("px", ""));
                var height2 = Convert.ToInt32(h2.Replace("-", "").Replace("px", ""));
                var width2 = Convert.ToInt32(w2.Replace("-", "").Replace("px", ""));

                // Get file from temporary folder
                var fn = Path.Combine(Server.MapPath(MapTempFolder), Path.GetFileName(fileName));
                // ...get image and resize it, ...
                var img = new WebImage(fn);

                //img.Resize(width, height);
                // ... crop the part the user selected, ...
                try { 
                img.Crop(top, left, img.Height - top - AvatarStoredHeight, img.Width - left - AvatarStoredWidth);
                }
                catch(Exception e)
                {
                    //
                }
                // ... delete the temporary file,...
                //System.IO.File.Delete(fn);
                // ... and save the new one.
                var newFileName = Path.Combine(AvatarPath, Path.GetFileName(fn));
                var newFileLocation = HttpContext.Server.MapPath(newFileName);
                if (Directory.Exists(Path.GetDirectoryName(newFileLocation)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(newFileLocation));
                }
                img.Save(newFileLocation);


                var img2 = new WebImage(fn);
                img2.Resize(width2, height2);
                try
                {
                    img2.Crop(top2, left2, img2.Height - top2 - ThumbStoredHeight, img2.Width - left2 - ThumbStoredWidth);
                }
                catch (Exception e)
                {
                    img2.Resize(ThumbStoredWidth, ThumbStoredHeight);
                }
                var thumbFileName = Path.Combine(ThumbPath, Path.GetFileName(fn));
                var thumbFileLocation = HttpContext.Server.MapPath(thumbFileName);
                if (Directory.Exists(Path.GetDirectoryName(thumbFileLocation)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(thumbFileLocation));
                }
                img2.Save(thumbFileLocation);

                //
                System.IO.File.Delete(fn);
                // 
                string imageFileName = newFileName.Replace("\\", "/");
                string thumbImageFileName = thumbFileName.Replace("\\", "/");
                Session["ImageFileName"] = imageFileName;
                Session["ThumbFileName"] = thumbImageFileName;
                //
                return Json(new { success = true, avatarFileLocation = newFileName });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMessage = "Unable to upload file.\nERRORINFO: " + ex.Message });
            }
        }

        private bool IsImage(HttpPostedFileBase file)
        {
            if (file == null) return false;
            return file.ContentType.Contains("image") ||
                _imageFileExtensions.Any(item => file.FileName.EndsWith(item, StringComparison.OrdinalIgnoreCase));
        }

        private string GetTempSavedFilePath(HttpPostedFileBase file)
        {
            // Define destination
            var serverPath = HttpContext.Server.MapPath(TempFolder);
            if (Directory.Exists(serverPath) == false)
            {
                Directory.CreateDirectory(serverPath);
            }

            var originalPath = HttpContext.Server.MapPath(OriginalPath);
            if (Directory.Exists(originalPath) == false)
            {
                Directory.CreateDirectory(originalPath);
            }
            // Generate unique file name
            var fileName = Path.GetFileName(file.FileName);
            fileName = SaveTemporaryAvatarFileImage(file, serverPath, fileName, originalPath);

            // Clean up old files after every save
            CleanUpTempFolder(1);
            return Path.Combine(TempFolder, fileName);
        }

        private string SaveTemporaryAvatarFileImage(HttpPostedFileBase file, string serverPath, string fileName, string originalPath)
        {
            var img = new WebImage(file.InputStream);
            var imageName = "collector_" + DateTime.Now.ToFileTimeUtc().ToString() + "_" + fileName;

            var originalFileLocation = Path.Combine(originalPath, imageName);
            if (System.IO.File.Exists(originalFileLocation))
            {
                System.IO.File.Delete(originalFileLocation);
            }
            img.Save(originalFileLocation);
            var originalFileName = Path.Combine(OriginalPath, imageName);
            Session["OriginalFileName"] = originalFileName.Replace("\\", "/");

            int AvatarScreenWidth = 700;
            var ratio = img.Height / (double)img.Width;
            img.Resize(AvatarScreenWidth, (int)(AvatarScreenWidth * ratio));

            var fullFileName = Path.Combine(serverPath, imageName);
            if (System.IO.File.Exists(fullFileName))
            {
                System.IO.File.Delete(fullFileName);
            }

            img.Save(fullFileName);
            
            return Path.GetFileName(img.FileName);
        }

        private void CleanUpTempFolder(int hoursOld)
        {
            try
            {
                var currentUtcNow = DateTime.UtcNow;
                var serverPath = HttpContext.Server.MapPath("/Temp");
                if (!Directory.Exists(serverPath)) return;
                var fileEntries = Directory.GetFiles(serverPath);
                foreach (var fileEntry in fileEntries)
                {
                    var fileCreationTime = System.IO.File.GetCreationTimeUtc(fileEntry);
                    var res = currentUtcNow - fileCreationTime;
                    if (res.TotalHours > hoursOld)
                    {
                        System.IO.File.Delete(fileEntry);
                    }
                }
            }
            catch
            {
                // Deliberately empty.
            }
        }

        public ActionResult _AddDetail(int id)
        {
            CollectorRespository collectorRespository = new CollectorRespository();
            Collector collector = collectorRespository.GetCollector(User.Identity.GetUserId());
            Collectible model = new Collectible();
            model.CollectionId = id;
            model.CollectorId = collector.CollectorId;
            CategoriesRespository categoriesRespository = new CategoriesRespository(); 
            List<Category> categories = categoriesRespository.GetAllCategories();
            ViewBag.CategoryId = new SelectList(categories, "CategoryId", "Name");
            CollectionsRespository collectionsRespository = new CollectionsRespository();
            ViewBag.CollectorId = new SelectList(collectionsRespository.GetCollections(collector.CollectorId), "CollectionId", "Name");
            return PartialView("_AddDetail", model);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult _AddDetail(Collectible model)
        {
            CollectorRespository collectorRespository = new CollectorRespository();
            Collector collector = collectorRespository.GetCollector(User.Identity.GetUserId());
            model.CollectorId = collector.CollectorId;
            //model.CollectionId = (int)Session["UploadCollectionId"];
            model.ThumbImage = Session["ImageFileName"].ToString();
            model.NormalImage = Session["ImageFileName"].ToString();
            model.CreatedDate = DateTime.Now;
            CollectiblesRespository collectiblesRepository = new CollectiblesRespository();
            int collectibleId = collectiblesRepository.InsertCollectible(model);
            if (model.CollectionId == 0)
            {
                return RedirectToAction("CollectibleDetail", "Home", new { id = collectibleId });
            }
            else
            { 
                return RedirectToAction("CollectionDetail", "Home", new { id = model.CollectionId });
            }
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult _AddDetail2(Collectible model)
        {
            CollectorRespository collectorRespository = new CollectorRespository();
            Collector collector = collectorRespository.GetCollector(User.Identity.GetUserId());
            model.CollectorId = collector.CollectorId;
            model.OriginalImage = Session["OriginalFileName"].ToString();
            model.ThumbImage = Session["ThumbFileName"].ToString();
            model.NormalImage = Session["ImageFileName"].ToString();
            model.CreatedDate = DateTime.Now;
            CollectiblesRespository collectiblesRepository = new CollectiblesRespository();
            int collectibleId = collectiblesRepository.InsertCollectible(model);
            if (model.CollectionId == 0)
            {
                return RedirectToAction("CollectibleDetail", "Home", new { id = collectibleId });
            }
            else
            {
                return RedirectToAction("CollectionDetail", "Home", new { id = model.CollectionId });
            }
        }

    }

}