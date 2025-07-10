using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	class HUIDMCodeValidatorTest : TestCaseWithFactory
	{
		public void TestCodeLengthValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidLength, validCodes, InvalidPatternMessage, OrganisationRegistryCodeType);
		}

		public void TestCodePatternValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidPatternAndValidLength, validCodes, InvalidPatternMessage, OrganisationRegistryCodeType);
		}

		#region Implementation

		string CountryCode => Core.Constants.CountryCodes.Hungary;

		string CodeType => HungaryOrgCusCodeInfo.OrgCusCodes.IDM;

		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.HUIDM;

		readonly ZString[] validCodes = new ZString[] { "EDI", "PAPER", "ELECTRONIC", "UNKNOWN" };

		readonly ZString[] codesWithInvalidLength = new ZString[] { "1", "12", "1234", "123456", "123456789", "HUX12345678", "HU1234567" };

		readonly ZString[] codesWithInvalidPatternAndValidLength = new ZString[] { "123", "1234567", "12345", "1234657890" };

		string InvalidPatternMessage => "Expected values for Hungary Invoice Delivery Method are 'EDI', 'PAPER', 'ELECTRONIC' or 'UNKNOWN'.";

		#endregion
	}
}
