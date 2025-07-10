using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Summary description for TaiwanUnifiedBusinessNumberValidator.
	/// </summary>
	public static class TaiwanUnifiedBusinessNumberValidator
	{
		public static string GetValidateNotifyInformation(string number)
		{
			var result = string.Empty;
			if (HasExactlyEightDigits(number))
			{
				if (!IsValidVATNumber(number))
				{
					result = Res.GetString("E90166BC-4D96-42C0-8944-5B540B6823C5", "The Taiwan VAT number is invalid.");
				}
			}
			else
			{
				result = Res.GetString("07CD9422-A7D6-4E94-814B-2611315544AD", "The Taiwan VAT number must be an 8-digit number.");
			}
			return result;
		}

		public static void Validate(ZPropertyInfo targetInfo, ZString code, INotificationType notificationType)
		{
			var result = GetValidateNotifyInformation(code);
			if (!string.IsNullOrEmpty(result))
			{
				targetInfo.AddNotification(notificationType, result);
			}
		}

		static bool HasExactlyEightDigits(string number) => new Regex("^[0-9]{8}$").IsMatch(number);

		static bool IsValidVATNumber(string number)
		{
			var denominator = ZDate.Today >= new ZDate(2023, 4, 1) ? 5 : 10;
			var sumList = GetVATNumberSumList(number);
			return sumList.Any(x => x % denominator == 0);
		}

		static List<int> GetVATNumberSumList(string number)
		{
			var result = new List<int>();
			var multipliers = number.ToArray().Select(x => (int)char.GetNumericValue(x)).ToArray();
			var sumByNumberDigitsArray = GetSumByNumberDigitsArray(multipliers);
			var sumByNumberDigits = sumByNumberDigitsArray.Sum();
			if (multipliers[6] == 7)
			{
				result.Add(sumByNumberDigits - sumByNumberDigitsArray[6]);
				result.Add(sumByNumberDigits - sumByNumberDigitsArray[6] + 1);
			}
			else
			{
				result.Add(sumByNumberDigits);
			}
			return result;
		}

		static int[] GetSumByNumberDigitsArray(int[] multipliers)
		{
			var result = new int[8];
			var logicalMultiplicand = new int[] { 1, 2, 1, 2, 1, 2, 4, 1 };
			for (int i = 0; i < 8; i++)
			{
				result[i] = SumByNumberDigits(multipliers[i] * logicalMultiplicand[i]);
			}
			return result;
		}

		static int SumByNumberDigits(int number)
		{
			var sum = 0;
			while (number > 0)
			{
				sum += number % 10;
				number /= 10;
			}
			return sum;
		}
	}
}
