using System;
using System.Globalization;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class SouthAfricanIDNumberValidationTest : SouthAfricanIDNumberValidation
	{
		public class TestCase : TestCaseWithFactory
		{
			public void TestCalculatedCheckDigitIs0()
			{
				ZString iDNumber = "7110285071080";
				int calculatedCheckDigit = new SouthAfricanIDNumberValidationTest().CalculateCheckDigit(iDNumber);
				int checkDigit = Convert.ToInt32(iDNumber.Substring(12, 1), CultureInfo.InvariantCulture);
				Assert("Valid", calculatedCheckDigit == checkDigit);
				AssertEquals(calculatedCheckDigit, 0);
			}

			public void TestSouthAfricanIDNumberValidation()
			{
				var orgHeader = Factory.New<OrgHeader>();
				var customsCode = orgHeader.CustomsCodes.AddNew();
				customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.SouthAfrica;
				customsCode.OK_CodeType = OrgCusCode.SouthAfricaCodeTypes.IDNumber;

				customsCode.OK_CustomsRegNo = "1234567890128";
				AssertEquals("There is no error in OK_CustomsRegNoInfo.", false, customsCode.OK_CustomsRegNoInfo.HasNotifications());

				customsCode.OK_CustomsRegNo = "1234567890123";
				AssertEquals("There is an error in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
				AssertHasErrorContaining(customsCode.OK_CustomsRegNoInfo, "The ID Number does not have a valid check-digit. The check-digit should be 8.");

				customsCode.OK_CustomsRegNo = "123456789012a";
				AssertEquals("There is an error in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
				AssertHasErrorContaining(customsCode.OK_CustomsRegNoInfo, "The ID Number must be 12 digits and a 13th check-digit.");

				customsCode.OK_CustomsRegNo = "123456789012";
				AssertEquals("There is an error in OK_CustomsRegNoInfo.", true, customsCode.OK_CustomsRegNoInfo.HasNotifications());
				AssertHasErrorContaining(customsCode.OK_CustomsRegNoInfo, "The ID Number must be 12 digits and a 13th check-digit.");
			}

			public void TestIsValidCheckDigit()
			{
				ZString iDNumber = "1234567890128";
				int calculatedCheckDigit = new SouthAfricanIDNumberValidationTest().CalculateCheckDigit(iDNumber);
				int checkDigit = Convert.ToInt32(iDNumber.Substring(12, 1), CultureInfo.InvariantCulture);
				Assert("Valid", calculatedCheckDigit == checkDigit);
				AssertEquals(calculatedCheckDigit, 8);
			}

			public void TestIsValidCheckDigitFalse()
			{
				ZString iDNumber = "1234567890123";
				int calculatedCheckDigit = new SouthAfricanIDNumberValidationTest().CalculateCheckDigit(iDNumber);
				int checkDigit = Convert.ToInt32(iDNumber.Substring(12, 1), CultureInfo.InvariantCulture);
				Assert("Invalid", calculatedCheckDigit != checkDigit);
				AssertEquals(calculatedCheckDigit, 8);
			}
		}
	}
}
