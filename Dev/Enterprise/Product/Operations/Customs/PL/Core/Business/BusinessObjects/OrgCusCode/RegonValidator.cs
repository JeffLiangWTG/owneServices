using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business;

class REGONValidator
{
	internal bool Validate(ZPropertyInfo codeInfo)
	{
		var value = (ZString)codeInfo.Value;

		var valueLength = value.Length;
		if (valueLength != OrgCusCodesLength.RegonAcceptedLength_9 && valueLength != OrgCusCodesLength.RegonAcceptedLength_14 || !value.IsNumbersOnlyOrEmpty)
		{
			codeInfo.AddError(Res.GetString("PLREGONValidator|InvalidLengthMessage", "REGON number must consist of {0} or {1} digits and correct control digit.", OrgCusCodesLength.RegonAcceptedLength_9, OrgCusCodesLength.RegonAcceptedLength_14));
			return false;
		}

		if (!IsValidCheckDigit(value))
		{
			codeInfo.AddError(Res.GetString("PLREGONValidator|InvalidCheckDigitMessage", "REGON number is not valid."));
			return false;
		}

		return true;
	}

	static bool IsValidCheckDigit(ZString code)
	{
		switch (code.Length)
		{
			case OrgCusCodesLength.RegonAcceptedLength_9:
				return Check9DigitsOfRegon(code);
			case OrgCusCodesLength.RegonAcceptedLength_14:
				return Check14DigitsOfRegon(code);
			default:
				return false;
		}
	}

	static bool Check9DigitsOfRegon(ZString code)
	{
		var sum = RegonMultiplier(code[0], 8) +
				RegonMultiplier(code[1], 9) +
				RegonMultiplier(code[2], 2) +
				RegonMultiplier(code[3], 3) +
				RegonMultiplier(code[4], 4) +
				RegonMultiplier(code[5], 5) +
				RegonMultiplier(code[6], 6) +
				RegonMultiplier(code[7], 7);

		return IsREGONControlNumberValid(sum, code[8]);
	}

	static bool Check14DigitsOfRegon(ZString code)
	{
		var sum = RegonMultiplier(code[0], 2) +
				RegonMultiplier(code[1], 4) +
				RegonMultiplier(code[2], 8) +
				RegonMultiplier(code[3], 5) +
				RegonMultiplier(code[4], 0) +
				RegonMultiplier(code[5], 9) +
				RegonMultiplier(code[6], 7) +
				RegonMultiplier(code[7], 3) +
				RegonMultiplier(code[8], 6) +
				RegonMultiplier(code[9], 1) +
				RegonMultiplier(code[10], 2) +
				RegonMultiplier(code[11], 4) +
				RegonMultiplier(code[12], 8);

		return IsREGONControlNumberValid(sum, code[13]);
	}

	static int RegonMultiplier(char value, int multiplier) => value.ToInt() * multiplier;

	static bool IsREGONControlNumberValid(int sumOfRegonNumbers, char expectedControlNumber)
	{
		var controlNumber = sumOfRegonNumbers % 11;
		controlNumber = controlNumber == 10 ? 0 : controlNumber;

		return controlNumber == expectedControlNumber.ToInt();
	}
}
