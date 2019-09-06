using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyMuseo.Models
{
    public class CollectiblesViewModel
    {
        public int CollectionId { get; set; }
        public IEnumerable<Collectible> Collectibles { get; set; }
        public IEnumerable<SelectListItem> Collections { get; set; }
    }
}