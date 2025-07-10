using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public static class AESCountryCodeValidator
	{
		public static void ValidateOrgCountryForMyanmar(ZPropertyInfo propertyInfo, OrgHeader organisation, ZDateTime exportDate)
		{
			if (organisation != null)
			{
				ValidateCountryForMyanmar(propertyInfo, organisation.CountryCode, exportDate);
			}
		}

		public static void ValidateCountryForMyanmar(ZPropertyInfo propertyInfo, ZString countryCode, ZDateTime exportDate)
		{
			if (IsCountryCodeBUValid(exportDate))
			{
				if (countryCode == Core.Constants.CountryCodes.Myanmar)
				{
					propertyInfo.AddWarning(CountryCodeBUWillBeSentInsteadOfMM);
				}
			}
			else
			{
				if (countryCode == USCCountry.Burma)
				{
					propertyInfo.AddWarning(CountryCodeBUNotAcceptable);
				}
			}
		}
		public const string CountryCodeBUWillBeSentInsteadOfMM = "AES Does not accept MM (Myanmar) as a country code, BU will be sent in the message in its place (Burma).";
		public const string CountryCodeBUNotAcceptable = "As of 11 SEP 2019, AES no longer accepts BU (Burma) as a country code, please use MM (Myanmar) instead.";

		static bool IsCountryCodeBUValid(ZDateTime exportDate)
		{
			return !exportDate.IsEmpty && exportDate < new ZDate(2019, 9, 11);
		}
	}
}
