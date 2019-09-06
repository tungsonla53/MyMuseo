using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;

namespace MyMuseo.Models
{
    public class User : IUser
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Nickname { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public bool IsConfirmed { get; set; }
        public string ConfirmationToken { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}