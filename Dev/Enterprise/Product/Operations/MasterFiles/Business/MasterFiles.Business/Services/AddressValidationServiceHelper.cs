using System;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using CargoWise.Application;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AddressValidationServiceHelper
	{
		public AuthenticationHeaderValue GetAuthenticationHeaderValue(bool enableSysToSysTrust)
		{
			var productionRegistrationKey = GetProductRegistrationKey();
			CheckSystemIsRegistered(productionRegistrationKey);

			return enableSysToSysTrust
				? TokenManager.GetInstance(MDMProductCodes.AVS).GetAuthorizationHeaderValue()
				: new AuthenticationHeaderValue((NoResString)"Basic", GetBasicAuthenticationParameter(productionRegistrationKey));
		}

		IProductRegistrationKey GetProductRegistrationKey()
			=> ObjectFactory.Get<IProductRegistration>().Key;

		void CheckSystemIsRegistered(IProductRegistrationKey productRegistrationKey)
		{
			if (string.IsNullOrEmpty(productRegistrationKey.SystemId))
			{
				throw new AuthenticationException(Res.GetString("AB6202EE-A675-4582-8591-F75FC7AEAEBF", "Address Validation verifies the address information you have entered is correct and can provide suggestions if the address is not found. This service aims to improve data accuracy and reduce futile deliveries. It also provides Coordinates for displaying of addresses on a map and is a pre-requisite for optimization, routing and rating. Only registered systems can utilize this function. Please register and you will be able to obtain the various benefits of this function."));
			}
		}

		string GetBasicAuthenticationParameter(IProductRegistrationKey productRegistrationKey)
			=> Convert.ToBase64String(Encoding.ASCII.GetBytes($"{productRegistrationKey.SystemId}:{productRegistrationKey.Password}"));
	}
}
