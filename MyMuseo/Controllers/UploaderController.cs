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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net;
using System.ComponentModel.DataAnnotations;

namespace MyMuseo.Controllers
{
    public class UploaderController : BaseController
    {
        
        public ActionResult Index()
        {
            var images = new ImagesModel();
            //Read out files from the files directory
            var files = Directory.GetFiles(Server.MapPath("~/Content/uploads/img"));
            //Add them to the model
            foreach (var file in files)
                images.Images.Add(Path.GetFileName(file));

            return View(images);
        }

        

        public ActionResult UploadImage()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadImage(Collectible collectible, IEnumerable<HttpPostedFileBase> files)
        {
            string base64 = Request.Form["image-data"];

            string filePath = ProcessImage(base64);
            collectible.NormalImage = filePath;

            if (ModelState.IsValid)
            {
                
                return RedirectToAction("Index");
            }

            return View(collectible);
        }

        public ActionResult ProfileInfo()
        {
            ViewBag.ImageRequirements = this.GetCustomContent("Image Requirements");
            ViewBag.SmartphoneGuide = this.GetCustomContent("Smartphone Guide");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProfileInfo(Collector model, IEnumerable<HttpPostedFileBase> files)
        {
            string base64 = Request.Form["image-data"];
            string filePath = ProcessImage(base64);
            CollectorRespository repository = new CollectorRespository();
            string userId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            Collector collector = repository.GetCollector(userId);
            repository.UpdateCollectorProfileImage(collector.CollectorId, filePath);
            return Redirect("~/Collectors/ProfileInfo/" + collector.CollectorId);
        }

        public ActionResult ProfileImage()
        {
            ViewBag.ImageRequirements = this.GetCustomContent("Image Requirements");
            ViewBag.SmartphoneGuide = this.GetCustomContent("Smartphone Guide");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProfileImage(IEnumerable<HttpPostedFileBase> files)
        {
            string base64 = Request.Form["image-data"];
            string filePath = ProcessImage(base64);
            CollectorRespository repository = new CollectorRespository();
            string userId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            Collector collector = repository.GetCollector(userId);
            repository.UpdateCollectorProfileImage(collector.CollectorId, filePath);
            return Redirect("~/Home/CollectorDetail/"+ collector.CollectorId);
        }

        public ActionResult FeaturedImage()
        {
            CollectiblesRespository collectiblesRepository = new CollectiblesRespository();
            CollectorRespository collectorRepository = new CollectorRespository();
            string userId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            Collector collector = collectorRepository.GetCollector(userId);
            ViewBag.CollectiblesList = collectiblesRepository.GetCollectibles(collector.CollectorId);
            ViewBag.ImageRequirements = this.GetCustomContent("Image Requirements");
            ViewBag.SmartphoneGuide = this.GetCustomContent("Smartphone Guide");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FeaturedImage(Collector model, HttpPostedFileBase file)
        {
            string originalPath = "";
            if (file != null && file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                string newFileName = DateTime.Now.ToFileTimeUtc().ToString() + "_" + fileName;
                var path = Path.Combine(Server.MapPath("~/Content/uploads"), "collector_" + newFileName);
                this.SaveOriginalImage(file, path);
                originalPath = "/Content/uploads/collector_" + newFileName;
            }
            string base64 = Request.Form["image-data"];
            string filePath = ProcessImage(base64);
            CollectorRespository repository = new CollectorRespository();
            string userId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            Collector collector = repository.GetCollector(userId);
            CollectiblesRespository collectiblesRepository = new CollectiblesRespository();
            Collectible newCollectible = new Collectible();
            newCollectible.CollectorId = collector.CollectorId;
            newCollectible.CollectionId = 0;
            newCollectible.OriginalImage = originalPath;
            newCollectible.NormalImage = originalPath;
            newCollectible.ThumbImage = filePath;
            newCollectible.Description = "";
            newCollectible.ArtistId = 0;
            newCollectible.DisplayOrder = 1;
            newCollectible.CreatedDate = DateTime.Now;
            int itemId = collectiblesRepository.InsertCollectible(newCollectible);
            repository.UpdateCollectorFeaturedItemId(collector.CollectorId, itemId);
            return Redirect("~/Collectors/EditCollectibleDetail/" + itemId);
        }

        [HttpPost]
        public ActionResult FeaturedImageSelection(Collector model, IEnumerable<HttpPostedFileBase> files)
        {
            CollectorRespository repository = new CollectorRespository();
            string userId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            Collector collector = repository.GetCollector(userId);
            repository.UpdateCollectorFeaturedItemId(collector.CollectorId, model.FeaturedItemId);
            return Redirect("~/Home/CollectorDetail/" + model.CollectorId);
        }


        public ActionResult CollectionFeaturedImage(int id)
        {
            CollectiblesRespository collectiblesRepository = new CollectiblesRespository();
            CollectionsRespository collectionsRepository = new CollectionsRespository();
            Collection collection = collectionsRepository.GetCollection(id);
            List<Collectible> collectiblesList = collectiblesRepository.GetCollectibles(collection.CollectorId);
            List<Collectible> availableList = new List<Collectible>();
            foreach (Collectible item in collectiblesList)
            {
                if (item.CollectionId == 0 || item.CollectionId == collection.CollectionId)
                {
                    availableList.Add(item);
                }
            }
            ViewBag.CollectiblesList = availableList;
            ViewBag.ImageRequirements = this.GetCustomContent("Image Requirements");
            ViewBag.SmartphoneGuide = this.GetCustomContent("Smartphone Guide");
            return View(collection);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CollectionFeaturedImage(Collection model, HttpPostedFileBase file)
        {
            string originalPath = "";
            if (file != null && file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                string newFileName = DateTime.Now.ToFileTimeUtc().ToString() + "_" + fileName;
                var path = Path.Combine(Server.MapPath("~/Content/uploads"), "collector_" + newFileName);
                this.SaveOriginalImage(file, path);
                originalPath = "/Content/uploads/collector_" + newFileName;
            }
            string base64 = Request.Form["image-data"];
            string filePath = ProcessImage(base64);
            CollectiblesRespository collectiblesRepository = new CollectiblesRespository();
            Collectible newCollectible = new Collectible();
            newCollectible.CollectorId = model.CollectorId;
            newCollectible.CollectionId = model.CollectionId;
            newCollectible.OriginalImage = originalPath;
            newCollectible.NormalImage = originalPath;
            newCollectible.ThumbImage = filePath;
            newCollectible.Description = "";
            newCollectible.ArtistId = 0;
            newCollectible.DisplayOrder = 1;
            newCollectible.CreatedDate = DateTime.Now;
            int itemId = collectiblesRepository.InsertCollectible(newCollectible);
            CollectionsRespository repository = new CollectionsRespository();
            repository.UpdateCollectionImage(model.CollectionId, filePath, originalPath);
            repository.UpdateCollectionFeaturedItemId(model.CollectionId, itemId);
            return Redirect("~/Collectors/EditCollectibleDetail/" + itemId);
        }

        [HttpPost]
        public ActionResult CollectionFeaturedImageSelection(Collection model, IEnumerable<HttpPostedFileBase> files)
        {
            CollectiblesRespository collectiblesRepository = new CollectiblesRespository();
            Collectible item = collectiblesRepository.GetCollectible(model.FeaturedItemId);
            if (item.CollectionId == 0)
            {
                item.CollectionId = model.CollectionId;
                collectiblesRepository.UpdateCollectible(item);
            }
            CollectionsRespository repository = new CollectionsRespository();
            repository.UpdateCollectionImage(model.CollectionId, item.ThumbImage, item.NormalImage);
            repository.UpdateCollectionFeaturedItemId(model.CollectionId, model.FeaturedItemId);
            return Redirect("~/Home/CollectionDetail/" + model.CollectionId);
        }


        public ActionResult UpdateCollectibleImage(int id)
        {
            CollectiblesRespository repository = new CollectiblesRespository();
            Collectible model = repository.GetCollectible(id);
            ViewBag.ImageRequirements = this.GetCustomContent("Image Requirements");
            ViewBag.SmartphoneGuide = this.GetCustomContent("Smartphone Guide");
            return View(model);
        }

        
        [HttpPost, ValidateInput(false)]
        public ActionResult UpdateCollectibleImage(Collectible model, HttpPostedFileBase file)
        {
            string originalPath = "";
            
            if (file != null && file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                string newFileName = DateTime.Now.ToFileTimeUtc().ToString() + "_" + fileName;
                var path = Path.Combine(Server.MapPath("~/Content/uploads"), "collector_" + newFileName);
                this.SaveOriginalImage(file, path);
                originalPath = "/Content/uploads/collector_" + newFileName;
            }
            
            string base64 = Request.Form["image-data"];
            string thumbPath = ProcessImage(base64);
            CollectiblesRespository repository = new CollectiblesRespository();
            repository.UpdateCollectibleImage(model.CollectibleId, thumbPath, originalPath);
            return Redirect("~/Home/CollectibleDetail/" + model.CollectibleId);
        }

        public ActionResult UpdateGroupImage(int id)
        {
            Group model = _collectorRespository.GetGroup(id);
            ViewBag.ImageRequirements = this.GetCustomContent("Image Requirements");
            ViewBag.SmartphoneGuide = this.GetCustomContent("Smartphone Guide");
            return View(model);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult UpdateGroupImage(Group model, HttpPostedFileBase file)
        {
            string base64 = Request.Form["image-data"];
            string thumbPath = ProcessImage(base64);
            _collectorRespository.UpdateGroupImage(model.GroupId, thumbPath);
            return Redirect("~/Groups/Photos/" + model.GroupId);
        }

        public ActionResult UpdateCollectionImage(int id)
        {
            CollectionsRespository repository = new CollectionsRespository();
            Collection model = repository.GetCollection(id);
            ViewBag.ImageRequirements = this.GetCustomContent("Image Requirements");
            ViewBag.SmartphoneGuide = this.GetCustomContent("Smartphone Guide");
            return View(model);
        }


        [HttpPost, ValidateInput(false)]
        public ActionResult UpdateCollectionImage(Collection model, HttpPostedFileBase file)
        {

            string originalPath = "";

            if (file != null && file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                string newFileName = DateTime.Now.ToFileTimeUtc().ToString() + "_" + fileName;
                var path = Path.Combine(Server.MapPath("~/Content/uploads"), "collector_" + newFileName);
                this.SaveOriginalImage(file, path);
                originalPath = "/Content/uploads/collector_" + newFileName;
            }

            string base64 = Request.Form["image-data"];
            string thumbPath = ProcessImage(base64);
            CollectionsRespository repository = new CollectionsRespository();
            repository.UpdateCollectionImage(model.CollectionId, thumbPath, originalPath);
            return Redirect("~/Home/CollectionDetail/" + model.CollectionId);
        }

        public ActionResult AddCollectibleItem(int id)
        {
            CollectorRespository collectorRespository = new CollectorRespository();
            Collector collector = collectorRespository.GetCollector(User.Identity.GetUserId());
            Collectible model = new Collectible();
            model.CollectionId = id;
            model.CollectorId = collector.CollectorId;
            model.ArtistId = 0;
            model.Price = 0.00M;
            model.OfferPrice = 0.00M;
            CategoriesRespository categoriesRespository = new CategoriesRespository();
            List<Category> categories = categoriesRespository.GetAllCategories();
            categories.Add(new Category { CategoryId = 0, Name = "Other" });
            ViewBag.CategoryId = new SelectList(categories, "CategoryId", "Name");
            CollectionsRespository collectionsRespository = new CollectionsRespository();
            List<Collection> collectionList = collectionsRespository.GetCollections(collector.CollectorId);
            collectionList.Add(new Collection { CollectionId = 0, Name = "N/A" });
            ViewBag.CollectionId = new SelectList(collectionList, "CollectionId", "Name", model.CollectionId);
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

            ViewBag.ImageRequirements = this.GetCustomContent("Image Requirements");
            ViewBag.SmartphoneGuide = this.GetCustomContent("Smartphone Guide");
            return View(model);
        }


        [HttpPost, ValidateInput(false)]
        //public ActionResult AddCollectibleItem(Collectible model, HttpPostedFileBase file)
        public ActionResult AddCollectibleItem(Collectible model, IEnumerable<HttpPostedFileBase> files)
        {
            if (!String.IsNullOrEmpty(model.OtherCategory))
            {
                CategoriesRespository categoriesRepository = new CategoriesRespository();
                Category categoryModel = new Category();
                categoryModel.Name = model.OtherCategory;
                categoryModel.Description = model.OtherCategory;
                categoryModel.ParentCategoryId = 0;
                categoryModel.CreatedOnUtc = DateTime.UtcNow;
                model.CategoryId = categoriesRepository.InsertCategory(categoryModel);
            }
            string originalPath = "";
            string audioPath = "";
            foreach (var file in files)
            {
                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    if (fileName.EndsWith(".mp3"))
                    {
                        string newFileName = DateTime.Now.ToFileTimeUtc().ToString() + "_" + fileName;
                        var path = Path.Combine(Server.MapPath("~/Content/uploads"), "audio_" + newFileName);
                        file.SaveAs(path);
                        audioPath = "/Content/uploads/audio_" + newFileName;
                    }
                    else
                    {
                        string newFileName = DateTime.Now.ToFileTimeUtc().ToString() + "_" + fileName;
                        var path = Path.Combine(Server.MapPath("~/Content/uploads"), "collector_" + newFileName);
                        this.SaveOriginalImage(file, path);
                        originalPath = "/Content/uploads/collector_" + newFileName;
                    }
                }
            }
            string base64 = Request.Form["image-data"];
            string thumbPath = ProcessImage(base64);

            model.OriginalImage = originalPath;
            model.NormalImage = originalPath;
            model.ThumbImage = thumbPath;
            model.AudioFile = audioPath;
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
        public ActionResult AddGroup(Group model, HttpPostedFileBase file)
        {
            string originalPath = "";

            if (file != null && file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                string newFileName = DateTime.Now.ToFileTimeUtc().ToString() + "_" + fileName;
                var path = Path.Combine(Server.MapPath("~/Content/uploads"), "group_" + newFileName);
                this.SaveOriginalImage(file, path);
                originalPath = "/Content/uploads/group_" + newFileName;
            }

            string base64 = Request.Form["image-data"];
            string thumbPath = ProcessImage(base64);

            model.ImageBanner = originalPath;
            model.Image = thumbPath;
            model.DisplayOrder = 1;
            model.CreatedOnUtc = DateTime.Now;
            model.UpdatedOnUtc = DateTime.Now;

            CollectorRespository collectorRepository = new CollectorRespository();
            int groupId = collectorRepository.InsertGroup(model);

            Collector collector = collectorRepository.GetCollector(User.Identity.GetUserId());
            GroupMember member = new GroupMember();
            member.GroupId = groupId;
            member.CollectorId = collector.CollectorId;
            member.GroupRoleId = 1;
            member.CreatedDate = DateTime.Now;
            collectorRepository.JoinGroup(member);
            return Redirect("~/Groups/Board/" + groupId);
        }

        public ActionResult AddGroup()
        {
            Group model = new Group();
            return View(model);
        }

        public ActionResult AddBanner()
        {
            Banner model = new Banner();
            return View(model);
        }


        [HttpPost, ValidateInput(false)]
        public ActionResult AddBanner(Banner model, IEnumerable<HttpPostedFileBase> files)
        {
            string originalPath = "";
            string mobilePath = "";
            var index = 0;
            foreach (var file in files)
            {
                if (file != null && file.ContentLength > 0)
                {
                    if (index == 0)
                    { 
                        var fileName = Path.GetFileName(file.FileName);
                        string newFileName = DateTime.Now.ToFileTimeUtc().ToString() + "_" + fileName;
                        var path = Path.Combine(Server.MapPath("~/Content/uploads"), "banner_" + newFileName);
                        this.SaveOriginalImage(file, path);
                        originalPath = "/Content/uploads/banner_" + newFileName;
                    }
                    if (index == 1)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        string newFileName = DateTime.Now.ToFileTimeUtc().ToString() + "_" + fileName;
                        var path = Path.Combine(Server.MapPath("~/Content/uploads"), "banner_m_" + newFileName);
                        this.SaveOriginalImage(file, path);
                        mobilePath = "/Content/uploads/banner_m_" + newFileName;
                    }  
                }
                index++;
            }
            if (files.Count() < 1)
            {
                return View();
            }
            model.Image = originalPath;
            model.ImageMobile = mobilePath;
            model.DisplayOrder = 99;
            model.CreatedOnUtc = DateTime.Now;
            model.UpdatedOnUtc = DateTime.Now;

            BannersRepository bannerRepository = new BannersRepository();
            int bannerId = bannerRepository.InsertBanner(model);
            return Redirect("~/Admin/Banners/");
        }

        public ActionResult UpdateBanner(int id)
        {
            BannersRepository bannerRepository = new BannersRepository();
            Banner model = bannerRepository.GetBanner(id);
            return View(model);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult UpdateBanner(Banner model, IEnumerable<HttpPostedFileBase> files)
        {
            string originalPath = "";
            string mobilePath = "";
            var index = 0;
            foreach (var file in files)
            {
                if (file != null && file.ContentLength > 0)
                {
                    if (index == 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        string newFileName = DateTime.Now.ToFileTimeUtc().ToString() + "_" + fileName;
                        var path = Path.Combine(Server.MapPath("~/Content/uploads"), "banner_" + newFileName);
                        this.SaveOriginalImage(file, path);
                        originalPath = "/Content/uploads/banner_" + newFileName;
                    }
                    if (index == 1)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        string newFileName = DateTime.Now.ToFileTimeUtc().ToString() + "_" + fileName;
                        var path = Path.Combine(Server.MapPath("~/Content/uploads"), "banner_m_" + newFileName);
                        this.SaveOriginalImage(file, path);
                        mobilePath = "/Content/uploads/banner_m_" + newFileName;
                    }
                }
                index++;
            }
            if (!String.IsNullOrEmpty(originalPath))
            {
                model.Image = originalPath;
            }
            if (!String.IsNullOrEmpty(mobilePath))
            {
                model.ImageMobile = mobilePath;
            }
            model.UpdatedOnUtc = DateTime.Now;

            BannersRepository bannersRepository = new BannersRepository();
            bannersRepository.UpdateBanner(model);
            return Redirect("~/Admin/Banners/");
        }

        private string ProcessImage(string croppedImage)
        {
            string filePath = String.Empty;
            try
            {
                string base64 = croppedImage;
                byte[] bytes = Convert.FromBase64String(base64.Split(',')[1]);
                filePath = "/Content/uploads/thumb/Col-" + Guid.NewGuid() + ".png";
                using (FileStream stream = new FileStream(Server.MapPath(filePath), FileMode.Create))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();
                }
            }
            catch (Exception ex)
            {
                string st = ex.Message;
            }

            return filePath;
        }

        private void SaveOriginalImage (HttpPostedFileBase fileUpload, string path)
        {
            byte[] imageData = new byte[fileUpload.ContentLength];
            fileUpload.InputStream.Read(imageData, 0, fileUpload.ContentLength);

            MemoryStream ms = new MemoryStream(imageData);
            Image originalImage = Image.FromStream(ms);

            if (originalImage.PropertyIdList.Contains(0x0112))
            {  
                int rotationValue = originalImage.GetPropertyItem(0x0112).Value[0];
                RotateFlipType rotateFlipType = GetOrientationToFlipType(rotationValue);
                if (rotateFlipType == RotateFlipType.RotateNoneFlipNone)
                {
                    fileUpload.SaveAs(path);
                }
                else
                {
                    originalImage.RotateFlip(rotateFlipType);
                    originalImage.RemovePropertyItem(0x0112);
                    originalImage.Save(path);
                }
            }
            else
            {
                fileUpload.SaveAs(path);
            }
        }
        
        private static RotateFlipType GetOrientationToFlipType(int orientationValue)
        {
            RotateFlipType rotateFlipType = RotateFlipType.RotateNoneFlipNone;

            switch (orientationValue)
            {
                case 1:
                    rotateFlipType = RotateFlipType.RotateNoneFlipNone;
                    break;
                case 2:
                    rotateFlipType = RotateFlipType.RotateNoneFlipX;
                    break;
                case 3:
                    rotateFlipType = RotateFlipType.Rotate180FlipNone;
                    break;
                case 4:
                    rotateFlipType = RotateFlipType.Rotate180FlipX;
                    break;
                case 5:
                    rotateFlipType = RotateFlipType.Rotate90FlipX;
                    break;
                case 6:
                    rotateFlipType = RotateFlipType.Rotate90FlipNone;
                    break;
                case 7:
                    rotateFlipType = RotateFlipType.Rotate270FlipX;
                    break;
                case 8:
                    rotateFlipType = RotateFlipType.Rotate270FlipNone;
                    break;
                default:
                    rotateFlipType = RotateFlipType.RotateNoneFlipNone;
                    break;
            }

            return rotateFlipType;
        }

    }
}
