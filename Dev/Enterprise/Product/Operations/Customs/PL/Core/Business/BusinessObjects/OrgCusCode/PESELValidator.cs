using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business;

class PESELValidator
{
	internal bool Validate(ZPropertyInfo codeInfo)
	{
		var value = (ZString)codeInfo.Value;

		if (value.Length != OrgCusCodesLength.PeselAcceptedLength)
		{
			codeInfo.AddError(Res.GetString("PLPESELValidator|InvalidLengthMessage", "The PESEL Number needs to be {0} in length.", OrgCusCodesLength.PeselAcceptedLength));
			return false;
		}

		if (!value.IsNumbersOnlyOrEmpty)
		{
			codeInfo.AddError(Res.GetString("PLPESELValidator|InvalidPatternMessage",
				@"The PESEL Number pattern is invalid. Valid pattern is: nnnnnnnnnnn with 'n' a digit from 0 to 9. Please verify that you are entering a correct number."));
			return false;
		}

		if (!IsValidCheckDigit(value))
		{
			codeInfo.AddError(Res.GetString("PLPESELValidator|InvalidCheckDigitMessage", "The check digit in the PESEL number is incorrect."));
			return false;
		}

		return true;
	}

	static bool IsValidCheckDigit(ZString code)
	{
		var digits = code.ToString().Select(x => x.ToInt()).ToArray();
		var weights = new[] { 1, 3, 7, 9, 1, 3, 7, 9, 1, 3, 1 };

		if (digits.Length != weights.Length)
		{
			return false;
		}

		var checkSum = digits.Select((digit, index) => digit * weights[index]).Sum();
		return checkSum % 10 == 0;
	}
}
