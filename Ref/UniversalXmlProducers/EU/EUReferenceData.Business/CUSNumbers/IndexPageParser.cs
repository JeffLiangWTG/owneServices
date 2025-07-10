using System;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.CUSNumbers.Business
{
	public class IndexPageParser
	{
		public IndexPageParser(string pageContent)
		{
			this.pageContent = pageContent;
		}

		public DateTime ExtractPublicationTime()
		{
			var match = lastUpdateRegex.Match(pageContent);
			if (match.Success)
			{
				var dateString = match.Groups[1].Value;
				if (!string.IsNullOrEmpty(dateString))
				{
					if (DateTime.TryParseExact(dateString, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var lastUpdate))
					{
						return lastUpdate;
					}
				}
			}

			return DateTime.MinValue;
		}

		readonly string pageContent;

		static readonly Regex lastUpdateRegex = new Regex($"<span>\\s*{Constants.CUSNumbers.LASTUPDATESTRING}\\s*(\\d{{2}}-\\d{{2}}-\\d{{4}})\\s*<\\/span>", RegexOptions.Compiled | RegexOptions.Multiline);
	}
}
