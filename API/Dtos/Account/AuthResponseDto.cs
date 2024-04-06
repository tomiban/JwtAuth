using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Dtos.Account
{
    public class AuthResponseDto
    {
        public string?  Token { get; set; } = String.Empty;
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
    }
}