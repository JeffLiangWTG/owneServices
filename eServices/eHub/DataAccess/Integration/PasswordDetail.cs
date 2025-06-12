namespace CargoWise.eHub.DataAccess.Integration
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

		public static implicit operator PasswordDetail(eServices.eHubDataAccess.Integration.PasswordDetail passwordDetail)
			=> passwordDetail is null ? null : new PasswordDetail(passwordDetail.Password, passwordDetail.EmailAddress);
	}
}
