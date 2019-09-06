using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMuseo.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public int ActionTypeId { get; set; }
        public int CollectorId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}