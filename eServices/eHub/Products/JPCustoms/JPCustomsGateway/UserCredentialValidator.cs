using System.Configuration;
using System.IdentityModel.Selectors;
using System.ServiceModel;

namespace CargoWise.eHub.Products.JPCustoms.Gateway
{
	public class UserCredentialValidator : UserNamePasswordValidator
	{
		public override void Validate(string userName, string password)
		{
			if (userName != ConfigurationManager.AppSettings["UserName"] || password != ConfigurationManager.AppSettings["Password"])
			{
				throw new FaultException("ClientId or Password invalid.");
			}
		}
	}
}
