using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMuseo.Models
{
    public class ContentModel
    {
        public int ContentId { get; set; }
        public string ContentName { get; set; }
        public string ContentText { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}