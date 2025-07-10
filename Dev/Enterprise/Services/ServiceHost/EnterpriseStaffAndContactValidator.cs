namespace Enterprise.Services.ServiceHost
{
	public class EnterpriseStaffAndContactValidator : EnterpriseUserNamePasswordValidatorBase
	{
		protected override AuthenticationResult GetAuthenticationResult(string userName, string password)
		{
			return GetAuthenticationResultCore(userName, password, allowStaffOnly: false);
		}
	}
}
