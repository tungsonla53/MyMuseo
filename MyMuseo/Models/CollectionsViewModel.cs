using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyMuseo.Models
{
    public class CollectionsViewModel
    {
        public IEnumerable<Collection> Collections { get; set; }
    }
}