using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMuseo.Models
{
    public class Country
    {
        public int CountryId { get; set; }
        public string Name { get; set; }
        public string TwoLetterIsoCode { get; set; }
        public string ThreeLetterIsoCode { get; set; }
        public int NumericIsoCode { get; set; }
        public int DisplayOrder { get; set; }
    }
}