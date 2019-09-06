using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyMuseo.Models;
using MyMuseo.DataService;
using Microsoft.AspNet.Identity;
using System.Net.Mail;
using System.IO;

namespace MyMuseo.Controllers
{
    public class GroupsController : BaseController
    {
        // GET: Groups
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult MyGroups()
        {
            Collector collector = _collectorRespository.GetCollector(User.Identity.GetUserId());
            List<Group> allGroups = _collectorRespository.GetGroups();
            List<Group> adminGroups = new List<Group>();
            List<Group> memberGroups = new List<Group>();
            foreach (Group group in allGroups)
            {
                List<GroupMember> members = _collectorRespository.GetGroupMembers(group.GroupId);
                foreach (GroupMember member in members)
                {
                    if (member.CollectorId == collector.CollectorId)
                    {
                        if (member.GroupRoleId == 1)
                        {
                            adminGroups.Add(group);
                        }
                        else
                        {
                            memberGroups.Add(group);
                        }
                    }
                }
            }
            ViewBag.AdminGroups = adminGroups;
            ViewBag.MemberGroups = memberGroups;
            ViewBag.GetGroupMembersCount = new Func<int, string>(GetGroupMembersCount);
            return View();
        }

        public ActionResult Board(int id)
        {
            ViewBag.MenuText = "Discussions";
            ViewBag.MenuId = 2;
            ViewBag.IsGroupMember = false;
            if (Request.IsAuthenticated)
            {
                Session["FullName"] = GetCurrentCollectorName();
                Session["CurrentCollectorId"] = GetCurrentCollectorId();
                ViewBag.IsGroupMember = this.IsGroupMember(id);
            }
            else
            {
                Session["FullName"] = "";
                Session["CurrentCollectorId"] = 0;
            }

            DiscussionsRespository discussionsRespository = new DiscussionsRespository();
            List<Discussion> discussionList = discussionsRespository.GetGroupDiscussions(id);
            ViewBag.DiscussionList = discussionList;
            ViewBag.GetUserName = new Func<int, string>(GetCollectorName);
            ViewBag.GetUserImage = new Func<int, string>(GetCollectorImage);
            CollectorRespository collectorRespository = new CollectorRespository();
            Group model = collectorRespository.GetGroups().Where(x => x.GroupId == id).SingleOrDefault();
            return View(model);
        }

        public ActionResult Discussion(int id)
        {
            ViewBag.MenuText = "Discussions";
            ViewBag.MenuId = 2;
            ViewBag.IsGroupMember = false;
            if (Request.IsAuthenticated)
            {
                Session["FullName"] = GetCurrentCollectorName();
                Session["CurrentCollectorId"] = GetCurrentCollectorId();
                ViewBag.IsGroupMember = this.IsGroupMember(id);
            }
            else
            {
                Session["FullName"] = "";
                Session["CurrentCollectorId"] = 0;
            }

            DiscussionsRespository discussionsRespository = new DiscussionsRespository();
            Discussion startDiscussion = discussionsRespository.GetDiscussion(id);
            ViewBag.Topic = startDiscussion;
            CollectorRespository collectorRespository = new CollectorRespository();
            Group model = collectorRespository.GetGroups().Where(x => x.GroupId == startDiscussion.GroupId).SingleOrDefault();
            List<Discussion> replyList = discussionsRespository.GetDiscussionReplies(id);
            ViewBag.ReplyList = replyList;
            ViewBag.GetUserName = new Func<int, string>(GetCollectorName);
            ViewBag.GetUserImage = new Func<int, string>(GetCollectorImage);
            return View(model);
        }

        public ActionResult Members(int id)
        {
            ViewBag.MenuText = "Members";
            ViewBag.MenuId = 3;
            ViewBag.IsGroupMember = false;
            ViewBag.IsGroupAdmin = false;
            if (Request.IsAuthenticated)
            {
                Session["FullName"] = GetCurrentCollectorName();
                Session["CurrentCollectorId"] = GetCurrentCollectorId();
                ViewBag.IsGroupAdmin = this.IsGroupAdmin(id);
                ViewBag.IsGroupMember = this.IsGroupMember(id);
            }
            else
            {
                Session["FullName"] = "";
                Session["CurrentCollectorId"] = 0;
            }

            ViewBag.GetUserName = new Func<int, string>(GetCollectorName);
            ViewBag.GetUserImage = new Func<int, string>(GetCollectorImage);

            CollectorRespository collectorRespository = new CollectorRespository();
            Group model = collectorRespository.GetGroups().Where(x => x.GroupId == id).SingleOrDefault();
            List<GroupMember> members = collectorRespository.GetGroupMembers(id);
            List<Collector> memberCollectors = new List<Collector>();
            List<Collector> adminCollectors = new List<Collector>();
            foreach (GroupMember member in members)
            {
                if (member.GroupRoleId == 1)
                {
                    adminCollectors.Add(collectorRespository.GetCollector(member.CollectorId));
                }
                else
                {
                    memberCollectors.Add(collectorRespository.GetCollector(member.CollectorId));
                }
            }
            ViewBag.AdminCollectors = adminCollectors;
            ViewBag.MemberCollectors = memberCollectors;
            ViewBag.AvailableCollectors = collectorRespository.GetCollectors(10000, "DESC");
            return View(model);
        }

        public ActionResult Photos(int id)
        {
            ViewBag.MenuText = "Photos";
            ViewBag.MenuId = 1;
            ViewBag.IsGroupMember = false;
            ViewBag.CollectiblesList = new List<Collectible>();
            if (Request.IsAuthenticated)
            {
                Session["FullName"] = GetCurrentCollectorName();
                Session["CurrentCollectorId"] = GetCurrentCollectorId();
                ViewBag.IsGroupMember = this.IsGroupMember(id);
                CollectiblesRespository collectiblesRepository = new CollectiblesRespository();
                ViewBag.CollectiblesList = collectiblesRepository.GetCollectibles(GetCurrentCollectorId());
            }
            else
            {
                Session["FullName"] = "";
                Session["CurrentCollectorId"] = 0;
            }

            ViewBag.GetUserName = new Func<int, string>(GetCollectorName);
            ViewBag.GetUserImage = new Func<int, string>(GetCollectorImage);
            CollectorRespository collectorRespository = new CollectorRespository();
            Group model = collectorRespository.GetGroups().Where(x => x.GroupId == id).SingleOrDefault();
            ViewBag.GroupCollectibles = collectorRespository.GetGroupImages(id);
            return View(model);
        }

        public ActionResult About(int id)
        {
            ViewBag.MenuText = "About";
            ViewBag.MenuId = 4;
            ViewBag.IsGroupMember = false;
            ViewBag.IsGroupAdmin = false;
            if (Request.IsAuthenticated)
            {
                Session["FullName"] = GetCurrentCollectorName();
                Session["CurrentCollectorId"] = GetCurrentCollectorId();
                ViewBag.IsGroupAdmin = this.IsGroupAdmin(id);
                ViewBag.IsGroupMember = this.IsGroupMember(id);
            }
            else
            {
                Session["FullName"] = "";
                Session["CurrentCollectorId"] = 0;
            }

            ViewBag.GetUserName = new Func<int, string>(GetCollectorName);
            ViewBag.GetUserImage = new Func<int, string>(GetCollectorImage);

            CollectorRespository collectorRespository = new CollectorRespository();
            Group model = collectorRespository.GetGroups().Where(x => x.GroupId == id).SingleOrDefault();
            List<GroupMember> members = collectorRespository.GetGroupMembers(id);
            List<Collector> memberCollectors = new List<Collector>();
            List<Collector> adminCollectors = new List<Collector>();
            foreach (GroupMember member in members)
            {
                if (member.GroupRoleId == 1)
                {
                    adminCollectors.Add(collectorRespository.GetCollector(member.CollectorId));
                }
                else
                {
                    memberCollectors.Add(collectorRespository.GetCollector(member.CollectorId));
                }
            }
            ViewBag.AdminCollectors = adminCollectors;
            ViewBag.MemberCollectors = memberCollectors;
            return View(model);
        }

        public ActionResult InviteFriends(int id)
        {
            ViewBag.MenuText = "Invite Friends";
            ViewBag.MenuId = 5;
            ViewBag.IsGroupMember = false;
            if (Request.IsAuthenticated)
            {
                Session["FullName"] = GetCurrentCollectorName();
                Session["CurrentCollectorId"] = GetCurrentCollectorId();
                ViewBag.IsGroupMember = this.IsGroupMember(id);
            }
            else
            {
                Session["FullName"] = "";
                Session["CurrentCollectorId"] = 0;
            }

            ViewBag.GetUserName = new Func<int, string>(GetCollectorName);
            ViewBag.GetUserImage = new Func<int, string>(GetCollectorImage);

            CollectorRespository collectorRespository = new CollectorRespository();
            Group model = collectorRespository.GetGroups().Where(x => x.GroupId == id).SingleOrDefault();
            List<GroupMember> members = collectorRespository.GetGroupMembers(id);
            List<Collector> memberCollectors = new List<Collector>();
            List<Collector> adminCollectors = new List<Collector>();
            foreach (GroupMember member in members)
            {
                if (member.GroupRoleId == 1)
                {
                    adminCollectors.Add(collectorRespository.GetCollector(member.CollectorId));
                }
                else
                {
                    memberCollectors.Add(collectorRespository.GetCollector(member.CollectorId));
                }
            }
            ViewBag.AdminCollectors = adminCollectors;
            ViewBag.MemberCollectors = memberCollectors;
            // TODO
            List<Collector> collectors = collectorRespository.GetCollectors(10000, "DESC");
            ViewBag.CollectorId = new SelectList(collectors, "CollectorId", "FirstName");
            return View(model);
        }


        [HttpPost]
        public ActionResult JoinGroup(int id)
        {
            string userId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            CollectorRespository collectorRespository = new CollectorRespository();
            GroupMember model = new GroupMember();
            model.GroupId = id;
            model.CollectorId = collectorRespository.GetCollector(userId).CollectorId;
            model.GroupRoleId = 0;
            model.CreatedDate = DateTime.Now;
            collectorRespository.JoinGroup(model);
            return Redirect("~/Groups/Board/" + id);
        }

        [HttpPost]
        public ActionResult LeaveGroup(int id)
        {
            string userId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            CollectorRespository collectorRespository = new CollectorRespository();
            GroupMember model = new GroupMember();
            model.GroupId = id;
            model.CollectorId = collectorRespository.GetCollector(userId).CollectorId;
            collectorRespository.LeaveGroup(model);
            return Redirect("~/Groups/Photos/" + id);
        }

        [HttpPost]
        public ActionResult AddPhoto(FormCollection form)
        {
            string userId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            CollectorRespository collectorRespository = new CollectorRespository();
            int groupId = Convert.ToInt16(form[0]);
            int collectibleId = Convert.ToInt16(form[1]);
            GroupPhoto model = new GroupPhoto();
            model.CollectorId = collectorRespository.GetCollector(userId).CollectorId;
            model.GroupId = groupId;
            model.CollectibleId = collectibleId;
            model.CreatedDate = DateTime.Now;
            collectorRespository.InsertGroupPhoto(model);
            return Redirect("~/Groups/Photos/" + groupId);
        }

        [HttpPost]
        public ActionResult Invitation(FormCollection form)
        {
            string userId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            CollectorRespository collectorRespository = new CollectorRespository();
            GroupInvitation invitationModel = new GroupInvitation();
            int id = Convert.ToInt16(form[0]);
            invitationModel.CollectorId = collectorRespository.GetCollector(userId).CollectorId;
            invitationModel.ToCollectorId = Convert.ToInt16(form[1]);
            invitationModel.Message = form[2].ToString();
            invitationModel.GroupId = id;
            invitationModel.StatusId = 0;
            invitationModel.CreatedDate = DateTime.Now;
            collectorRespository.InsertGroupInvitation(invitationModel);
            Group modelGroup = collectorRespository.GetGroups().Where(x => x.GroupId == id).SingleOrDefault();
            Message invitationMessage = new Message();
            invitationMessage.MessageTopic = "Group Invitation - " + "<a href='../../Groups/Photos/"+id+"'>" + modelGroup.Name + "</a>";
            invitationMessage.MessageText = invitationModel.Message;
            invitationMessage.FromCollectorId = invitationModel.CollectorId;
            invitationMessage.ToCollectorId = invitationModel.ToCollectorId;
            invitationMessage.CreatedDate = DateTime.Now;
            collectorRespository.InsertMessage(invitationMessage);
            //
            var linkUrl = Url.Action("Photos", "Groups", new { id = id }, protocol: Request.Url.Scheme);
            string toEmail = this.GetCollectorEmail(invitationModel.ToCollectorId);
            string toName = this.GetCollectorName(invitationModel.ToCollectorId);
            string fromName = this.GetCollectorName(invitationModel.CollectorId);
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("no-reply@mymuseo.com");
            mailMessage.To.Add(new MailAddress(toEmail));
            mailMessage.CC.Add(new MailAddress("tsonla@yahoo.com"));
            mailMessage.CC.Add(new MailAddress("gjamroski@mac.com"));
            mailMessage.Subject = "Group Invitation";
            mailMessage.IsBodyHtml = true;
            var imagePath0 = Path.Combine(Server.MapPath("~/Content/images"), "myMuseo.png");
            String image0 = Convert.ToBase64String(System.IO.File.ReadAllBytes(imagePath0));
            var imagePath1 = Path.Combine(Server.MapPath("~/Content/images"), "mymuseo-img.jpg");
            String image1 = Convert.ToBase64String(System.IO.File.ReadAllBytes(imagePath1));
            TemplateModel content = _collectorRespository.GetTemplateByName("Group Invitation");
            string mailTemplate = content.TemplateText;
            string theMessage = String.Format(mailTemplate, image0, image1, toName, fromName, modelGroup.Name, linkUrl, linkUrl);
            mailMessage.Body = theMessage;
            this.SendMail(mailMessage);
            //
            return Redirect("~/Groups/Board/" + id);
        }

        public string GetCollectorImage(int collectorId)
        {
            CollectorRespository collectorRespository = new CollectorRespository();
            Collector collector = collectorRespository.GetCollector(collectorId);
            return collector.ProfileImage;
        }

        public string GetGroupMembersCount(int groupId)
        {
            CollectorRespository collectorRespository = new CollectorRespository();
            int count = collectorRespository.GetGroupMembersCount(groupId);
            if (count == 0)
            { return "no member"; }
            if (count == 1)
            { return "1 member"; }
            return count + " members";
        }

        public bool IsGroupAdmin(int groupId)
        {
            CollectorRespository collectorRespository = new CollectorRespository();
            Collector collector = collectorRespository.GetCollector(User.Identity.GetUserId());
            List<GroupMember> members = collectorRespository.GetGroupMembers(groupId);
            foreach (GroupMember member in members)
            {
                if (member.CollectorId == collector.CollectorId)
                {
                    if (member.GroupRoleId == 1)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsGroupMember(int groupId)
        {
            CollectorRespository collectorRespository = new CollectorRespository();
            Collector collector = collectorRespository.GetCollector(User.Identity.GetUserId());
            List<GroupMember> members = collectorRespository.GetGroupMembers(groupId);
            foreach (GroupMember member in members)
            {
                if (member.CollectorId == collector.CollectorId)
                {
                   return true;
                }
            }
            return false;
        }

        [HttpPost]
        public ActionResult UpdateAbout(FormCollection form)
        {
            int groupId = Convert.ToInt32(form[0]);
            string description = form[1];
            CollectorRespository collectorRespository = new CollectorRespository();
            collectorRespository.UpdateGroupDescription(groupId, description);
            return Redirect("~/Groups/About/" + groupId);
        }

        [HttpPost]
        public ActionResult StartDiscussion(FormCollection form)
        {
            DiscussionsRespository discussionsRespository = new DiscussionsRespository();
            int groupId = Convert.ToInt32(form[0]);
            string topic = form[1];
            string comments = form[2];
            int discussionId = 0;
            if (form[3].ToString() != String.Empty)
            {
                discussionId = Convert.ToInt32(form[3]);
            }
            Discussion discussionModel = new Discussion();
            discussionModel.GroupId = groupId;
            discussionModel.PostByCollectorId = this.GetCurrentCollectorId();
            discussionModel.ParentId = discussionId;
            discussionModel.DiscussionTopic = topic;
            discussionModel.DiscussionText = comments;
            discussionModel.CreatedDate = DateTime.Now;
            discussionsRespository.InsertDiscussion(discussionModel);
            return Redirect("~/Groups/Board/" + groupId);
        }

        [HttpPost]
        public ActionResult ReplyDiscussion(FormCollection form)
        {
            DiscussionsRespository discussionsRespository = new DiscussionsRespository();
            int groupId = Convert.ToInt32(form[0]);
            string comments = form[1];
            int discussionId = 0;
            if (form[2].ToString() != String.Empty)
            {
                discussionId = Convert.ToInt32(form[2]);
            }
            Discussion discussionModel = new Discussion();
            discussionModel.GroupId = groupId;
            discussionModel.PostByCollectorId = this.GetCurrentCollectorId();
            discussionModel.ParentId = discussionId;
            discussionModel.DiscussionTopic = "Reply";
            discussionModel.DiscussionText = comments;
            discussionModel.CreatedDate = DateTime.Now;
            discussionsRespository.InsertDiscussion(discussionModel);
            return Redirect("~/Groups/Discussion/" + discussionId);
        }

        [HttpPost]
        public ActionResult AddAdmin(FormCollection form)
        {
            string userId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            CollectorRespository collectorRespository = new CollectorRespository();
            int groupId = Convert.ToInt16(form[0]);
            int adminId = Convert.ToInt16(form[1]);
            GroupMember model = new GroupMember();
            model.GroupId = groupId;
            model.CollectorId = adminId;
            model.GroupRoleId = 1;
            model.CreatedDate = DateTime.Now;
            _collectorRespository.JoinGroup(model);
            return Redirect("~/Groups/Members/" + groupId);
        }

        [HttpPost]
        public ActionResult DeleteGroup(FormCollection form)
        {
            int groupId = Convert.ToInt16(form[0]);
            _collectorRespository.DeleteGroup(groupId);
            return Redirect("~/Groups/MyGroups/");
        }

        [HttpPost]
        public ActionResult LeaveMyGroup(FormCollection form)
        {
            int groupId = Convert.ToInt16(form[0]);
            GroupMember model = new GroupMember();
            model.CollectorId = _collector.CollectorId;
            model.GroupId = groupId;
            _collectorRespository.LeaveGroup(model);
            return Redirect("~/Groups/MyGroups/");
        }

    }
}