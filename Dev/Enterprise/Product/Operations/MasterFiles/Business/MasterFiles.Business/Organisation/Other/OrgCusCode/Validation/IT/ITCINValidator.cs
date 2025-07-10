using System;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	public static class ITCINValidator
	{
		[ThreadSafe]
		static readonly string[] Matrix3 = new string[3] { "0011001100", "1000111100", "1000000011" };
		[ThreadSafe]
		static readonly string[] Matrix8 = new string[3] { "3404151526", "1246024613", "4123456701" };
		[ThreadSafe]
		static readonly string[] Matrix = new string[3] { "ABCDEFGH", "JKLMNPQR", "STUVWXYZ" };

		public static ZString CalculateCheckDigitAgainstSixCharLengthString(ZString stringToCheck) => CalculateCheckDigit(stringToCheck, 6);
		public static ZString CalculateCheckDigitAgainstEightCharLengthString(ZString stringToCheck) => CalculateCheckDigit(stringToCheck, 8);

		static ZString CalculateCheckDigit(ZString stringToCheck, int stringBaseLength)
		{
			var stringToCheckTrimmed = stringToCheck.Trim();
			if (stringToCheckTrimmed.IsEmpty)
			{
				return ZString.Empty;
			}
			else
			{
				stringToCheckTrimmed = stringToCheckTrimmed.TrimStart('0');
				if (stringToCheckTrimmed.Length > stringBaseLength)
				{
					throw new ArgumentOutOfRangeException(nameof(stringToCheck), Res.GetString("77B45404-42F1-4B5F-BF2F-5107421CD32F", "The string '{0}' (trimmed of spaces and leading zeros) can contain a maximum of {1} characters", stringToCheck, stringBaseLength));
				}
				return CalculateCIN(stringToCheckTrimmed.IsEmpty ? new ZString("0") : stringToCheckTrimmed, stringBaseLength);
			}
		}

		static ZString CalculateCIN(ZString lcTarget, int baseLen)
		{
			lcTarget = lcTarget.PadLeft(baseLen, '0');

			int lnLen = lcTarget.Length;
			int lnIndex = 0;
			var c3 = 0;
			var c8 = 0;

			int delta;
			while ((delta = lnLen - lnIndex) > 0)
			{
				var terna = lcTarget.Substring(lnIndex, delta >= 3 ? 3 : delta);

				c3 += CodiceMod3(terna);
				c8 += CodiceMod8(terna);

				lnIndex += 3;
			}

			return GetCIN(c3 % 3, c8 % 8);
		}

		static int CodiceMod3(string terna)
		{
			return CodiceMod(terna, Matrix3);
		}

		static int CodiceMod8(string terna)
		{
			return CodiceMod(terna, Matrix8);
		}

		static int CodiceMod(string terna, string[] matrix)
		{
			var cod = 0;
			var n = terna.Length;
			for (var i = 0; i < n; i++)
			{
				var x = terna.Substring(i, 1).ToInt();
				cod += int.Parse(matrix[i].Substring(x, 1), CultureInfo.InvariantCulture);
			}
			return cod;
		}

		static string GetCIN(int c3, int c8)
		{
			return Matrix[c3].Substring(c8, 1);
		}

		static int ToInt(this string input)
		{
			var result = 0;
			if (!string.IsNullOrEmpty(input) && char.IsDigit(input[0]))
			{
				result = int.Parse(new string(input.TakeWhile(x => char.IsDigit(x)).ToArray()), CultureInfo.InvariantCulture);
			}
			return result;
		}
	}
}
