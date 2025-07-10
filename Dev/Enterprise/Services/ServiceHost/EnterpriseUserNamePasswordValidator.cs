namespace Enterprise.Services.ServiceHost
{
	public class EnterpriseUserNamePasswordValidator : EnterpriseUserNamePasswordValidatorBase
	{
		protected override AuthenticationResult GetAuthenticationResult(string userName, string password)
		{
			return GetAuthenticationResultCore(userName, password, allowStaffOnly: true);
		}
	}
}
