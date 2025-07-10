using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public sealed class ITFiscalCodeValidator : IITCusCodeValidator
	{
		ITCusCodeValidationResult IITCusCodeValidator.Validate(ZString code)
		{
			var regexPattern = new Regex(@"^[A-Z]{6}[0-9]{2}[A-Z]{1}[0-9]{2}[A-Z]{1}[0-9]{3}[A-Z]{1}$");
			return regexPattern.IsMatch(code)
				? ITCusCodeValidationResult.Valid
				: ITCusCodeValidationResult.InvalidPattern;
		}

		internal const int FiscalCodeLength = 16;
	}
}
