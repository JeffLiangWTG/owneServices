using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ITCusCodeValidationSelectorTest : TestCase
	{
		public void TestGetCODCodeValidator()
		{
			CombineAssertions(() =>
			{
				AssertType<ITElevenDigitVATCodeValidator>(ITCusCodeValidationSelector.GetCODCodeValidator("00891230153"));
				AssertType<ITFiscalCodeValidator>(ITCusCodeValidationSelector.GetCODCodeValidator("ABCDEF12B01Q123A"));
				AssertType<ITNotSupportedCodeValidator>(ITCusCodeValidationSelector.GetCODCodeValidator(""));
				AssertType<ITNotSupportedCodeValidator>("When Code Length more than 16", ITCusCodeValidationSelector.GetCODCodeValidator("ABCDEF12B01Q123A121"));
				AssertType<ITNotSupportedCodeValidator>("When Code Length less than 11", ITCusCodeValidationSelector.GetCODCodeValidator("008912301"));
				AssertType<ITNotSupportedCodeValidator>("When Code is 13 digit VAT Code", ITCusCodeValidationSelector.GetCODCodeValidator("IT00891230153"));
			});
		}

		public void TestGetIVACodeValidator()
		{
			CombineAssertions(() =>
			{
				AssertType<ITElevenDigitVATCodeValidator>("When 11 Digit VAT Code", ITCusCodeValidationSelector.GetIVACodeValidator("00891230153"));
				AssertType<ITThirteenDigitVATCodeValidator>("When 13 Digit VAT Code", ITCusCodeValidationSelector.GetIVACodeValidator("IT00891230153"));
				AssertType<ITNotSupportedCodeValidator>(ITCusCodeValidationSelector.GetIVACodeValidator(""));
				AssertType<ITNotSupportedCodeValidator>("When Code Length more than 16", ITCusCodeValidationSelector.GetIVACodeValidator("ABCDEF12B01Q123A121"));
				AssertType<ITNotSupportedCodeValidator>("When Code Length less than 11", ITCusCodeValidationSelector.GetIVACodeValidator("008912301"));
			});
		}
	}
}
