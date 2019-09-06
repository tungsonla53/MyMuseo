using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMuseo.Models
{
    public class GroupPhoto
    {
        public int PhotoId { get; set; }
        public int CollectorId { get; set; }
        public int GroupId { get; set; }
        public int CollectibleId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}