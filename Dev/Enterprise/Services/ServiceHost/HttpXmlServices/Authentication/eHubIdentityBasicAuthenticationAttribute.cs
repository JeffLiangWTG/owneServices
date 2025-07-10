using System.Web;

namespace Enterprise.Services.ServiceHost
{
	public sealed class eHubIdentityBasicAuthenticationAttribute : IdentityBasicAuthenticationAttribute
	{
		protected override bool IsOK(string userName, string password, out string message)
		{
			message = string.Empty;
			try
			{
				new eHubUserNamePasswordValidator().Validate(userName, password);
				if (HttpContext.Current != null)
				{
					var possibleErrorMessage = (string)HttpContext.Current.Items[eHubUserNamePasswordValidator.ErrorMessageKeyInContextItems];
					if (!string.IsNullOrEmpty(possibleErrorMessage))
					{
						message = possibleErrorMessage;
						return false;
					}
				}

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
