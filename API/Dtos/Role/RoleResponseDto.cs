using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Dtos.Role
{
    public class RoleResponseDto
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public int TotalUser { get; set; }
    }
}