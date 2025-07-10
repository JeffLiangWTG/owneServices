using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class ISFTariffValidatorTest : TestCaseWithFactory
	{
		[TestDate(2014, 04, 03)]
		public void TestFormattedHarmonisedNum()
		{
			var header = Factory.New<CusISFHeader>();
			var line = header.Lines.AddNew();
			var version = USCDataVersion.GetLastHTSAttempt(Factory);
			version.UZ_Version = 901;
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "11111010";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			Factory.Save();
			header.BF_NumOfHarmChars = NumberOfHarmonizedDigitsList.Codes.Eight;
			string tariffNotFoundMessage = "not recognized as a valid tariff.";
			string tariffDataVersionNotMatchMessage = "is not recognized.";
			string tariffLengthNotEnoughMessage = "is too short;";
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			line.BL_FormattedHarmonisedNum = ZString.Empty;
			AssertNoMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, tariffNotFoundMessage);
			AssertNoMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, tariffLengthNotEnoughMessage);
			line.BL_FormattedHarmonisedNum = "1010101010";
			AssertNoMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, tariffNotFoundMessage);
			AssertNoMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, tariffLengthNotEnoughMessage);
			AssertHasWarningContaining(line.BL_FormattedHarmonisedNumInfo, tariffDataVersionNotMatchMessage);
			line.BL_FormattedHarmonisedNum = "22221010";
			AssertNoMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, tariffNotFoundMessage);
			AssertNoMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, tariffLengthNotEnoughMessage);
			line.BL_FormattedHarmonisedNum = "111110";
			AssertNoMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, tariffNotFoundMessage);
			AssertHasMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, tariffLengthNotEnoughMessage);
			header.BF_EntryType = SubmissionTypeList.Codes.ISF5;
			line.BL_FormattedHarmonisedNum = ZString.Empty;
			AssertNoMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, tariffNotFoundMessage);
			AssertNoMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, tariffLengthNotEnoughMessage);
			line.BL_FormattedHarmonisedNum = "1010101010";
			AssertNoMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, tariffNotFoundMessage);
			AssertNoMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, tariffLengthNotEnoughMessage);
			AssertHasWarningContaining(line.BL_FormattedHarmonisedNumInfo, tariffDataVersionNotMatchMessage);
			line.BL_FormattedHarmonisedNum = "22221010";
			AssertNoMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, tariffNotFoundMessage);
			AssertNoMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, tariffLengthNotEnoughMessage);
			line.BL_FormattedHarmonisedNum = "111110";
			AssertNoMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, tariffNotFoundMessage);
			AssertHasMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, tariffLengthNotEnoughMessage);
			version.UZ_Version = 1403;
			header.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			line.BL_FormattedHarmonisedNum = ZString.Empty;
			AssertNoMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, tariffNotFoundMessage);
			AssertNoMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, tariffLengthNotEnoughMessage);
			line.BL_FormattedHarmonisedNum = "11111010";
			AssertNoMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, tariffNotFoundMessage);
			AssertNoMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, tariffLengthNotEnoughMessage);
			AssertNoWarningContaining(line.BL_FormattedHarmonisedNumInfo, tariffDataVersionNotMatchMessage);
			line.BL_FormattedHarmonisedNum = "22221010";
			AssertHasMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, tariffNotFoundMessage);
			AssertNoMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, tariffLengthNotEnoughMessage);
			line.BL_FormattedHarmonisedNum = "111110";
			AssertNoMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, tariffNotFoundMessage);
			AssertHasMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, tariffLengthNotEnoughMessage);
			header.BF_EntryType = SubmissionTypeList.Codes.ISF5;
			line.BL_FormattedHarmonisedNum = ZString.Empty;
			AssertNoMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, tariffNotFoundMessage);
			AssertNoMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, tariffLengthNotEnoughMessage);
			line.BL_FormattedHarmonisedNum = "11111010";
			AssertNoMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, tariffNotFoundMessage);
			AssertNoMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, tariffLengthNotEnoughMessage);
			AssertNoWarningContaining(line.BL_FormattedHarmonisedNumInfo, tariffDataVersionNotMatchMessage);
			line.BL_FormattedHarmonisedNum = "22221010";
			AssertHasMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, tariffNotFoundMessage);
			AssertNoMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, tariffLengthNotEnoughMessage);
			line.BL_FormattedHarmonisedNum = "111110";
			AssertNoMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, tariffNotFoundMessage);
			AssertHasMessageErrorContaining(line.BL_FormattedHarmonisedNumInfo, tariffLengthNotEnoughMessage);
		}
	}
}
