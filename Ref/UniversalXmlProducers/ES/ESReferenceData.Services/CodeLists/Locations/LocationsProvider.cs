using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.ESReferenceData.Services
{
	public class LocationsProvider : RequestCodeProvider
	{
		protected override Encoding Encoding => Encoding.GetEncoding("ISO-8859-1");

		public IEnumerable<ILocationsItem> GetLocationItems(string locationsURL, string officeCodesURL, DateTime date)
		{
			var officeCodesItems = GetOfficeCodesFromCsvFile(officeCodesURL, date);
			var webRequestWrapper = GetWebRequestWrapper();

			var locationItems = new List<ILocationsItem>();
			foreach (var officeCodeItem in officeCodesItems)
			{
				var htmlOfficeLocationPageCollection = new List<string>();
				var officeCode = officeCodeItem.Code;

				var htmlContent = GetOfficeLocationHtmlPageContent(locationsURL, webRequestWrapper, officeCode);
				htmlOfficeLocationPageCollection.Add(htmlContent);

				var postArgs = GetPostArgsFromHtml(htmlContent, LocationProviderEmptyKeys, $" Office Code: {officeCode}");

				var pages = CalulateNumberOfPages(postArgs);

				postArgs["VEZ"] = "AVANZAR";
				for (int i = ResultsPerPage; i < pages * ResultsPerPage; i += ResultsPerPage)
				{
					if (i > ResultsPerPage)
					{
						PreparePostArgsForNextPage(postArgs, i);
					}

					htmlOfficeLocationPageCollection.Add(webRequestWrapper.GetContentFromPost(locationsURL, GetFormattedPostData(postArgs)));
				}

				foreach (string htmlString in htmlOfficeLocationPageCollection)
				{
					locationItems.AddRange(GetTableElements("GESTOR", htmlString));
				}
			}

			return locationItems;
		}

		protected override Exception GetNewExceptionToThrow(string exceptionMessage) => new LocationsProviderException(exceptionMessage);

		#region Implementation

		protected List<LocationOfficeCodesItem> GetOfficeCodesFromCsvFile(string url, DateTime date)
		{
			var webRequestWrapper = GetWebRequestWrapper();

			var formattedDate = date.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
			var htmlContent = webRequestWrapper.GetContent($"{url}?IDETAB=TADUANAS&VEZ=BUSCAR&FECCON={formattedDate}");

			var postArgs = GetPostArgsFromHtml(htmlContent, OfficeCodeEmptyKeys);

			postArgs["VEZ"] = "EXPOREXCEL";
			postArgs.Add("FECCON", formattedDate);

			return GeLocationOfficeCodesFromCsv(webRequestWrapper.GetContentFromPost(url, GetFormattedPostData(postArgs), false));
		}

		static IEnumerable<ILocationsItem> GetTableElements(string id, string html)
		{
			var locationItems = new List<ILocationsItem>();
			var doc = new HtmlDocument();
			doc.LoadHtml(html);
			var nodes = doc.DocumentNode.SelectNodes($"//table[@id='{id}']//tr");
			if (nodes != null)
			{
				foreach (var row in nodes)
				{
					var tdNodes = row.SelectNodes("td");
					if (tdNodes != null && tdNodes.Count == ColumnsInLocationsTable)
					{
						var location = ReplaceAllSpaces(tdNodes[0].InnerText);
						var name = ReplaceMultipleSpaces(tdNodes[1].InnerText);
						var startDate = ReplaceAllSpaces(tdNodes[2].InnerText);
						var endDate = ReplaceAllSpaces(tdNodes[3].InnerText);

						var locationData = new LocationsItem(location, name, startDate, endDate);
						locationItems.Add(locationData);
					}
				}
			}

			return locationItems;

			string ReplaceMultipleSpaces(string str) => Regex.Replace(str, @"\s+", " ");
			string ReplaceAllSpaces(string str) => str.Replace(" ", "");
		}

		static List<LocationOfficeCodesItem> GeLocationOfficeCodesFromCsv(string inputCsvString)
		{
			inputCsvString = inputCsvString.Replace("\"", "'").Replace(Constants.AEAT.FirstLineForCSV, string.Empty);
			var itemList = new List<LocationOfficeCodesItem>();
			using (var reader = new StringReader(inputCsvString))
			using (var csvReader = new CsvReader(reader))
			{
				csvReader.Configuration.Delimiter = ";";
				csvReader.Configuration.MissingFieldFound = null;
				csvReader.Configuration.RegisterClassMap<LocationOfficeCodesItemMap>();
				while (csvReader.Read())
				{
					var record = csvReader.GetRecord<LocationOfficeCodesItem>();
					if (record != null)
					{
						var code = record.Code;
						if (!string.IsNullOrEmpty(code) && !code.EndsWith("00", StringComparison.InvariantCulture))
						{
							itemList.Add(record);
						}
					}
				}
			}
			return itemList;
		}

		static string GetOfficeLocationHtmlPageContent(string locationsURL, IWebRequestWrapper webRequestWrapper, string officeCode) => webRequestWrapper.GetContent($"{locationsURL}?VEZ=BUSCAR&UBI_IDE_RECINTO=00{officeCode}");

		#endregion

		readonly ImmutableArray<string> LocationProviderEmptyKeys = new string[] { "ESTADO_COLS", "VEZ" }.ToImmutableArray();
		readonly ImmutableArray<string> OfficeCodeEmptyKeys = new string[] { "AYUCMP", "PAGINA", "VEZ", "COLUMNA_ORDEN", "MODO_ORDEN", "DEF_MASIVA" }.ToImmutableArray();

		const int ColumnsInLocationsTable = 9;
	}
}
