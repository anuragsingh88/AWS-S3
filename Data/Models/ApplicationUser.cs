﻿using Microsoft.AspNetCore.Identity;

namespace AWS_S3.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int? AppUserID { get; set; }
        public int? CreatedBy { get; set; }
        public DateTimeOffset? CreatedDateTime { get; set; }
        public DateTimeOffset? UpdatedDateTime { get; set; }
        public  int? UpdatedBy { get; set; }
    }
}