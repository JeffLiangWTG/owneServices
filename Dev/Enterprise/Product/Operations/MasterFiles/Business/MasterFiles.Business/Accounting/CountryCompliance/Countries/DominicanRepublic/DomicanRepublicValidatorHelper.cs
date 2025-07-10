using System.Globalization;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.CountryCompliance.DominicanRepublic
{
	internal static class DomicanRepublicValidatorHelper
	{
		#region Check Digit 10

		internal static bool ValidMod10CheckDigit(ZString code)
		{
			var factors = new int[] { 1, 2, 1, 2, 1, 2, 1, 2, 1, 2 };
			var accumulated = 0;

			for (int i = 0; i < factors.Length; i++)
			{
				if (!TryParseMulti(code[i], factors[i], out var multi))
				{
					return false;
				}
				accumulated += (multi <= 9 ? multi : multi % 10 + 1);
			}

			int total = accumulated - (10 * (accumulated / 10));
			if (total != 0)
			{
				total = 10 - total;
			}

			return int.Parse(code[code.Length - 1].ToString(), CultureInfo.InvariantCulture) == total;
		}

		#endregion

		#region Check Digit 11

		internal static bool ValidMod11CheckDigit(ZString code)
		{
			var factors = new int[] { 7, 9, 8, 6, 5, 4, 3, 2 };
			var accumulated = 0;

			for (int i = 0; i < factors.Length; i++)
			{
				if (!TryParseMulti(code[i], factors[i], out var multi))
				{
					return false;
				}
				accumulated += multi;
			}

			int total = accumulated - (11 * (accumulated / 11));

			if (total != 1)
			{
				total = total == 0 ? 2 : 11 - total;
			}

			return int.Parse(code[code.Length - 1].ToString(), CultureInfo.InvariantCulture) == total;
		}

		#endregion

		static bool TryParseMulti(char code, int factor, out int multi)
		{
			if (!int.TryParse(code.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var origin))
			{
				multi = 0;
				return false;
			}
			multi = origin * factor;
			return true;
		}
	}
}
