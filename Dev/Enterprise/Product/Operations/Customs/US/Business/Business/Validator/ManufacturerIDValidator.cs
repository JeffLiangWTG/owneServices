
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class ManufacturerIDValidator : ValidationProvider
	{
		#region Constants

		public static class Constants
		{
			public const string Format = "Manufacturer ID must be a minimum of 7, maximum of 15 characters, containing only alpha and numeric characters.";
			public const string UpperCase = "Manufacturer ID must be entered in CAPITAL LETTERS.";
			public const string ISOCode = "The first 2 characters of a Manufacturer ID should be the ISO code for the country. (Or in the case of Canada, the first 2 characters should be the Canadian province code eg 'XA', 'XC' etc.)";
			public const string Canadian = "The Manufacturer ID needs to start with a valid Canadian province code eg ‘XA’, ‘XC’ because goods originate in CA.";
			public const string Country = "The first two characters of a Manufacturer ID should be the same as the Country code. (Or in the case of Canada, the first 2 characters should be the Canadian province code eg 'XA', 'XC' etc.)";
			public const string InvalidMIDForTextile = "For textile tariffs, the Manufacturer ID (MID) must be from the Country of Origin of the goods.";
			public const string InvalidMIDForTextileCanadianProvinces = "For textile tariffs, the Manufacturer ID (MID) must be from the Country of Origin of the goods. The Canadian MID being used does not match the Canadian province of origin";
			public const string InvalidMIDForOrigin = "For Prior Notice, the Manufacturer ID (MID) must be from the Production Country of Origin of the goods.";
		}

		#endregion

		public ManufacturerIDValidator(IFactoryProvider factoryProvider)
			: base(factoryProvider)
		{
		}

		public bool IsISOCountryCodeValid(ZString value)
		{
			bool result = false;

			if (!value.IsEmpty)
			{
				USCCountry country = Factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, value.Left(2));
				result = (country != null);
			}

			return result;
		}

		public static bool IsMinMaxAndAlphaNumeric(ZString value)
		{
			return value.Length >= 7 && value.Length <= 15 && value.IsLettersAndNumbersOnlyOrEmpty;
		}

		public static bool IsOnlyUpperCase(ZString value)
		{
			string upperCaseValue = value.ToUpper();
			return value.CompareTo(upperCaseValue) == 0;
		}

		public static bool IsMIDCanadianAndValid(ZString countryOfOrigin, ZString mID)
		{
			bool result = false;

			if (!mID.IsEmpty)
			{
				if (CanadaProvinceTerritoryCodes.IsCanadianProvince(countryOfOrigin) || countryOfOrigin == Core.Constants.CountryCodes.Canada)
				{
					ZString provinceCode = mID.Left(2);
					result = CanadaProvinceTerritoryCodes.IsCanadianProvince(provinceCode);
				}
			}

			return result;
		}

		public static bool IsMIDValidForCountry(ZString mID, ZString countryCode)
		{
			if (countryCode == Core.Constants.CountryCodes.Canada)
			{
				return CanadaProvinceTerritoryCodes.IsCanadianProvince(mID.Left(2));
			}
			else
			{
				return countryCode == mID.Left(2);
			}
		}
	}
}
