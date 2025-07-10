namespace Enterprise.ProcessManagement.Business
{
	public class JiraCredentials
	{
		public JiraCredentials(string username, string password)
		{
			Username = username;
			Password = password;
		}

		public string Username { get; set; }
		public string Password { get; set; }
	}
}
