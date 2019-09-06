using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMuseo.Models
{
    public class Post
    {
        public int PostId { get; set; }
        public string PostTopic { get; set; }
        public string PostText { get; set; }
        public int ThreadId { get; set; }
        public int PostByCollectorId { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<Reply> Replies { get; set; }
    }
}