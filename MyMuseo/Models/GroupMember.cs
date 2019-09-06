using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMuseo.Models
{
    public class GroupMember
    {
        public int MemberId { get; set; }
        public int CollectorId { get; set; }
        public int GroupId { get; set; }
        public int GroupRoleId { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}