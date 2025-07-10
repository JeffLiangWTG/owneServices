using System;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.INReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.INReferenceData.Business
{
	public class EDILocationXMLProducer
	{
		const string DataSource = "IN EDI Location";
		const string LocationTableId = "pagetable";

		public EDILocationXMLProducer(IHttpClientHelper httpClientHelper)
		{
			this.httpClientHelper = httpClientHelper;
		}

		readonly IHttpClientHelper httpClientHelper;

		public string GenerateEDILocationXMLFile(string locationsUrl, string outputFilePath)
		{
			var locations = GetLocationData(locationsUrl);

			var errorBuilder = new StringBuilder();
			var dataParser = new EDILocationParser(errorBuilder);
			var refCusCodeLists = dataParser.ParseLocationData(locations);

			var xmlWriterConfig = XMLWriterHelper.GetEDILocationXMLWriterConfiguration();

			XMLWriterHelper.ExportToXMLFile(
				xmlWriterConfig,
				refCusCodeLists,
				DataSource,
				DateTime.Now,
				UpdateType.Full,
				outputFilePath);

			return errorBuilder.ToString();
		}

		HtmlNode GetLocationData(string locationsUrl)
		{
			var locationsSourcePage = LoadWebPage(locationsUrl);

			var locationTable = locationsSourcePage.DocumentNode.SelectNodes("//table")?.FirstOrDefault(x => x.Id == LocationTableId)
								?? throw new UnhandledApplicationException($"Unable to fetch table with EDILocation data from source {locationsUrl}");

			return locationTable;
		}

		HtmlDocument LoadWebPage(string url)
		{
			string page = httpClientHelper.GetWebPageAsync(url).GetAwaiter().GetResult();

			if (string.IsNullOrEmpty(page))
			{
				throw new UnhandledApplicationException($"EDILocation Source Url ({url}) is incorrect");
			}

			var htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(page);

			return htmlDocument;
		}
	}
}
