using System;
using System.Text;

namespace Enterprise.MasterFiles.Business
{
	public static class Base32Helper
	{
		#region Fields

		const int Mask = 31;
		const int Shift = 5;

		#endregion

		public static byte[] FromBase32(string encoded)
		{
			if (encoded == null)
			{
				throw new ArgumentNullException(nameof(encoded));
			}

			// Remove whitespace and padding. Note: the padding is used as hint 
			// to determine how many bits to decode from the last incomplete chunk
			// Also, canonicalize to all upper case
			encoded = encoded.Trim().TrimEnd('=').ToUpper();
			if (encoded.Length == 0)
			{
				return Array.Empty<byte>();
			}

			var outLength = encoded.Length * Shift / 8;
			var result = new byte[outLength];
			var buffer = 0;
			var next = 0;
			var bitsLeft = 0;
			var charValue = 0;

			foreach (var c in encoded)
			{
				charValue = CharToInt(c);
				if (charValue < 0)
				{
					throw new FormatException("Illegal character: '" + c + "'");
				}

				buffer <<= Shift;
				buffer |= charValue & Mask;
				bitsLeft += Shift;
				if (bitsLeft >= 8)
				{
					var newCharacter = buffer >> (bitsLeft - 8);
					buffer = buffer ^ (newCharacter << (bitsLeft - 8));

					result[next++] = (byte)newCharacter;
					bitsLeft -= 8;
				}
			}

			return result;
		}

		public static string ToBase32(byte[] data, bool padOutput = false)
		{
			ValidateArguments(data, data.Length);

			if (data.Length == 0)
			{
				return "";
			}

			var digits = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray();
			var outputLength = (data.Length * 8 + Shift - 1) / Shift;
			var conversion = new StringBuilder(outputLength);

			var offset = 0;
			var last = data.Length;
			int buffer = data[offset++];
			var bitsLeft = 8;
			while (bitsLeft > 0 || offset < last)
			{
				if (bitsLeft < Shift)
				{
					if (offset < last)
					{
						buffer <<= 8;
						buffer |= (data[offset++] & 0xff);
						bitsLeft += 8;
					}
					else
					{
						int pad = Shift - bitsLeft;
						buffer <<= pad;
						bitsLeft += pad;
					}
				}
				int index = Mask & (buffer >> (bitsLeft - Shift));
				bitsLeft -= Shift;
				conversion.Append(digits[index]);
			}

			if (padOutput)
			{
				int padding = 8 - (conversion.Length % 8);
				if (padding > 0)
				{
					conversion.Append('=', padding == 8 ? 0 : padding);
				}
			}

			return conversion.ToString();
		}

		#region Private Helpers

		static void ValidateArguments(byte[] data, int length)
		{
			if (data == null)
			{
				throw new ArgumentNullException(nameof(data));
			}

			if (length < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(length));
			}

			if (length > 0 && length >= (1 << 28))
			{
				throw new ArgumentOutOfRangeException(nameof(data));
			}
		}

		static int CharToInt(char c)
		{
			switch (c)
			{
				case 'A':
					return 0;
				case 'B':
					return 1;
				case 'C':
					return 2;
				case 'D':
					return 3;
				case 'E':
					return 4;
				case 'F':
					return 5;
				case 'G':
					return 6;
				case 'H':
					return 7;
				case 'I':
					return 8;
				case 'J':
					return 9;
				case 'K':
					return 10;
				case 'L':
					return 11;
				case 'M':
					return 12;
				case 'N':
					return 13;
				case 'O':
					return 14;
				case 'P':
					return 15;
				case 'Q':
					return 16;
				case 'R':
					return 17;
				case 'S':
					return 18;
				case 'T':
					return 19;
				case 'U':
					return 20;
				case 'V':
					return 21;
				case 'W':
					return 22;
				case 'X':
					return 23;
				case 'Y':
					return 24;
				case 'Z':
					return 25;
				case '2':
					return 26;
				case '3':
					return 27;
				case '4':
					return 28;
				case '5':
					return 29;
				case '6':
					return 30;
				case '7':
					return 31;
			}
			return -1;
		}

		#endregion
	}
}
