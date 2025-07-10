using System.Collections;
using CargoWise.Application;

namespace Enterprise.Customs.Business
{
	public class CreateDeclarationHelperProvider : Integration.Customs.Shared.ICreateDeclarationHelperProvider
	{
		public Integration.Customs.Shared.ICreateDeclarationHelper NewCreateDeclarationHelper(string countryCode)
		{
			var jurisdictionCountry = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdictionOrEU(countryCode);
			var providers = ObjectFactory.Get<Hashtable>("CreateDeclarationHelpers");
			var providerHandle = (ObjectHandle)(providers.ContainsKey(jurisdictionCountry) ? providers[jurisdictionCountry] : providers["Shared"]);

			return (Integration.Customs.Shared.ICreateDeclarationHelper)providerHandle.GetObject();
		}
	}
}
