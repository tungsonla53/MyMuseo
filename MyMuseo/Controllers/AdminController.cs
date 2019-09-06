using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyMuseo.Models;
using MyMuseo.DataService;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace MyMuseo.Controllers
{
    [Authorize]
    public class AdminController : BaseController
    {
        public ActionResult Index()
        {
            return Redirect("~/Admin/RegisteredUsers/");
        }
        public ActionResult RegisteredUsers()
        {
            CollectorRespository collectorRespository = new CollectorRespository();
            var collectorsList = collectorRespository.GetCollectors(10000, "DESC");
            List<CollectorViewModel> detailsList = new List<CollectorViewModel>();
            AddressRepository addressRespository = new AddressRepository();
            foreach (Collector collector in collectorsList)
            {
                CollectorViewModel item = new CollectorViewModel();
                item.Collector = collector;
                try
                {
                    item.AddressInfo = addressRespository.GetCollectorAddress(collector.CollectorId);
                }
                catch(Exception e)
                {
                }
                if (item.AddressInfo == null)
                {
                    item.AddressInfo = new AddressInfo();
                    item.AddressInfo.CountryId = 0;
                }
                item.Email = GetUserEmail(collector.UserId);
                detailsList.Add(item);
            }

            ViewBag.CollectorsDetailsList = detailsList;
            ViewBag.GetCountryName = new Func<int, string>(GetCountryName);
            ViewBag.GroupId = new SelectList(collectorRespository.GetGroups(), "GroupId", "Name");
            return View();
        }

        public ActionResult Mailbox()
        {
            CollectorRespository collectorRespository = new CollectorRespository();
            ViewBag.Messages = collectorRespository.GetMessages(0);
            ViewBag.GetUserName = new Func<int, string>(GetCollectorName);
            return View();
        }

        public ActionResult Marketing()
        {
            return View();
        }

        public ActionResult Settings()
        {
            return View();
        }

        public ActionResult CustomPages()
        {
            CollectorRespository repository = new CollectorRespository();
            List<ContentModel> contents = repository.GetContents();
            ViewBag.ContentsList = contents;
            return View();
        }

        public ActionResult EmailTemplates()
        {
            CollectorRespository repository = new CollectorRespository();
            List<TemplateModel> templates = repository.GetEmailTemplates();
            ViewBag.TemplatesList = templates;
            return View();
        }

        public ActionResult HomepageCarousel()
        {
            return View();
        }

        public ActionResult SitemapGenerator()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetUsers()
        {
            CollectorRespository repository = new CollectorRespository();
            List<Collector> data = repository.GetCollectors(10000, "DESC");
            // To keep Datatable loading happy
            var jsonData = new { data };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GroupAdmin(FormCollection form)
        {
            int collectorId = Convert.ToInt16(form[0]);
            int groupId = Convert.ToInt16(form[1]);
            CollectorRespository collectorRespository = new CollectorRespository();
            GroupMember member = new GroupMember();
            member.CollectorId = collectorId;
            member.GroupId = groupId;
            member.GroupRoleId = 1;
            member.CreatedDate = DateTime.Now;
            collectorRespository.JoinGroup(member);
            return Redirect("~/Groups/Members/" + groupId);
        }

        [HttpPost]
        public ActionResult SiteAdmin(FormCollection form)
        {
            int collectorId = Convert.ToInt16(form[0]);
            CollectorRespository collectorRespository = new CollectorRespository();
            collectorRespository.SiteAdmin(collectorId);
            return Redirect("~/Admin/RegisteredUsers/");
        }

        [HttpPost]
        public ActionResult HideProfile(FormCollection form)
        {
            int collectorId = Convert.ToInt16(form[0]);
            CollectorRespository collectorRespository = new CollectorRespository();
            collectorRespository.HideProfile(collectorId);
            return Redirect("~/Admin/RegisteredUsers/");
        }

        [HttpPost]
        public ActionResult ShowProfile(FormCollection form)
        {
            int collectorId = Convert.ToInt16(form[0]);
            CollectorRespository collectorRespository = new CollectorRespository();
            collectorRespository.ShowProfile(collectorId);
            return Redirect("~/Admin/RegisteredUsers/");
        }

        [HttpPost]
        public ActionResult DeleteCollector(FormCollection form)
        {
            int collectorId = Convert.ToInt16(form[0]);
            CollectorRespository collectorRespository = new CollectorRespository();
            collectorRespository.DeleteCollector(collectorId);
            collectorRespository.CleanUpCollector();
            return Redirect("~/Admin/RegisteredUsers/");
        }

        [HttpPost]
        public ActionResult DeleteMessage(FormCollection form)
        {
            int messageId = Convert.ToInt16(form[0]);
            _collectorRespository.DeleteMessage(messageId);
            return Redirect("~/Admin/Mailbox/");
        }

        public ActionResult EditContent(int id)
        {
            ContentModel model = new ContentModel();
            if (id == 0)
            {
                model.ContentId = id;
            }
            else
            {
                CollectorRespository collectorRespository = new CollectorRespository();
                model = collectorRespository.GetContent(id);
            }
            return View(model);
        }

        public ActionResult EditEmailTemplate(int id)
        {
            TemplateModel model = new TemplateModel();
            if (id == 0)
            {
                model.TemplateId = id;
            }
            else
            {
                CollectorRespository collectorRespository = new CollectorRespository();
                model = collectorRespository.GetTemplate(id);
            }
            return View(model);
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditContent(ContentModel model)
        {
            CollectorRespository collectorRespository = new CollectorRespository();
            if (model.ContentId == 0)
            {
                model.CreatedDate = DateTime.Now;
                collectorRespository.InsertContent(model);
            }
            else
            {
                collectorRespository.UpdateContent(model);
            }
            return Redirect("~/Admin/CustomPages/");
        }


        [HttpPost, ValidateInput(false)]
        public ActionResult EditEmailTemplate(TemplateModel model)
        {
            CollectorRespository collectorRespository = new CollectorRespository();
            if (model.TemplateId == 0)
            {
                model.TemplateTypeId = 1;
                model.CreatedDate = DateTime.Now;
                collectorRespository.InsertTemplate(model);
            }
            else
            {
                collectorRespository.UpdateTemplate(model);
            }
            return Redirect("~/Admin/EmailTemplates/");
        }
        public ActionResult Groups()
        {
            CollectorRespository repository = new CollectorRespository();
            List<Group> groups = repository.GetGroups();
            ViewBag.GroupsList = groups;
            return View();
        }

        [HttpPost]
        public ActionResult DeleteGroup(FormCollection form)
        {
            int groupId = Convert.ToInt16(form[0]);
            CollectorRespository collectorRespository = new CollectorRespository();
            collectorRespository.DeleteGroup(groupId);
            return Redirect("~/Admin/Groups/");
        }

        [HttpPost]
        public ActionResult ReorderGroup(FormCollection form)
        {
            string groupData = form[0].ToString();
            string[] groupNames = groupData.Split('/');
            for (int i=0; i< groupNames.Length; i++)
            {
                if (!String.IsNullOrEmpty(groupNames[i]))
                {
                    _collectorRespository.UpdateGroupOrder(groupNames[i], i);
                }
            }
            return Redirect("~/Admin/Groups/");
        }

        public ActionResult Banners()
        {
            BannersRepository repository = new BannersRepository();
            List<Banner> banners = repository.GetAllBanners();
            ViewBag.BannersList = banners;
            return View();
        }

        [HttpPost]
        public ActionResult ReorderBanner(FormCollection form)
        {
            BannersRepository repository = new BannersRepository();
            string bannerData = form[0].ToString();
            string[] bannerIds = bannerData.Split('/');
            for (int i = 0; i < bannerIds.Length; i++)
            {
                if (!String.IsNullOrEmpty(bannerIds[i]))
                {
                    repository.UpdateBannerOrder(Convert.ToInt32(bannerIds[i]), i);
                }
            }
            return Redirect("~/Admin/Banners/");
        }

        [HttpPost]
        public ActionResult DeleteBanner(FormCollection form)
        {
            int bannerId = Convert.ToInt16(form[0]);
            BannersRepository bannerRepository = new BannersRepository();
            bannerRepository.DeleteBanner(bannerId);
            return Redirect("~/Admin/Banners/");
        }
    }
}