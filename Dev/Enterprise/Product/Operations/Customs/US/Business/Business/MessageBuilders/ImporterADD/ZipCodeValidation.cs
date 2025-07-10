using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class ZipCodeValidation
	{
		public string ValidateForZipCode(BusinessObjectFactory factory, ZString zipCodeEntered, ZString stateEntered, ZString countryCode)
		{
			var result = ZString.Empty;
			if (!stateEntered.IsEmpty && !zipCodeEntered.IsEmpty)
			{
				if (countryCode == Core.Constants.CountryCodes.UnitedStates)
				{
					result = ValidateForUSZipCodeRange(factory, zipCodeEntered, stateEntered);
				}
				var invalidPostCodeDigits = ValidateForZipCodeFormat(zipCodeEntered, countryCode);
				result = result.IsEmpty ? invalidPostCodeDigits : invalidPostCodeDigits.IsEmpty ? result : ZString.Format("{0}\r\n{1}", result, invalidPostCodeDigits);
			}

			return result;
		}

		ZString ValidateForUSZipCodeRange(BusinessObjectFactory factory, ZString zipCodeEntered, ZString stateEntered)
		{
			var result = ZString.Empty;
			var zipCodeRanges = factory.Load<USCZipCode>(new ZQuery(USCZipCodeSchema.UZ_State, stateEntered));

			foreach (USCZipCode zipCodeRange in zipCodeRanges)
			{
				var userEntered = ZInt.ParseSafe(zipCodeEntered.Left(5), -1);

				var beginning = ZInt.ParseSafe(zipCodeRange.UZ_BeginZipCodeRange, -1);
				var end = ZInt.ParseSafe(zipCodeRange.UZ_EndZipCodeRange, -1);

				if (userEntered < beginning || userEntered > end)
				{
					if (result.IsEmpty)
					{
						result = string.Format(CultureInfo.InvariantCulture, InvalidUSZIPCodeEntered, zipCodeRange.UZ_State, zipCodeRange.UZ_BeginZipCodeRange, zipCodeRange.UZ_EndZipCodeRange);
					}
					else
					{
						result += ",\r\n" + string.Format(CultureInfo.InvariantCulture, AdditionalZIPCodeRange, zipCodeRange.UZ_BeginZipCodeRange, zipCodeRange.UZ_EndZipCodeRange);
					}
				}
				else
				{
					//Zip Code is in one of the valid ranges
					result = ZString.Empty;
					break;
				}
			}

			return result;
		}

		ZString ValidateForZipCodeFormat(ZString zipCodeEntered, ZString countryCode)
		{
			var result = ZString.Empty;
			if (countryCode.Equals(Core.Constants.CountryCodes.UnitedStates) && !Regex.IsMatch(zipCodeEntered, @"^[0-9]{5}(-?[0-9]{4})?$"))
			{
				result = InvalidPostCodeDigitsForUS;
			}
			else if (countryCode.Equals(Core.Constants.CountryCodes.Canada) && !Regex.IsMatch(zipCodeEntered, @"^[a-zA-Z0-9]{3}\s?[a-zA-Z0-9]{3}$"))
			{
				result = InvalidPostCodeDigitsForCA;
			}

			return result;
		}

		public string ValidateZipCodeLengthAndGetMessage(ZString zipCodeEntered, int? length)
		{
			var result = ZString.Empty;
			if (!zipCodeEntered.IsEmpty && length != null)
			{
				if (zipCodeEntered.Length != length)
				{
					result = string.Format(CultureInfo.InvariantCulture, InvalidPostCodeLength, length.ToString());
				}
				else
				{
					result = ZString.Empty;
				}
			}

			return result;
		}

		public const string InvalidUSZIPCodeEntered = "The zip code entered for {0} is not valid. The first five numbers of the zip code should fall between {1} and {2}";
		public const string AdditionalZIPCodeRange = "or between {0} and {1}";
		public const string InvalidPostCodeLength = "Postal code must have length {0}";
		public const string InvalidPostCodeDigitsForUS = "Postal code must have 5 or 9 digits";
		public const string InvalidPostCodeDigitsForCA = "Postal code must be 6 characters, such as 'AAAAAA' or 'AAA AAA'";

		public string ValidateCAZipCodeAndGetMessage(ZString zipCodeEntered)
		{
			var result = "";
			if (!Regex.IsMatch(zipCodeEntered, @"^[A-Z][0-9][A-Z] ?[0-9][A-Z][0-9]$", RegexOptions.IgnoreCase))
			{
				result = InvalidPostCodeForCA;
			}
			return result;
		}

		public const string InvalidPostCodeForCA = "For Canada, postal codes must be in the following format: ANAbNAN or ANANAN (where A=alphabetic, N=numeric, b-blank).";

		public string ValidateMXZipCodeAndGetMessage(ZString zipCodeEntered)
		{
			string result = "";

			if (!Regex.IsMatch(zipCodeEntered, @"^[0-9]{5}$"))
			{
				result = InvalidPostCodeForMX;
			}

			return result;
		}

		public const string InvalidPostCodeForMX = "For Mexico, postal codes must 5 characters and must be all numeric.";

		public void ValidatePostalCodeForChinaManufacturer(ZPropertyInfo propertyInfo, ZString country, ZString postalCode)
		{
			if (country == Core.Constants.CountryCodes.China && ZZCustomsFunctionality.PostalCodeIsRequiredForChinaMF)
			{
				if (postalCode.IsEmpty)
				{
					propertyInfo.AddMessageError(PostalCodeRequiredForChinaManufacturer);
				}
				else if (!Regex.IsMatch(postalCode, @"^[0-9]{6}$"))
				{
					propertyInfo.AddMessageError(PostalCodeIsInvalidForChinaManufacturer);
				}
			}
		}
		internal const string PostalCodeRequiredForChinaManufacturer = "Postal code required for Manufacturer.";
		internal const string PostalCodeIsInvalidForChinaManufacturer = "Invalid format for Postal Code, should be 6 digits.";

		public static void ValidateForEmptyZIPForUSAddress(ZPropertyInfo organisationRelatedInfo, IAddressDetails addressDetails = null)
		{
			if (!(addressDetails is JobDocAddress docAddress && docAddress.E2_AddressOverride) && organisationRelatedInfo.Value is ZGuid pk)
			{
				addressDetails = organisationRelatedInfo.BizObj.Factory.Load<OrgAddress>(pk);
			}

			if (addressDetails != null)
			{
				if (Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(addressDetails.Country) == Core.Constants.CountryCodes.UnitedStates
					&& addressDetails.PostCode.IsEmpty)
				{
					organisationRelatedInfo.AddMessageError(ZIPCanNotBeEmpty);
				}
			}
		}
		internal const string ZIPCanNotBeEmpty = "Please enter a zip code for the selected address.";
	}
}
