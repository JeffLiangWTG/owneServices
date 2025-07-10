using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ManufacturerIDValidatorTest : TestCaseWithFactory
	{
		public void TestIsMaximumLengthExceeded()
		{
			AssertEquals("Is ID valid - MaximumLengthExceeded", false, ManufacturerIDValidator.IsMinMaxAndAlphaNumeric("1234567890123456"));
			AssertEquals("Is ID valid - MaximumLengthExceeded", true, ManufacturerIDValidator.IsMinMaxAndAlphaNumeric("G32312312323232"));
		}

		public void TestIsISOCountryCodeValid()
		{
			USCCountry country = Factory.New<USCCountry>();
			country.UC_Code = "G4";
			ManufacturerIDValidator validator = new ManufacturerIDValidator(Factory);
			AssertEquals("IsISOCountryCodeValid", true, validator.IsISOCountryCodeValid("G42312312323232"));
			AssertEquals("IsISOCountryCodeValid", false, validator.IsISOCountryCodeValid("G32312312323232"));
			AssertEquals("IsISOCountryCodeValid", false, validator.IsISOCountryCodeValid(""));
		}

		public void TestIsMinimumLengthEntered()
		{
			AssertEquals("IsMinimumLengthEntered", true, ManufacturerIDValidator.IsMinMaxAndAlphaNumeric("AU55523351"));
			AssertEquals("IsMinimumLengthEntered", false, ManufacturerIDValidator.IsMinMaxAndAlphaNumeric("GB2312"));
			AssertEquals("IsMinimumLengthEntered", true, ManufacturerIDValidator.IsMinMaxAndAlphaNumeric("GB23123"));
		}

		public void TestIsAlphaNumeric()
		{
			AssertEquals("IsAlphaNumeric", true, ManufacturerIDValidator.IsMinMaxAndAlphaNumeric("AU55523351"));
			AssertEquals("IsAlphaNumeric", false, ManufacturerIDValidator.IsMinMaxAndAlphaNumeric("GB#1084"));
			AssertEquals("IsAlphaNumeric", false, ManufacturerIDValidator.IsMinMaxAndAlphaNumeric("GB-TT279"));
			AssertEquals("IsAlphaNumeric", false, ManufacturerIDValidator.IsMinMaxAndAlphaNumeric("GB129*279"));
		}

		public void TestIsUseOnlyUpperCase()
		{
			AssertEquals("Only upper case", true, ManufacturerIDValidator.IsOnlyUpperCase("XYBEREQU6LON"));
			AssertEquals("Mixed case", false, ManufacturerIDValidator.IsOnlyUpperCase("XYBEREQU6lon"));
		}

		public void TestIsMIDCanadianAndValid()
		{
			AssertEquals(false, ManufacturerIDValidator.IsMIDCanadianAndValid(ZString.Empty, "G32312312323232"));
			AssertEquals(false, ManufacturerIDValidator.IsMIDCanadianAndValid(CanadaProvinceTerritoryCodes.Codes.XO, "G32312312323232"));
			AssertEquals(true, ManufacturerIDValidator.IsMIDCanadianAndValid(CanadaProvinceTerritoryCodes.Codes.XO, "XO12312323232"));
			AssertEquals(false, ManufacturerIDValidator.IsMIDCanadianAndValid(Core.Constants.CountryCodes.Canada, "G32312312323232"));
			AssertEquals(true, ManufacturerIDValidator.IsMIDCanadianAndValid(Core.Constants.CountryCodes.Canada, CanadaProvinceTerritoryCodes.Codes.XM + "2312312323232"));
		}
	}
}
