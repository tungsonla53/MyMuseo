using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMuseo.Models
{
    public class ThreadLike
    {
        public int ThreadLikeId { get; set; }
        public int ThreadId { get; set; }
        public int LikeByCollectorId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}