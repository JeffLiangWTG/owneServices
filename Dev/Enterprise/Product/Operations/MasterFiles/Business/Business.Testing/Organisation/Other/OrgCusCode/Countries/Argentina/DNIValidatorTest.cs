using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DNIValidatorTest : TestCaseWithFactory
	{
		string CodeType => ArgentinaOrgCusCodeInfo.OrgCusCodes.DNI;
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.ARDNI;
		string CountryCode => Core.Constants.CountryCodes.Argentina;

		public void TestDNICodeLengthValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidLength, validCodes, InvalidLengthMessage, OrganisationRegistryCodeType);
		}

		public void TestDNICodePatternValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidPatternAndValidLength, validCodes, InvalidPatternMessage, OrganisationRegistryCodeType);
		}

		string InvalidLengthMessage => @"The DNI registration code length is invalid.
DNI codes must be 7, 8, 9 or 10 digits long.";
		string InvalidPatternMessage => @"The DNI registration code pattern is invalid.

Valid patterns are:
	nnnnnnnn
	nn.nnn.nnn
	nnnnnnn
	n.nnn.nnn

with 'n' is a digit from 0 to 9. 
Please verify that you are entering a correct number.";

		readonly ZString[] validCodes = new ZString[] {  "34813676","34.813.676","22008323","22.008.323","10975339","10.975.339","71488885","71.488.885","35140285","35.140.285",
											"71495144","71.495.144","71498983","71.498.983","22051392","22.051.392","71480682","71.480.682","93316577","93.316.577",
											"5924187","5.924.187","5362535","5.362.535","2151943","2.151.943","1065799","1.065.799","1640438","1.640.438" };

		readonly ZString[] codesWithInvalidLength = new ZString[] { "123456789012", "123456789013", "1", "12", "123", "1234", "12345", "123456" };

		readonly ZString[] codesWithInvalidPatternAndValidLength = new ZString[] { "34.81.676", "348.13.676", ".81.3676", "348..13676", "ABCDEFGHI", "AB.CDE.FGH", "1.2333.456" };
	}
}
