using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMuseo.Models
{
    public class Purchase
    {
        public int PurchaseId { get; set; }
        public int CustomerId { get; set; }
        public int CollectibleId { get; set; }
        public int CollectorId { get; set; }
        public int Count { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public Decimal Price { get; set; }
        // Type = 1 Buy
        // Type = 2 Offer
        public int TypeId { get; set; }
        // Status = 1  Pending
        // Status = 2  Paid
        // Status = 9  Cancelled 
        public int StatusId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string TrackingNumber { get; set; }
        public string Shipper { get; set; }
    }
}