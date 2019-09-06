using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMuseo.Models
{
    public class Reply
    {
        public int ReplyId { get; set; }
        public string ReplyTopic { get; set; }
        public string ReplyText { get; set; }
        public int PostId { get; set; }
        public int ReplyByCollectorId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}