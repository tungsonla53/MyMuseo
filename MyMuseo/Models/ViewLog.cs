using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMuseo.Models
{
    public class ViewLog
    {
        public int ViewId { get; set; }
        public int ViewCollectorId { get; set; }
        public int ViewCollectionId { get; set; }
        public int ViewCollectibleId { get; set; }
        public int ViewByCollectorId { get; set; }
        public string ViewPageName { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}