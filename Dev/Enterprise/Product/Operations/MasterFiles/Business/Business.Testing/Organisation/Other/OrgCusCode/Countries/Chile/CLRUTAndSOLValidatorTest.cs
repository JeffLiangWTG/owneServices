using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CLRUTAndSOLValidatorTest : TestCaseWithFactory
	{
		string OrganisationRegistryCodeType => OrganisationRegistry.RegistrationNumberFormatFields.CLRUTSOL;
		string CountryCode => Core.Constants.CountryCodes.Chile;

		public void TestCLRUTSOLCodeLengthValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, ChileOrgCusCodeInfo.OrgCusCodes.RUT, codesWithInvalidLength, validCodes, InvalidLengthMessage, OrganisationRegistryCodeType);
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, ChileOrgCusCodeInfo.OrgCusCodes.SOL, codesWithInvalidLength, validCodes, InvalidLengthMessage, OrganisationRegistryCodeType);
		}

		public void TestCLRUTSOLCodePatternValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, ChileOrgCusCodeInfo.OrgCusCodes.RUT, codesWithInvalidPatternAndValidLength, validCodes, InvalidPatternMessage, OrganisationRegistryCodeType);
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, ChileOrgCusCodeInfo.OrgCusCodes.SOL, codesWithInvalidPatternAndValidLength, validCodes, InvalidPatternMessage, OrganisationRegistryCodeType);
		}

		public void TestCLRUTSOLCodeCheckDigitValidation()
		{
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, ChileOrgCusCodeInfo.OrgCusCodes.RUT, codesWithInvalidCheckDigit, validCodes, InvalidCheckDigitMessage, OrganisationRegistryCodeType);
			OrgCusCodeValidatorTestHelper.AssertValidation(CountryCode, ChileOrgCusCodeInfo.OrgCusCodes.SOL, codesWithInvalidCheckDigit, validCodes, InvalidCheckDigitMessage, OrganisationRegistryCodeType);
		}

		string InvalidLengthMessage => @"The RUT / SOL registration code needs to be 8, 9, 10, 11 or 12 in length, including the check digit.";
		string InvalidPatternMessage => @"The RUT / SOL registration code pattern is invalid.

Valid patterns are:
	nn.nnn.nnn-x OR n.nnn.nnn-x
	nnnnnnnn-x OR nnnnnnn-x
	nnnnnnnnx OR nnnnnnnx

with 'n' a digit from 0 to 9 and X (check digit) either a digit from 0 to 9 or the letter 'K'.";
		string InvalidCheckDigitMessage => "The check digit in the RUT / SOL registration code is incorrect.";

		readonly ZString[] validCodes = new ZString[]
		{
			"12.768.491-K","24.262.811-K","20.570.356-K","22.247.454-K","17.701.757-4","20.577.198-0","19.942.739-3","19.376.184-4","03.465.498-0","76.053.604-0",
			"8.821.823-K","8.291.966-K","7.682.732-K","9.336.354-K","7.331.348-1","6.039.249-8","6.161.985-2","7.148.893-4","8.391.301-0",
			"18930396-3","13732104-1","11904929-6","21622915-0","14153033-K","13304969-K","20431070-K","24935273-K","07850392-0",
			"5440318-6","7307871-7","5894776-8","8953396-1","8339434-K","8252522-K","5028767-K","6170759-K","5422034-0",
			"174925054","127412537","219651961","242883624","17693316K","12014006K","12357611K","24917940K","018894750",
			"58858439","71741990","58797944","77609962","7614558K","6974124K","7063810K","9445526K","67613740"
		};

		readonly ZString[] codesWithInvalidLength = new ZString[]
		{
			"12.768.491-kk", "124.262.811-k", "319.942.739-3", "19.376.184-45",
			"8858439", "1741990", "5797944", "7609962", "714558k","232","DDDD","1"
		};

		readonly ZString[] codesWithInvalidPatternAndValidLength = new ZString[]
		{
			"12768.491-k","24262.811-k","20.570.356K",".247.454-K12","177017575-","-177017575",".177017575","177-017575","177.017575","177.017575.","12.768.491-k","20.570.356-k",
			"8821.823-k","8.291966-k",".682.7321-K","19.336.354-","174__5054","1.27.41.2","7.682.732-k",
			"189303963-","1-37321041","17693316U","12014006?","12357611&","(24917940","8339434-k",
			"5EE58439","717TT990","587979T4","7614558T","6974124t","7063810U","X9445526","FDRICOLAP","706 810U","6974124k",
		};

		readonly ZString[] codesWithInvalidCheckDigit = new ZString[]
		{
			"14.778.491-K","24.262.811-7","21.571.356-K","18.701.757-4","20.577.198-8","79.093.604-0",
			"8.821.821-K","8.291.966-5","7.332.348-1","6.039.249-K","1.111.985-2","7.111.893-4",
			"18931196-3","13732104-K","12204929-6","14935273-K","17850392-0",
			"7740318-6","7307871-K","5891276-8","8339434-1","8332522-K","1028767-K",
			"171925054","127111538","176933161","62014006K","123576118","14917940K","019994750",
			"89858439","72241990","6614558K","69741243","7033810K","94455263","67713740"
		};
	}
}
