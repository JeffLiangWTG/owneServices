using CargoWise.Types;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.Business
{
	public sealed class NctsSettings : Integration.Customs.Shared.INctsSettings
	{
		public NctsSettings()
		{
		}

		public bool IsNctsEnabled
		{
			get
			{
				var company = GlbCompany.CurrentCompany;
				var countryCode = company.Country?.Code;
				return !string.IsNullOrEmpty(countryCode) && company.Factory.IsCountryEuOrCtCountry(countryCode);
			}
		}

		public bool IsUsingPhase5(string countryCode)
		{
			var today = ZDateTime.Today;
			return !ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(FunctionalityTypes.NCTSPhase4, countryCode, today)
				|| ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(FunctionalityTypes.NCTSPhase5Override, countryCode, today);
		}
	}
}
