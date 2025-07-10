using System.Security.Cryptography;
using System.Text;

namespace CargoWise.RefDbRepo.Common.Infrastructure.Test
{
	public static class SecureRandomStringGenerator
	{
		const string UpperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		const string LowerChars = "abcdefghijklmnopqrstuvwxyz";
		const string Numbers = "0123456789";
		const string Symbols = "!@#$%^&*()-_=+[]{};:,.<>?";
		const int Length = 16;

		static SecureRandomStringGenerator()
		{
			RandomString = GenerateRandomString();
		}

		static string GenerateRandomString()
		{
			var randomNumber = new byte[Length];
			using (var rng = RandomNumberGenerator.Create())
			{
				rng.GetBytes(randomNumber);
			}

			var randomStringBuilder = new StringBuilder(Length);
			randomStringBuilder.Append(UpperChars[randomNumber[0] % UpperChars.Length]);
			randomStringBuilder.Append(LowerChars[randomNumber[1] % LowerChars.Length]);
			randomStringBuilder.Append(Numbers[randomNumber[2] % Numbers.Length]);
			randomStringBuilder.Append(Symbols[randomNumber[3] % Symbols.Length]);

			const string allChars = UpperChars + LowerChars + Numbers + Symbols;
			for (int i = 4; i < Length; i++)
			{
				randomStringBuilder.Append(allChars[randomNumber[i] % allChars.Length]);
			}

			return randomStringBuilder.ToString();
		}

		public static string RandomString { get; }
	}
}
