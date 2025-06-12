using System.Text.RegularExpressions;

namespace CargoWise.eHub.Products.GBCustoms.Core.BT.Tests.TestHelpers
{
	internal static class AssertExtensions
	{
		public static string ReplaceGuidColumn(this string query, string columnName, string placeholderValue)
		{
			return Regex.Replace(
				query,
				string.Format(@"<{0}>[a-f0-9\-]{{36}}</{0}>", columnName),
				string.Format(@"<{0}>{1}</{0}>", columnName, placeholderValue));
		}

		public static string ReplaceDateTimeColumn(this string query, string columnName, string placeholderValue)
		{
			return Regex.Replace(
				query,
				// NOTE: This is DateTimeOffset.ToString("O") format!
				string.Format(@"<{0}>[\d-]{{10}}T[\d:]{{8}}Z</{0}>", columnName),
				string.Format(@"<{0}>{1}</{0}>", columnName, placeholderValue));
		}

		public static string ReplaceHexColumn(this string query, string columnName, string placeholderValue)
		{
			return Regex.Replace(
				query,
				string.Format(@"<{0}>0x\d{{16}}</{0}>", columnName),
				string.Format(@"<{0}>{1}</{0}>", columnName, placeholderValue));
		}
	}
}
