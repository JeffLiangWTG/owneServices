using System.Text.RegularExpressions;

namespace Extensions;
public static class StringExtensions
{
	/// <summary>
	/// Converts a string to a lower hyphen-delimited string.
	/// </summary>
	/// <param name="input">The input string to be converted.</param>
	/// <returns>A hyphen-delimited string in lowercase.</returns>
	public static string ToLowerHyphen(this string input)
	{
		// Use a regular expression to insert a hyphen before each uppercase letter and then convert to lowercase
		string result = Regex.Replace(input, "([a-z])([A-Z])", "$1-$2").ToLower();

		return result;
	}
}
