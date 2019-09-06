using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMuseo.Models
{
    public class ThreadResponse
    {
        public int ThreadResponseId { get; set; }
        public int ThreadId { get; set; }
        public int ThreadResponseTypeId { get; set; }
        public int ResponseByCollectorId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}