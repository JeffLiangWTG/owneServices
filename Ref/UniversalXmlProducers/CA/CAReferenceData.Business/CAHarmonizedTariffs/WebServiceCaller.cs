using System;
using System.Collections;
using System.Text;
using System.Xml;
using CargoWise.RefDbRepo.CAReferenceData.Services;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs
{
	public class WebServiceCaller : IWebServiceCaller
	{
		public WebServiceCaller(StringBuilder stringBuilder)
		{
			_stringBuilder = stringBuilder;
		}
		readonly StringBuilder _stringBuilder;

		public DateTime? GetLatestUpdateOnDate(string queryType)
		{
			DateTime? date = null;
			var param = new Hashtable
			{
				{ "$top", 1 },
				{ "$select", "UpdateOn" },
				{ "$orderby", "UpdateOn desc" }
			};
			var scraper = new WebSvcScraper(ApplicationConfig.CustomsTariffAPI, queryType, param, Constants.DefaultValues.ENLanguage);
			var xmlDoc = scraper.QueryAndReadXmlResponse()?.GetAwaiter().GetResult();
			if (scraper.HasErrorNotification)
			{
				_stringBuilder.Append(scraper.GetErrorNotification());
			}
			else
			{
				XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
				nsmgr.AddNamespace(string.Empty, "http://www.w3.org/2005/Atom");
				nsmgr.AddNamespace("m", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
				nsmgr.AddNamespace("d", "http://schemas.microsoft.com/ado/2007/08/dataservices");
				var updateOnString = xmlDoc.SelectSingleNode("//d:UpdateOn", nsmgr)?.InnerText ?? string.Empty;
				if (DateTime.TryParse(updateOnString, out var parseDate))
				{
					date = parseDate;
				}
				else
				{
					_stringBuilder.Append($"Failed to get correct UpdateOn from web service, UpdateOn : {updateOnString}");
				}
			}
			return date;
		}

		public bool QueryAndDownloadXmlFiles(string workingFolder, string queryType, string[] orderByColumns, string[] selectColumns, string acceptLanguage = "EN", string filter = "")
		{
			int extractedRecordCount = 0;
			var anyReocrd = false;
			var skip = 0;
			do
			{
				var param = new Hashtable();
				param.Add("$top", 250);
				param.Add("$orderby", string.Join(",", orderByColumns));
				param.Add("$select", string.Join(",", selectColumns));
				param.Add("$skip", skip);
				if (!string.IsNullOrEmpty(filter))
				{
					param.Add("$filter", filter);
				}
				var scraper = new WebSvcScraper(ApplicationConfig.CustomsTariffAPI, queryType, param, acceptLanguage);
				var fileName = $"{queryType}_{acceptLanguage}_{skip}.xml";
				extractedRecordCount = scraper.QueryAndSaveXmlResponseFile(workingFolder, fileName)?.GetAwaiter().GetResult() ?? 0;
				anyReocrd = anyReocrd || extractedRecordCount > 0;
				var downloadFlag = extractedRecordCount == 0 ? "skipped" : "downloaded";
				Console.WriteLine($"File:{fileName} {downloadFlag}.");
				if (scraper.HasErrorNotification)
				{
					_stringBuilder.Append(scraper.GetErrorNotification());
				}
				skip += 250;
			} while (extractedRecordCount == 250);
			return anyReocrd;
		}
	}
}
