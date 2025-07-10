using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	sealed class ITElevenDigitVATCodeValidator : ITVatCodeValidator
	{
		protected override Regex GetPatternForValidation() => new Regex(@"^[0-9]{11}$");

		protected override ZString GetCleansedCode(ZString code) => code;

		internal const int VATCodeLength = 11;
	}
}
