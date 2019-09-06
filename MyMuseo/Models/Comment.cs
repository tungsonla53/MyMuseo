using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMuseo.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        public string CommentText { get; set; }
        public int CollectionId { get; set; }
        public int CollectibleId { get; set; }
        public int PostByCollectorId { get; set; }
        public bool IsApproved { get; set; }
        public bool FlagAsAbuse { get; set; }
        public int ParentId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}