namespace WordpressAdmin.API.Models;

public class LoginData
{
    public string[] LoginUrls { get; set; }
    public string[] Usernames { get; set; }
    public string[] Passwords { get; set; }
    public string[] PEPasswords { get; }

    // Ensures required data is provided in the constructor
    public LoginData(string[] loginUrls, string[] usernames, string[] passwords, string[] pePasswords)
    {
        if (loginUrls == null || !loginUrls.Any()) throw new ArgumentException("LoginUrls cannot be empty");
        if (usernames == null || !usernames.Any()) throw new ArgumentException("Usernames cannot be empty");
        if (passwords == null || !passwords.Any()) throw new ArgumentException("Passwords cannot be empty");
        if (pePasswords == null || !pePasswords.Any()) throw new ArgumentException("PEPasswords cannot be empty");

        LoginUrls = loginUrls;
        Usernames = usernames;
        Passwords = passwords;
        PEPasswords = pePasswords;
    }
}