using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMuseo.Models
{
    public class Favorite
    {
        public int FavoriteId { get; set; }
        public int CollectorId { get; set; }
        public int FavoriteCollectorId { get; set; }
        public int FavoriteCollectionId { get; set; }
        public int FavoriteCollectibleId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}