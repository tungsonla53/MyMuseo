using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMuseo.Models
{
    public class GroupInvitation
    {
        public int InvitationId { get; set; }
        public int CollectorId { get; set; }
        public int GroupId { get; set; }
        public int ToCollectorId { get; set; }
        public int StatusId { get; set; }
        public String Message { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}