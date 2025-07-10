using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public static class InBondNumberCheckDigitCalculator
	{
		public static ZString GetCheckDigit(ZString eightDigitNumberWithoutCheckDigit)
		{
			ZInt inbondNumber = ZInt.ParseSafe(eightDigitNumberWithoutCheckDigit, -1);

			ZDecimal result = 0m;

			if (inbondNumber > 0)
			{
				ZDecimal theLastDigitOfRemainderByDivision = (inbondNumber / 7m * 10) % 10;

				result = theLastDigitOfRemainderByDivision * 0.7m;
			}
			return result.Round(0).ToString();
		}

		public static ZInt CalculatePaperlessITNoCheckDigit(ZString inbondNumber)
		{
			//check if inbondNumber in correct format (because CalculatePaperlessITNoCheckDigit a public method that can be called from any place)
			if (!Regex.IsMatch(inbondNumber, @"^V[A-Z0-9]{2}[0-9]{8}$", RegexOptions.IgnoreCase))
			{
				return -1;
			}

			//Convert all positions containing alphabetic characters to the numeric
			//first position always 'V'
			ZString calculationString = GetDigitFromCharacter(inbondNumber.SubstringSafe(0, 1).ToUpper());

			if (inbondNumber.SubstringSafe(1, 1).IsLettersOnlyOrEmpty)
			{
				calculationString = calculationString + GetDigitFromCharacter(inbondNumber.SubstringSafe(1, 1));
			}
			else
			{
				calculationString = calculationString + inbondNumber.SubstringSafe(1, 1);
			}

			if (inbondNumber.SubstringSafe(2, 1).IsLettersOnlyOrEmpty)
			{
				calculationString = calculationString + GetDigitFromCharacter(inbondNumber.SubstringSafe(2, 1)) + inbondNumber.SubstringSafe(3, 7);
			}
			else
			{
				calculationString = calculationString + inbondNumber.SubstringSafe(2, 8);
			}

			//Start with the unit's position and multiply every other position by 2. Essentially all odd positions will be multiplied by 2.
			//If the result of the multiplication is greater than 9, add 1 to the unit's digit (right most digit) of the result and disregard the ten's digit.
			ZInt item1 = ZInt.ParseEmptyAsZero(calculationString.SubstringSafe(1, 1)) * 2;
			ZInt item2 = ZInt.ParseEmptyAsZero(calculationString.SubstringSafe(3, 1)) * 2;
			ZInt item3 = ZInt.ParseEmptyAsZero(calculationString.SubstringSafe(5, 1)) * 2;
			ZInt item4 = ZInt.ParseEmptyAsZero(calculationString.SubstringSafe(7, 1)) * 2;
			ZInt item5 = ZInt.ParseEmptyAsZero(calculationString.SubstringSafe(9, 1)) * 2;

			ZInt result1 = item1 > 9 ? ZInt.ParseEmptyAsZero(item1.ToString().Substring(1, 1)) + 1 : (int)item1;
			result1 += item2 > 9 ? ZInt.ParseEmptyAsZero(item2.ToString().Substring(1, 1)) + 1 : (int)item2;
			result1 += item3 > 9 ? ZInt.ParseEmptyAsZero(item3.ToString().Substring(1, 1)) + 1 : (int)item3;
			result1 += item4 > 9 ? ZInt.ParseEmptyAsZero(item4.ToString().Substring(1, 1)) + 1 : (int)item4;
			result1 += item5 > 9 ? ZInt.ParseEmptyAsZero(item5.ToString().Substring(1, 1)) + 1 : (int)item5;

			//Total all even positions starting with the position adjacent to the unit's position.
			ZInt result2 = ZInt.ParseEmptyAsZero(calculationString.SubstringSafe(0, 1)) +
				ZInt.ParseEmptyAsZero(calculationString.SubstringSafe(2, 1)) +
				ZInt.ParseEmptyAsZero(calculationString.SubstringSafe(4, 1)) +
				ZInt.ParseEmptyAsZero(calculationString.SubstringSafe(6, 1)) +
				ZInt.ParseEmptyAsZero(calculationString.SubstringSafe(8, 1));

			//Add the sums from the preceding two-steps
			//Subtract the unit's digit from 10. The result is the check digit.
			ZInt result = (result1 + result2).ToString().Length > 1 ? 10 - ZInt.ParseEmptyAsZero((result1 + result2).ToString().Substring(1, 1)) : -1;

			//Normally, the result of the arithmetic will be a single digit.
			//In instances when the unit's digit (in the previous step) equals 0, the check digit will be 0.
			//result can not exceed 10, but may be equal 10
			if (result == 10)
			{
				result = 0;
			}

			return result;
		}

		static ZString GetDigitFromCharacter(ZString character)
		{
			ZString result = ZString.Empty;

			if (character == "A" | character == "J")
			{
				result = "1";
			}
			else if (character == "B" | character == "K" | character == "S")
			{
				result = "2";
			}
			else if (character == "C" | character == "L" | character == "T")
			{
				result = "3";
			}
			else if (character == "D" | character == "M" | character == "U")
			{
				result = "4";
			}
			else if (character == "E" | character == "N" | character == "V")
			{
				result = "5";
			}
			else if (character == "F" | character == "O" | character == "W")
			{
				result = "6";
			}
			else if (character == "G" | character == "P" | character == "X")
			{
				result = "7";
			}
			else if (character == "H" | character == "Q" | character == "Y")
			{
				result = "8";
			}
			else if (character == "I" | character == "R" | character == "Z")
			{
				result = "9";
			}

			return result;
		}
	}
}
