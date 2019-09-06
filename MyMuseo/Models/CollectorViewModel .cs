using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMuseo.Models
{
    public class CollectorViewModel
    {
        public Collector Collector { get; set; }
        public AddressInfo AddressInfo { get; set; }
        public String Email { get; set; }
    }
}