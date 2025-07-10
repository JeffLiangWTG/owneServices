using System;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.AUReferenceData.Services
{
	public static class DownloadHelper
	{
		public static (Uri fileUri, DateTime publishedDate) DiscoverDataFile(IHttpClientHelper clientHelper, string fileName, string indexUriString)
		{
			string indexPage;
			try
			{
				indexPage = clientHelper.GetWebPageAsync(indexUriString).Result;
			}
			catch (Exception)
			{
				Console.Error.WriteLine($"Cannot load {indexUriString}");
				throw;
			}
			var match = Regex.Match(indexPage, $"{fileName}-(?<publishedDate>[0-9]{{10}}).txt");
			if (match.Success)
			{
				return (new Uri(new Uri(indexUriString), match.Value), DateTime.ParseExact(match.Groups["publishedDate"].Value, "yyMMddhhmm", CultureInfo.InvariantCulture));
			}
			else
			{
				throw new UnhandledApplicationException($"Cannot find file {fileName} from {indexUriString}");
			}
		}
	}
}
