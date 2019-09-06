using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMuseo.Models
{
    public class Follow
    {
        public int FollowId { get; set; }
        public int CollectorId { get; set; }
        public int FollowCollectorId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}