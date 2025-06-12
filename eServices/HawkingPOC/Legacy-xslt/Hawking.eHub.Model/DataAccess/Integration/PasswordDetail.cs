namespace Hawking.eHub.Model.DataAccess.Integration
{
    public class PasswordDetail
    {
        public PasswordDetail(string password, string emailAddress)
        {
            Password = password;
            EmailAddress = emailAddress;
        }

        public string Password { get; set; }
        public string EmailAddress { get; private set; }
    }
}
