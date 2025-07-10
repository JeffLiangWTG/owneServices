using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	sealed class ITThirteenDigitVATCodeValidator : ITVatCodeValidator
	{
		protected override Regex GetPatternForValidation() => new Regex(@"^[I]{1}[T]{1}[0-9]{11}$");

		protected override ZString GetCleansedCode(ZString code) => code.Remove(0, 2);

		internal const int VATCodeLength = 13;
	}
}
