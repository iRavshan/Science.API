using Microsoft.AspNetCore.Identity;

namespace Science.Domain.Models
{
    public class User : IdentityUser
    {
        public string Firstname { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;

    }
}
