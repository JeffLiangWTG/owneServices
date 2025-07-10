using System;

namespace Enterprise.Freight.Business
{
	public static class Base26NumberConverter
	{
		public static int GetNumberRepresentation(string text)
		{
			if (text.Length > maxLetterRepresentation.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(text),
					FormattableString.Invariant($"Acceptable range is A to {maxLetterRepresentation} inclusive."));
			}
			int result = 0;
			var divisor = (int)Math.Pow(alphabet.Length, text.Length);
			for (int i = 0; i < text.Length; i++)
			{
				divisor /= alphabet.Length;
				result += (alphabet.IndexOf(text[i]) + 1) * divisor;
			}

			return result;
		}

		public static string GetLetterRepresentation(int number)
		{
			var base26 = ConvertToBase26(number);
			if (base26.Length == 0)
			{
				return string.Empty;
			}

			var result = string.Empty;
			char[] alphabetArray = alphabet.ToCharArray();

			for (var i = base26.Length - 1; i >= 0; i--)
			{
				if (base26[i] <= 0)
				{
					base26[i] += alphabetArray.Length;
					if (i > 0)
					{
						base26[i - 1] -= 1;
					}
				}
			}

			for (var i = 0; i < base26.Length; i++)
			{
				result += alphabetArray[base26[i] - 1];
			}

			return result;
		}

		public static int[] ConvertToBase26(int number)
		{
			if (number <= 0)
			{
				return Array.Empty<int>();
			}
			else if (number > MaxNumberRepresentation)
			{
				throw new ArgumentOutOfRangeException(nameof(number),
					FormattableString.Invariant($"Acceptable range is 0 to {MaxNumberRepresentation} inclusive."));
			}

			var letterLength = GetLetterLength(number);
			var baseLength = alphabet.Length;
			var result = new int[letterLength];
			var divisor = (int)Math.Pow(baseLength, letterLength - 1);
			var remainder = number;

			for (var i = 0; i < letterLength; i++)
			{
				if (i == 0)
				{
					result[0] = number / divisor;
				}
				else
				{
					divisor /= baseLength;
					result[i] = remainder / divisor;
				}
				remainder -= result[i] * divisor;
			}

			return result;
		}

		public static int GetLetterLength(int number)
		{
			var maxLetters = alphabet.Length;
			int length = 1;
			while (number > maxLetters)
			{
				length++;
				maxLetters += (int)Math.Pow(alphabet.Length, length);
			}

			return length;
		}

		const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		public const int MaxNumberRepresentation = 321272406;
		const string maxLetterRepresentation = "ZZZZZZ";
	}
}
