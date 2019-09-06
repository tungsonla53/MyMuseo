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
    public class CollectionsController : Controller
    {
        private CollectorRespository _collectorRespository;
        private CollectionsRespository _collectionsRespository;
        private CategoriesRespository _categoriesRespository;
        private CommentsRespository _commentsRespository;
        private Collector _collector;

        public CollectionsController()
        {
            _collectorRespository = new CollectorRespository();
            _collectionsRespository = new CollectionsRespository();
            _categoriesRespository = new CategoriesRespository();
            _commentsRespository = new CommentsRespository();
            string userId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            _collector = _collectorRespository.GetCollector(userId);
        }

        //
        // GET: /Admin/

        public ViewResult Index()
        {
            CollectionsViewModel model = new CollectionsViewModel();
            model.Collections = _collectionsRespository.GetCollections(_collector.CollectorId);
            return View(model);
        }

        //
        // GET: /Admin/Details/5

        public ViewResult Details(int id)
        {
            Collection collection = _collectionsRespository.GetCollection(id);
            List<Comment> commentList = _commentsRespository.GetCollectionComments(id);
            ViewBag.CommentList = commentList;
            ViewBag.GetUserName = new Func<int, string>(GetCollectorName); 
            return View(collection);
        }

        [HttpPost]
        public ViewResult Details(FormCollection form)
        {
            int collectionId = Convert.ToInt32(form[0]);
            string comments = form[1];
            int commentId = 0;
            if (form[2].ToString() != String.Empty)
            {
                commentId = Convert.ToInt32(form[2]);
            }
            Comment commentModel = new Comment();
            commentModel.CollectionId = collectionId;
            commentModel.PostByCollectorId = _collector.CollectorId;
            commentModel.ParentId = commentId;
            commentModel.CommentText = comments;
            commentModel.CreatedDate = DateTime.Now;
            _commentsRespository.InsertComment(commentModel);
            Collection model = _collectionsRespository.GetCollection(collectionId);
            List<Comment> commentList = _commentsRespository.GetCollectionComments(collectionId);
            ViewBag.CommentList = commentList;
            ViewBag.GetUserName = new Func<int, string>(GetCollectorName);
            return View(model);
        }

        //
        // GET: /Collections/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Collections/Edit/5
        [HttpPost]
        public ActionResult Edit()
        {
            return View();
        }

        //
        // GET: /Admin/Delete/5
 
        public ActionResult Delete(int id)
        {
            Collector collector = _collectorRespository.GetCollector(id);
            return View(collector);
        }

        //
        // POST: /Admin/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            _collectorRespository.DeleteCollector(id);
            return RedirectToAction("Index");
        }

        public string GetCollectorName(int collectorId)
        {
            Collector collector = _collectorRespository.GetCollector(collectorId);
            return String.Format("{0} {1}",collector.FirstName, collector.LastName);
        }
    }
}