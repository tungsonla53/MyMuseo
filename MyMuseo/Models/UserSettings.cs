using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMuseo.Models
{
    public class UserSettings
    {
        public int UserSettingId { get; set; }
        public int CollectorId { get; set; }
        public int AllowFollowId { get; set; }
        public int AllowUnfollowId { get; set; }
        public int FeaturePrivacyId { get; set; }
        public int FollowPrivacyId { get; set; }
        public int LocationPrivacyId { get; set; }
        public int ProfilePrivacyId { get; set; }
        public bool ReceiveEmails { get; set; }
        public bool ReceiveLetters { get; set; }
        public bool ChangesToPolicies { get; set; }
        public bool NotifyWhenFollowed { get; set; }
        public bool NotifyWhenCommented { get; set; }
        public bool NotifyWhenReplied { get; set; }
        public bool NotifyWhenInvited { get; set; }
        public bool NotifyWhenInvitedToAnEvent { get; set; }
        public bool NotifyWhenSomeoneLikes { get; set; }
        public bool NotifyWhenSomeoneAttendingEvent { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}