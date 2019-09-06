using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyMuseo.Models
{
    public class Collection
    {
        public int CollectionId { get; set; }
        public int CollectorId { get; set; }
        public int ArtistId { get; set; }
        public int CategoryId { get; set; }
        public string ThumbImage { get; set; }
        public string NormalImage { get; set; }

        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        
        [Required]
        public bool IsForSale { get; set; }
        public int DisplayOrder { get; set; }
        public Decimal Price { get; set; }
        public string Dimensions { get; set; }
        public string Weight { get; set; }
        public DateTime CreatedDate { get; set; }
        public int FeaturedItemId { get; set; }
        public bool IsDraft { get; set; }
    }
}