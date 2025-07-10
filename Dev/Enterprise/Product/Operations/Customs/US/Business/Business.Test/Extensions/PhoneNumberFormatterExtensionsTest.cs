using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class PhoneNumberFormatterExtensionsTest : TestCase
	{
		public void TestGetLocalPhoneNumber()
		{
			var phoneNumber = (ZString)"+10123456789";
			AssertEquals("0123456789", phoneNumber.GetLocalPhoneNumber(Core.Constants.CountryCodes.UnitedStates));
		}

		public void TestGetUSFormattPhoneNumber()
		{
			var phoneNumber = (ZString)"1-303-555-1212";
			AssertEquals("3035551212", phoneNumber.GetUSFormattPhoneNumber());
			phoneNumber = (ZString)"+1 303-555-1212";
			AssertEquals("3035551212", phoneNumber.GetUSFormattPhoneNumber());
			phoneNumber = (ZString)"12345";
			AssertEquals("12345", phoneNumber.GetUSFormattPhoneNumber());
		}
	}
}
