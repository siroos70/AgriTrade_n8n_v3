using System;
using Microsoft.AspNetCore.Identity;

namespace AgriTrade.Backend.Data
{
    public class User : IdentityUser
    {
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool Active { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}