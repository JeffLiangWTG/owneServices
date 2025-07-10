
namespace Enterprise.Services.ServiceHost
{
	public sealed class DDDBasicAuthenticationAttribute : IdentityBasicAuthenticationAttribute
	{
		protected override bool IsOK(string userName, string password, out string message)
		{
			message = string.Empty;
			try
			{
				new DDDUserNamePasswordValidator().Validate(userName, password);
				return true;
			}
			catch (System.ServiceModel.FaultException ex)
			{
				message = ex.Message;
			}
			return false;
		}
	}
}
