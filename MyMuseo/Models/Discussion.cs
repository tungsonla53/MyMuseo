using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMuseo.Models
{
    public class Discussion
    {
        public int DiscussionId { get; set; }
        public string DiscussionTopic { get; set; }
        public string DiscussionText { get; set; }
        public int GroupId { get; set; }
        public int PostByCollectorId { get; set; }
        public bool IsApproved { get; set; }
        public bool FlagAsAbuse { get; set; }
        public int ParentId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}