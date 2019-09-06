using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMuseo.Models
{
    public class Artist
    {
        public int ArtistId { get; set; }
        public string Name { get; set; }
        public string HomePageName { get; set; }
        public string Bio { get; set; }
        public string HomePageImage { get; set; }
        public string BioImage { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime DeceasedDate { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public int DisplayOrder { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}