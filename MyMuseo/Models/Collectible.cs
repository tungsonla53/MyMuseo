using Foolproof;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyMuseo.Models
{
    public class Collectible
    {
        public int CollectibleId { get; set; }
        public int CollectorId { get; set; }
        public int CollectionId { get; set; }
        public int ArtistId { get; set; }

        [Required(ErrorMessage = "Please select a category")]
        public int CategoryId { get; set; }
        public string ThumbImage { get; set; }
        public string NormalImage { get; set; }
        public string OriginalImage { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public bool NotForSale { get; set; }
        public bool AllowOffers { get; set; }
        public int DisplayOrder { get; set; }

        [RequiredIf("NotForSale", false, ErrorMessage = "Please enter price")]
        [RegularExpression(@"^\d+.\d{0,2}$", ErrorMessage = "Has to be decimal with two decimal points")]
        public Decimal Price { get; set; }

        [RegularExpression(@"^\d+.\d{0,2}$", ErrorMessage = "Has to be decimal with two decimal points")]
        public Decimal OfferPrice { get; set; }
        public string ArtistName { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string Weight { get; set; }
        public string Medium { get; set; }

        [RegularExpression(@"^(19|20)\d{2}$", ErrorMessage = "Value between 1900-2099")]
        public string ItemDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string OtherCategory { get; set; }
        public string AudioFile { get; set; }
    }
}