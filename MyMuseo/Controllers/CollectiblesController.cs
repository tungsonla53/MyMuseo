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

namespace MyMuseo.Controllers
{
    [Authorize]
    public class CollectiblesController : BaseController
    {

        public CollectiblesController() : base()
        {
        }

        public ViewResult Index()
        {
            CollectiblesViewModel model = new CollectiblesViewModel();
            model.Collectibles = _collectiblesRespository.GetCollectibles(_collector.CollectorId);
            model.Collections = new SelectList(_collectionsRespository.GetCollections(_collector.CollectorId), "CollectionId", "Name");
            return View(model);
        }

        [HttpPost]
        public ViewResult Index(FormCollection form)
        {
            CollectiblesViewModel model = new CollectiblesViewModel();
            int collectionId = form[0] == "" ? 0 : Convert.ToInt32(form[0]);
            if (collectionId == 0)
            {
                model.Collectibles = _collectiblesRespository.GetCollectibles(_collector.CollectorId);
            }
            else
            {
                model.Collectibles = _collectiblesRespository.GetCollectibles(_collector.CollectorId).Where(i => i.CollectionId == collectionId).OrderBy(i => i.DisplayOrder);
            }
            model.Collections = new SelectList(_collectionsRespository.GetCollections(_collector.CollectorId), "CollectionId", "Name", collectionId);
            return View(model);
        }
        
        public ActionResult ReorderItems()
        {
            List<Collectible> items = _collectiblesRespository.GetCollectiblesByDisplayOrder(_collector.CollectorId);
            ViewBag.ItemsList = items;
            return View();
        }

        [HttpPost]
        public ActionResult ReOrder(FormCollection form)
        {
            string itemData = form[0].ToString();
            string[] itemIds = itemData.Split('/');
            for (int i = 0; i < itemIds.Length; i++)
            {
                if (!String.IsNullOrEmpty(itemIds[i]))
                {
                    _collectiblesRespository.UpdateDisplayOrder(Convert.ToInt32(itemIds[i]), i+1);
                }
            }
            return Redirect("~/Home/MyCollectibles/");
        }


        [HttpGet]
        public JsonResult CollectibleLookup()
        {
            var list = _collectiblesRespository.GetAllCollectibles(1000, "");
            return Json(list, JsonRequestBehavior.AllowGet);
        }
    }
}