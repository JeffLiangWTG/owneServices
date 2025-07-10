using System;
using System.Collections.Generic;

namespace Enterprise.Customs.Business
{
	public static class AdditionalInvoiceLineEntryLineLinkSupporter
	{
		public static bool DoesSupport(string countryCode)
		{
			return CountriesNeedingAdditionalLinkBetweenInvoiceLineAndEntryLine.Contains(Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode));
		}

		static List<string> CountriesNeedingAdditionalLinkBetweenInvoiceLineAndEntryLine
		{
			get { return countriesNeedingAdditionalLinkBetweenInvoiceLineAndEntryLine ?? (countriesNeedingAdditionalLinkBetweenInvoiceLineAndEntryLine = GetCountriesNeedingAdditionalLink()); }
		}
		[ThreadStatic]
		static List<string> countriesNeedingAdditionalLinkBetweenInvoiceLineAndEntryLine;

		static List<string> GetCountriesNeedingAdditionalLink()
		{
			var result = new List<string>();
			result.Add(Core.Constants.CountryCodes.UnitedStates);
			result.Add(Core.Constants.CountryCodes.Canada);
			result.Add(Core.Constants.CountryCodes.China);
			result.Add(Core.Constants.CountryCodes.KoreaSouth);
			result.Add(Core.Constants.CountryCodes.Brazil);
#if DEBUG
			result.Add("ER");
#endif
			return result;
		}
	}
}
