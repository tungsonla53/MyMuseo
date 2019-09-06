using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMuseo.Models
{
    public class Message
    {
        public int MessageId { get; set; }
        public string MessageTopic { get; set; }
        public string MessageText { get; set; }
        public int FromCollectorId { get; set; }
        public int ToCollectorId { get; set; }
        public int ParentId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ViewStatusId { get; set; }
        public DateTime ViewedDate { get; set; }
    }
}