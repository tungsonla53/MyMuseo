using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMuseo.Models
{
    public class TemplateModel
    {
        public int TemplateId { get; set; }
        public int TemplateTypeId { get; set; }
        public string TemplateName { get; set; }
        public string TemplateText { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}