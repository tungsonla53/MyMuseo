using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMuseo.Models
{
    public class Cart
    {
        public int RecordId { get; set; }
        public string CartId { get; set; }
        public int CollectibleId { get; set; }
        public int Count { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public Decimal Price { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}