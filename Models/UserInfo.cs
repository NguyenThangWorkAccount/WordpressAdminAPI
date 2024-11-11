namespace WordpressAdmin.API.Models
{
    public class UserData
    {
        public string? Username { get; set; }
        public Email? Email { get; set; }
        public string? Role { get; set; }
        public Password? Password { get; set; }
    }
}