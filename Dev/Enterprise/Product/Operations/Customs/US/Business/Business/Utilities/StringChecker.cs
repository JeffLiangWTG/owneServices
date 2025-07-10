using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public static class StringChecker
	{
		public static bool IsLettersAndNumbersAndSpaces(ZString s)
		{
			return Regex.IsMatch(s, "^[A-Z0-9 ]*$");
		}

		public static ZString ReplaceSpecialCharactersWithSpaces(ZString s)
		{
			return Regex.Replace(s, "[^A-Z0-9\\s]", " ");
		}

		public static bool IsValidCharactersForClassX(ZString s)
		{
			foreach (char c in s)
			{
				if (!IsValidCharactersForClassX(c))
				{
					return false;
				}
			}
			return true;
		}

		static bool IsValidCharactersForClassX(char c)
		{
			return (c <= 'Z' && c >= 'A') || char.IsDigit(c) || char.IsWhiteSpace(c) || SpecialCharactersForClassX.IndexOf(c) >= 0;
		}

		public static ZString ReplaceAsValidForClassX(ZString s)
		{
			var result = s;
			foreach (char c in result)
			{
				if (!IsValidCharactersForClassX(c))
				{
					result = result.Replace(c, ' ');
				}
			}
			return result;
		}

		public const string ShouldBeLettersAndNumbersSpaceOnly = "Field contains invalid characters, should be letters, numbers or space.";
		public const string SpecialCharactersForClassX = @"!@#$%^&*()-_=+[{]}\|;:‘“,<.>/?`~¢";
	}
}
