using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMuseo.Models
{
    public class ProfileInfo
    {
        public int ProfileId { get; set; }
        public string AreasOfInterest { get; set; }
        public string Links { get; set; }
        public string AboutMe { get; set; }
        public string ProfileImage { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdateddDate { get; set; }
        public string UserId { get; set; }
        public List<String> InterestList { get; set; }

        public List<LinkUrl> LinkUrlList { get; set; }

    }

    public class LinkUrl
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
}