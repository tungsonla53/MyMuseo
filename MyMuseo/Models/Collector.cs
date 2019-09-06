using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyMuseo.Models
{
    public class Collector
    {
        public int CollectorId { get; set; }

        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string AboutMe { get; set; }
        public string PageImage { get; set; }
        public string ProfileImage { get; set; }
        public int DisplayOrder { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UserId { get; set; }
        public string AreasOfInterest { get; set; }
        public string WebSites { get; set; }
        public bool IsAdmin { get; set; }
        public int FeaturedItemId { get; set; }

        [Required(ErrorMessage = "Please select a role")]
        public int TypeId { get; set; }
    }
}