using System;

namespace Shared.DTOs
{
    public class LoginRequestDto
    {
        public string Type { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
    }

    public class UserDto
    {
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string BU { get; set; }
        public string Type { get; set; }
        public string Customer { get; set; }
        public string Dept { get; set; }
        public string RoleId { get; set; }
        public string Role { get; set; }
    }

    public class LoginResultDto
    {
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string BU { get; set; }
        public string Type { get; set; }
        public string Customer { get; set; }
        public string Dept { get; set; }
        public string RoleId { get; set; }
        public string Role { get; set; }
        public string CurrentRole { get; set; }
    }
}
