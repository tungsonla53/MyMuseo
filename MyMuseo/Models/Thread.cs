using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMuseo.Models
{
    public class Thread
    {
        public int ThreadId { get; set; }
        public string ThreadTopic { get; set; }
        public string ThreadText { get; set; }
        public int ThreadTypeId { get; set; }
        public int PostByCollectorId { get; set; }
        public string ThreadImage { get; set; }
        public DateTime ThreadStartDate { get; set; }
        public DateTime ThreadEndDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<Post> Posts { get; set; }
        public virtual int LikesCount { get; set; }
        public virtual int InterestedCount { get; set; }
        public virtual int GoingCount { get; set; }
        public virtual bool IsOwner { get; set; }
        public virtual bool HasComments { get; set; }
    }
}