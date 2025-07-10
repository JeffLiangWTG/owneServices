using System;
using System.Globalization;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.ITReferenceData.Business
{
	public static class Extensions
	{
		public static DateTime? ToDateTime(this string dateTime)
		{
			return DateTime.TryParseExact(dateTime, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate)
				? parsedDate
				: (DateTime?)null;
		}

		public static bool HasCheckedAttribute(this HtmlNode htmlNode) => htmlNode?.Attributes?.Contains("checked") ?? false;

		public static bool HasYesValueAttribute(this HtmlNode htmlNode)
		{
			var attributeValue = htmlNode?.GetAttributeValue("value", string.Empty);
			return attributeValue == "SI" || attributeValue == "CONDIZIONE";
		}

		public static string ToItalianShortDateString(this DateTime dateTime) => dateTime.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

		public static DateTime GetDateOrFallbackToMinSmallDateTime(this DateTime? dateTime) => dateTime > minSmallDateTime ? dateTime.Value : minSmallDateTime;

		public static DateTime GetDateOrFallbackToMaxSmallDateTime(this DateTime? dateTime) => dateTime < maxSmallDateTime ? dateTime.Value : maxSmallDateTime;

		static readonly DateTime minSmallDateTime = new DateTime(1900, 01, 01);
		static readonly DateTime maxSmallDateTime = new DateTime(2079, 06, 06, 23, 59, 00);
	}
}
