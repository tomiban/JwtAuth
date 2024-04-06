using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Dtos.Account
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = String.Empty;
        [Required]
        public string FullName { get; set; } = String.Empty;
        [Required]
        public string Password { get; set; } = String.Empty;
        public List<string>? Roles { get; set; }
    }
}