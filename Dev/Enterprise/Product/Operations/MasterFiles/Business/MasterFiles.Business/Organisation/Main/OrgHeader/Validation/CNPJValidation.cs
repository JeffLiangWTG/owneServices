using System.Globalization;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class CNPJValidator
	{
		public static bool ValidateCNPJ(ZString cNPJRaw)
		{
			ZString cNPJStripped = cNPJRaw.KeepChars(ZString.AlphanumericCharacters);
			if (cNPJStripped.Length != 14 || !cNPJStripped.IsNumbersOnlyOrEmpty)
			{
				return false;
			}

			int[] c = new int[14];

			for (var i = 0; i < 14; i++)
			{
				c[i] = int.Parse(cNPJStripped[i].ToString(), CultureInfo.InvariantCulture);
			}

			// See http://search.cpan.org/dist/Business-BR-Ids/lib/Business/BR/CNPJ.pm#THE_CHECK_EQUATIONS.
			var formula1 = (5 * c[0] + 4 * c[1] + 3 * c[2] + 2 * c[3] + 9 * c[4] + 8 * c[5] + 7 * c[6] + 6 * c[7] + 5 * c[8] + 4 * c[9] + 3 * c[10] + 2 * c[11] + c[12]) % 11;
			var formula2 = (6 * c[0] + 5 * c[1] + 4 * c[2] + 3 * c[3] + 2 * c[4] + 9 * c[5] + 8 * c[6] + 7 * c[7] + 6 * c[8] + 5 * c[9] + 4 * c[10] + 3 * c[11] + 2 * c[12] + c[13]) % 11;

			return (formula1 == 0 || formula1 == 1 && c[12] == 0) && (formula2 == 0 || formula2 == 1 && c[13] == 0);
		}
	}
}
