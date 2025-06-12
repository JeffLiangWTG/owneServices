using CargoWise.eHub.Common;
using System.IdentityModel.Selectors;
using System.ServiceModel;

namespace CargoWise.eAdaptorSampleWebService
{
	public class eAdaptorUserNamePasswordValidator : UserNamePasswordValidator
	{
		public override void Validate(string userName, string password)
		{
			if (!ValidatePassword(userName, SHA512Encryptor.Encrypt(userName + password))) throw new FaultException("ClientID or Password invalid.");
		}

		bool ValidatePassword(string userName, string hash)
		{
#warning If you require user authentication, you can implement your own authentication here. You can remove this warning when done.
			return true;
		}
	}
}
