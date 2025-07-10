using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public static class VINHelper
	{
		public static bool IsValidVINWithTheNinthDigitCheck(ZString strVin)
		{
			var isValid = false;
			if (strVin.Length == 17)
			{
				strVin = strVin.ToUpper().Trim();
				var checkDigit = strVin[8];
				if (char.IsDigit(checkDigit) || checkDigit == 'X')
				{
					var intVinValue = 0;
					for (int i = 0; i < strVin.Length; i++)
					{
						var letter = strVin[i];
						if (!VinValueDictionary.ContainsKey(letter))
						{
							return false;
						}
						intVinValue += VinValueWeights[i] * (VinValueDictionary[letter]);
					}

					var intCheckDigit = checkDigit != 'X' ? (int)char.GetNumericValue(checkDigit) : 10;
					if ((intVinValue % 11) == intCheckDigit)
					{
						isValid = true;
					}
				}
			}
			return isValid;
		}

		static readonly ImmutableArray<int> VinValueWeights = new int[] { 8, 7, 6, 5, 4, 3, 2, 10, 0, 9, 8, 7, 6, 5, 4, 3, 2 }.ToImmutableArray();

		static readonly ImmutableDictionary<char, int> VinValueDictionary = new Dictionary<char, int>
		{
			{ 'A', 1 }, { 'B', 2 }, { 'C', 3 }, { 'D', 4 }, { 'E', 5 }, { 'F', 6 }, { 'G', 7 },
			{ 'H', 8 }, { 'J', 1 }, { 'K', 2 }, { 'L', 3 }, { 'M', 4 }, { 'N', 5 },
			{ 'P', 7 }, { 'R', 9 }, { 'S', 2 }, { 'T', 3 },
			{ 'U', 4 }, { 'V', 5 }, { 'W', 6 }, { 'X', 7 }, { 'Y', 8 }, { 'Z', 9 },
			{ '1', 1 }, { '2', 2 }, { '3', 3 }, { '4', 4 }, { '5', 5 }, { '6', 6 }, { '7', 7 }, { '8', 8 }, { '9', 9 }, { '0', 0 },
		}.ToImmutableDictionary();
	}
}
