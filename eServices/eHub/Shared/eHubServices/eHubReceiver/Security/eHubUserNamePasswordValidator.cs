using System.Configuration;
using System.IdentityModel.Selectors;
using System.ServiceModel;

namespace CargoWise.eHub.Share.eHubServices.eHubReceiver.Security
{
	public class eHubUserNamePasswordValidator : UserNamePasswordValidator
	{
		public override void Validate(string userName, string password)
		{
			if (userName != ConfigurationManager.AppSettings["UserName"] || password != ConfigurationManager.AppSettings["Password"])
			{
				throw new FaultException("ClientID or Password invalid.");
			}
		}
	}
}
