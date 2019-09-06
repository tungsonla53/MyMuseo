using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyMuseo.Models
{
    public class AddressInfo
    {
        public int AddressId { get; set; }
        [Required]
        public string Street { get; set; }
        public string Apt { get; set; }
        [Required(ErrorMessage = "Please select a category")]
        public int CountryId { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Region { get; set; }
        [Required]
        public string ZipPostalCode { get; set; }
        public string PhoneNumber { get; set; }
        public string FaxNumber { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int CollectorId { get; set; }
    }
}