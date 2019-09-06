using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyMuseo.Models
{
    public class Banner
    {
        public int BannerId { get; set; }

        [Required(ErrorMessage = "Please enter banner title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Please enter the target page")]
        public string LinkTo { get; set; }
        public string Image { get; set; }
        public string ImageMobile { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
    }
}