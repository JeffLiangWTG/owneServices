using System.Text.RegularExpressions;

namespace CargoWise.RefDbRepo.NZReferenceData.Business
{
	internal static partial class StringHelper
	{
		[GeneratedRegex("<[^>]+>")]
		private static partial Regex HtmlTagRegex();

		public static string RemoveHtmlTags(this string input)
		{
			return HtmlTagRegex().Replace(input, string.Empty);
		}

		[GeneratedRegex(@"&(?!lt;|gt;|amp;)\w+;")]
		private static partial Regex HtmlEntityRegex();

		public static string RemoveHtmlEntitiesExcludingIllegalBodyXmlChars(this string input)
		{
			return HtmlEntityRegex().Replace(input, string.Empty);
		}

		public static string EscapeBodyXmlCharacters(this string input)
		{
			return input.Replace("&", "&amp;")
				.Replace("<", "&lt;")
				.Replace(">", "&gt;");
		}
		public static string Truncate(this string input, int maxLength, out bool isTruncated)
		{
			isTruncated = false;

			if (input != null && input.Length > maxLength)
			{
				isTruncated = true;
				return input[..maxLength];
			}
			return input;
		}
	}
}
