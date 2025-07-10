using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public static class ExternalValidation
	{
		internal const string CAIsNotAValidCountryForCustomsMessagingPurpose = "'CA' is not a valid country of origin if the goods was exported from 'CA'. You should specify a province code starting with X.";
		internal const string CanadianProvinceCodeNotAllowedForCountryOfExport = "You should use 'CA' instead of its province code for the country of export";

		public static string GetErrorTextForCanadaCountryOfOrigin(string countryOfOrigin, string countryOfExport)
		{
			if (countryOfOrigin == Core.Constants.CountryCodes.Canada && countryOfExport == Core.Constants.CountryCodes.Canada)
			{
				return CAIsNotAValidCountryForCustomsMessagingPurpose;
			}
			return "";
		}

		public static string GetErrorTextForCanadaCountryOfExport(string countryOfExport)
		{
			if (CanadaProvinceTerritoryCodes.IsCanadianProvince(countryOfExport))
			{
				return CanadianProvinceCodeNotAllowedForCountryOfExport;
			}
			return "";
		}
	}

	class SpecialProgramIndicatorValidation
	{
		public SpecialProgramIndicatorValidation(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		public string GetErrorText(string countryOfOrigin, string countryOfExport, string spiCode, ZDateTime effectiveDutyDate, USCTariff tariff)
		{
			string result = string.Empty;
			string[] supportedCountryCodes = GetSupportedSPICountries(spiCode);

			var description = SpecialProgramList.GetCachedList(factory).GetDescriptionFromCode(spiCode);

			if (supportedCountryCodes.Length > 0)
			{
				bool hasMatchingCountry = ((IList<string>)supportedCountryCodes).Contains(countryOfOrigin);

				if (!hasMatchingCountry)
				{
					ZQuery query = new ZQuery(RefCountrySchema.RN_Code, supportedCountryCodes);
					RefCountry[] countries = factory.Load<RefCountry>(query);
					string[] countryNameList = new string[countries.Length];
					for (int i = 0; i < countries.Length; i++)
					{
						countryNameList[i] = countries[i].RN_DescMultilingual.GetUnresolvedString();
					}

					result = description + " is only supported when importing goods of an origin of " + EnglishListFormatter.GetString(countryNameList) + ".";
				}
			}

			switch (spiCode)
			{
				case SpecialProgramList.Codes.SG:
					if (countryOfExport != Core.Constants.CountryCodes.Singapore && tariff != null &&
						tariff.Applies(TariffRuleList.Codes.SGFTA, effectiveDutyDate) &&
						!tariff.Applies(TariffRuleList.Codes.SingaporePreferenceLevel, effectiveDutyDate))
					{
						result = SingaporeFTAError;
					}
					break;
				case SpecialProgramList.Codes.CL:
					if (countryOfExport != Core.Constants.CountryCodes.Chile && tariff != null &&
						!tariff.Applies(TariffRuleList.Codes.ChileanPreferenceLevel, effectiveDutyDate))
					{
						result = "Chile Free Trade Agreement requires that goods are exported directly from Chile, with the exception of Tariff Number '991199'";
					}
					break;

				case SpecialProgramList.Codes.IL:
				case SpecialProgramList.Codes.JO:
				case SpecialProgramList.Codes.MA:
				case SpecialProgramList.Codes.BH:
				case SpecialProgramList.Codes.KR:
				case SpecialProgramList.Codes.CO:
				case SpecialProgramList.Codes.PA:
				case SpecialProgramList.Codes.NP:
				case SpecialProgramList.Codes.OM:
				case SpecialProgramList.Codes.PE:
				case SpecialProgramList.Codes.AU:
				case SpecialProgramList.Codes.JP:
					if (countryOfExport != spiCode || countryOfOrigin != spiCode)
					{
						result = string.Format(CultureInfo.InvariantCulture, "{0} requires that goods are exported and originated directly from {1}.", description, GetCountryNameByCode(spiCode));
					}
					break;

				case SpecialProgramList.Codes.CA:
				case SpecialProgramList.Codes.MX:
					if (countryOfExport != Core.Constants.CountryCodes.Canada && countryOfExport != Core.Constants.CountryCodes.Mexico ||
						!(countryOfOrigin == spiCode || CanadaProvinceTerritoryCodes.IsCanadianProvince(countryOfOrigin) || CanadaProvinceTerritoryCodes.IsCanadianSoftwoodLumberRegion(countryOfOrigin)))
					{
						result = string.Format(CultureInfo.InvariantCulture, "{0} requires that goods are originated from {1} and exported from a NAFTA country.", description, GetCountryNameByCode(spiCode));
					}
					break;
				case SpecialProgramList.Codes.S:
				case SpecialProgramList.Codes.SPlus:
					if (tariff != null && (tariff.HasSpecialProgramsIndicator(SpecialProgramList.Codes.S) || tariff.HasSpecialProgramsIndicator(SpecialProgramList.Codes.SPlus)) && tariff.Applies(UniversalReferenceConstants.TariffAttributeTypes.Values.USMCA_REPAIR, effectiveDutyDate))
					{
						if (countryOfExport != Core.Constants.CountryCodes.UnitedStates && countryOfExport != Core.Constants.CountryCodes.Canada && countryOfExport != Core.Constants.CountryCodes.Mexico)
						{
							result = string.Format(CultureInfo.InvariantCulture, "{0} requires that goods are exported from a USMCA country.", description);
						}
					}
					else
					{
						if (countryOfExport != Core.Constants.CountryCodes.UnitedStates && countryOfExport != Core.Constants.CountryCodes.Canada && countryOfExport != Core.Constants.CountryCodes.Mexico ||
						!(countryOfOrigin == Core.Constants.CountryCodes.UnitedStates || countryOfOrigin == Core.Constants.CountryCodes.Canada || countryOfOrigin == Core.Constants.CountryCodes.Mexico ||
						CanadaProvinceTerritoryCodes.IsCanadianProvince(countryOfOrigin) || CanadaProvinceTerritoryCodes.IsCanadianSoftwoodLumberRegion(countryOfOrigin)))
						{
							result = string.Format(CultureInfo.InvariantCulture, "{0} requires that goods are originated from {1} or {2} and exported from a NAFTA country.", description, GetCountryNameByCode(SpecialProgramList.Codes.CA), GetCountryNameByCode(SpecialProgramList.Codes.MX));
						}
					}
					break;
			}
			return result;
		}

		ZString GetCountryNameByCode(ZString countryCode)
		{
			var country = factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode);
			return country?.RN_DescMultilingual?.GetUnresolvedString() ?? countryCode;
		}

		internal const string SingaporeFTAError = "Singapore Free Trade Agreement requires that goods are exported directly from Singapore";

		//Please add a country to this method if you need to validate against Country of Origin only. If Country of Export is validated, please see the example of SPI AU.
		string[] GetSupportedSPICountries(string spiCode)
		{
			switch (spiCode)
			{
				case SpecialProgramList.Codes.BSharp:
					return new string[] {
						Core.Constants.CountryCodes.Canada
					};

				case SpecialProgramList.Codes.CL:
					return new string[] {
						Core.Constants.CountryCodes.Chile
					};

				case SpecialProgramList.Codes.JPlus:
					return new string[] {
						Core.Constants.CountryCodes.Bolivia,
						Core.Constants.CountryCodes.Colombia,
						Core.Constants.CountryCodes.Ecuador,
						Core.Constants.CountryCodes.Peru
					};

				case SpecialProgramList.Codes.KSharp:
				case SpecialProgramList.Codes.LSharp:
				case SpecialProgramList.Codes.CSharp:
					return new string[] {
						Core.Constants.CountryCodes.Canada,
						Core.Constants.CountryCodes.Mexico
					};

				case SpecialProgramList.Codes.SG:
					return new string[] {
						Core.Constants.CountryCodes.Singapore
					};

				default:
					return System.Array.Empty<string>();
			}
		}
	}

	static class EnglishListFormatter
	{
		public static string GetString(string[] elements)
		{
			StringBuilder result = new StringBuilder();
			int lastElement = elements.Length - 2;
			int secondLastElement = elements.Length - 3;
			for (int i = 0; i < elements.Length; i++)
			{
				result.Append(elements[i]);
				if (i <= secondLastElement)
				{
					result.Append(", ");
				}
				else if (i == lastElement)
				{
					result.Append(" or ");
				}
			}
			return result.ToString();
		}
	}
}
