namespace WordpressAdminApi.Models;

public class LoginData
{
    public string[] LoginUrls { get; set; }
    public string[] Usernames { get; set; }
    public string[] Passwords { get; set; }

    // Ensures required data is provided in the constructor
    public LoginData(string[] loginUrls, string[] usernames, string[] passwords)
    {
        if (loginUrls == null || !loginUrls.Any()) throw new ArgumentException("LoginUrls cannot be empty");
        if (usernames == null || !usernames.Any()) throw new ArgumentException("Usernames cannot be empty");
        if (passwords == null || !passwords.Any()) throw new ArgumentException("Passwords cannot be empty");

        LoginUrls = loginUrls;
        Usernames = usernames;
        Passwords = passwords;
    }
}