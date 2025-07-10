using System.Text;

public static class StringExtensions
{
	public static string ToAlphaNumericOnly(this string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return string.Empty;
		}

		var result = new StringBuilder(input.Length);
		foreach (var c in input)
		{
			if (char.IsLetterOrDigit(c))
			{
				result.Append(c);
			}
		}

		return result.ToString();
	}
}
