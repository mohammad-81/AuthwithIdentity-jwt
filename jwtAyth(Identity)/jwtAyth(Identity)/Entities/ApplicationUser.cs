﻿using Microsoft.AspNetCore.Identity;

namespace jwtAuth_Identity_.Entities
{
    public class ApplicationUser:IdentityUser<long>
    {
        public string FullName { get; set; }
    }
}
