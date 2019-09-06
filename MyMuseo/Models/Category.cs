using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMuseo.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Alias { get; set; }
        public int ParentCategoryId { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }
}