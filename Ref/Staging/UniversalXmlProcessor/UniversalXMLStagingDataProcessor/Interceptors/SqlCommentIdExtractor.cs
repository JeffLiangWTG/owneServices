using System;
using System.Text.RegularExpressions;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public static partial class SqlCommentIdExtractor
	{
		[GeneratedRegex($@"{SqlCommentAdder.PrefixCommentFlag} : (?<{FieldName}>[^\s--]+(?:\s+[^\s--]+)*) --", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline)]
		private static partial Regex IdRegex();

		public static string ExtractId(string commandText)
		{
			if (string.IsNullOrWhiteSpace(commandText))
			{
				throw new ArgumentNullException(nameof(commandText), "commandText is null or empty");
			}

			var match = IdRegex().Match(commandText);
			if (!match.Success)
			{
				throw new InvalidOperationException($"Unable to extract id for '{commandText}'");
			}

			return match.Groups[FieldName].Value.Trim();
		}

		const string FieldName = "id";
	}
}
