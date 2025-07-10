using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public sealed class TemporaryStorageSettings : Integration.Customs.Shared.ITemporaryStorageSettings
	{
		public TemporaryStorageSettings() { }

		public bool IsUsingUCC5
		{
			get
			{
				var temporaryStorageProductionCountries = new HashSet<string>
				{
					Core.Constants.CountryCodes.Germany,
					Core.Constants.CountryCodes.France,
					Core.Constants.CountryCodes.Poland
				};
				var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				var customsJurisdiction = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(currentCountry);
				return temporaryStorageProductionCountries.Contains(Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(currentCountry))
					|| (!string.IsNullOrWhiteSpace(currentCountry) && ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Customs.Universal.Constants.FunctionalityTypes.TemporaryStorage, customsJurisdiction, ZDateTime.Now));
			}
		}

		public bool IsUsingUCC6 => (bool)ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().PNTSEnabled.Value || (bool)ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().PNTSEnabledDeveloperOnly.Value;

		public bool IsUsingTSRegister => (bool)ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabled.Value || (bool)ObjectFactory.Get<Integration.Customs.EU.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly.Value;
	}
}
