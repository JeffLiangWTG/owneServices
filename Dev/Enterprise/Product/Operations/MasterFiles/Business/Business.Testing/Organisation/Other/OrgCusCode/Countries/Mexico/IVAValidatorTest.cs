using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	class IVAValidatorTest : TestCaseWithFactory
	{
		string CodeType => OrgCusCode.CodeTypes.IVA;
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.MXIVA;
		string CountryCode => Core.Constants.CountryCodes.Mexico;

		public void TestIVACodeLengthValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidLength, validCodes, InvalidLengthMessage, OrganisationRegistryCodeType);
		}

		public void TestIVACodePatternValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, CodeType, codesWithInvalidPattern, validCodes, InvalidPatternMessage, OrganisationRegistryCodeType);
		}

		readonly ZString[] codesWithInvalidLength = new ZString[] { "12345678901", "12345678901234", "123456789" };
		readonly ZString[] codesWithInvalidPattern = new ZString[] { "XAX101010100Y", "R1NA85&821l78", "OLM14GG144Z7", "AME880912I&G", "PES1HY212&G7", "1AN700817300", "1AN700G173R5", "aab123456GHK", "AA&123456yyy" };
		readonly ZString[] validCodes = new ZString[] { "XAXX010101000", "XEXX010101000", "COFV880526RB7", "CAHG840609LA9", "ZUAC730724TX3", "AORM860704HD8", "GOGL830125KT9", "IEX000204AY1", "LAA170906RT2", "TLU990111JN8",
												"DGF920601FY6", "ELM071127GC8", "MEI060428UZ6", "SCL090414QE0", "KME850218KA9", "SIN990927BY1", "NCM080123TZ1", "FEH940630UG2", "TTR990621JJ0", "GWM790307NBO", "DAA020218JY1", "PE&620117SL5",
												"P&G4803059U8", "BAP8907061J9", "ELO0112145R5", "ULM1509029A1", "OLM1402144Z7", "SCL1104142W4", "PES1112122G7", "LAN7008173R5", "CLO970618IEA", "CAL990622R4A", "YLM081111665", "TME950302B63",
												"MME940615R36", "STI120720P88", "GMO071107H23", "TEX100507L41", "AME880912I89" };

		string InvalidLengthMessage
		{
			get { return @"The IVA registration code length is invalid.

IVA codes must be 12 or 13 digits long."; }
		}

		string InvalidPatternMessage
		{
			get { return @"The IVA registration code pattern is invalid.

Valid patterns are:
	XXXnnnnnnYYY
	XXXXnnnnnnYYY
	XXXXnnnnnnnnn

with 'X' an uppercase alphabetic character or '&' character; 'n' a digit from 0 to 9; 'Y' an alphanumeric character."; }
		}
	}
}
