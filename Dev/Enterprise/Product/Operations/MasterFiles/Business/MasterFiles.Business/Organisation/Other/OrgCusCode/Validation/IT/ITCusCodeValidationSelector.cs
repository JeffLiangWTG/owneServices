using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class ITCusCodeValidationSelector
	{
		public static IITCusCodeValidator GetIVACodeValidator(ZString code)
		{
			IITCusCodeValidator validator = code.Length switch
			{
				ITElevenDigitVATCodeValidator.VATCodeLength => new ITElevenDigitVATCodeValidator(),
				ITThirteenDigitVATCodeValidator.VATCodeLength => new ITThirteenDigitVATCodeValidator(),
				_ => new ITNotSupportedCodeValidator()
			};

			return validator;
		}

		public static IITCusCodeValidator GetCODCodeValidator(ZString code)
		{
			IITCusCodeValidator validator = code.Length switch
			{
				ITElevenDigitVATCodeValidator.VATCodeLength => new ITElevenDigitVATCodeValidator(),
				ITFiscalCodeValidator.FiscalCodeLength => new ITFiscalCodeValidator(),
				_ => new ITNotSupportedCodeValidator()
			};

			return validator;
		}
	}
}
