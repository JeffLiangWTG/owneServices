using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Resources;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.HtmlDataExtractor
{
	public static class HtmlDataExtractor
	{
		public static DateTime GetDataUpdatedOnDate(string rawHtml)
		{
			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(rawHtml);

			var dateNode = htmlDoc.DocumentNode?.SelectSingleNode("//font[starts-with(.,'" + MeasuresConstant.DateUpdateOn + "')]");

			if (dateNode == null || string.IsNullOrEmpty(dateNode.InnerText) ||
				!dateNode.InnerText.Contains(MeasuresConstant.DateUpdateOn) ||
				dateNode.InnerText.IndexOf(MeasuresConstant.DateUpdateOn, StringComparison.InvariantCultureIgnoreCase) < 0)
			{
				return MeasuresConstant.DefaultStartDate;
			}

			var dateText = dateNode.InnerText.Replace(MeasuresConstant.DateUpdateOn, "").Trim();
			return DateTime.ParseExact(dateText, "dd/MM/yyyy", CultureInfo.InvariantCulture);
		}

		public static string GetTariffDescription(string rawHtml)
		{
			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(rawHtml);

			const string xPath = "//html[1]//body[1]//form[3]//table[1]//tbody[1]//tr[2]//td[3]";
			var tariffDescriptionNode = htmlDoc.DocumentNode?.SelectSingleNode(xPath);
			return ScrappedRecord.CleanHtml(tariffDescriptionNode?.InnerText);
		}

		public static string GetNationalInformationSection(string rawHtml)
		{
			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(rawHtml);

			var nationalInformationSection = htmlDoc.DocumentNode?.SelectSingleNode("//td[text()='Nazionali']/ancestor::table[1]");
			return nationalInformationSection?.OuterHtml;
		}

		public static List<string> GetCertificateParameterString(string rawHtml)
		{
			Argument.NotNull(rawHtml, nameof(rawHtml));
			var result = new List<string>();
			var certificateRegex = @"(<A href=""javascript:linkToPostKeySicuroBill[0-9]?\('MisureServlet',)(.*)(\)"">)";
			var certificateMatches = Regex.Matches(rawHtml, certificateRegex);
			foreach (Match match in certificateMatches)
			{
				if (match != null && match.Groups.Count == 4 && Regex.Match(match.Groups[2].Value, @"[0-9]{2}/[0-9]{2}/[0-9]{4}").Success)
				{
					result.Add(match.Groups[2].Value);
				}
			}
			return result;
		}

		public static CertificateData GetCertificateData(string rawHtml)
		{
			Argument.NotNull(rawHtml, nameof(rawHtml));
			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(rawHtml);

			var certificateNumberRegex = @"((Codice|Certificato):[ ]*)([A-Z0-9]+)";
			var checkBoxValueRegex = @"(onclick=""this.checked=)(true|false)";

			var certificateNumber = htmlDoc.DocumentNode?.SelectSingleNode("//html[1]//body[1]//form[3]//table[1]//tbody[1]//tr[1]//td[2]")?.InnerText;
			if (string.IsNullOrEmpty(certificateNumber))
			{
				certificateNumber = htmlDoc.DocumentNode?.SelectSingleNode("//html[1]//body[1]//form[3]//table[1]//tbody[1]//tr[1]//td[1]")?.InnerText;
			}

			var certIdCheckBox = htmlDoc.DocumentNode?.SelectSingleNode("//html[1]//body[1]//form[3]//table[1]//tbody[1]//tr[8]//td[2]//input[6]")?.OuterHtml;
			if (string.IsNullOrEmpty(certIdCheckBox))
			{
				certIdCheckBox = htmlDoc.DocumentNode?.SelectSingleNode("//html[1]//body[1]//form[3]//table[1]//tbody[1]//tr[9]//td[2]//input[5]")?.OuterHtml;
			}

			if (!string.IsNullOrEmpty(certIdCheckBox) && !string.IsNullOrEmpty(certificateNumber))
			{
				var certificateNumberValue = Regex.Match(ScrappedRecord.CleanHtml(certificateNumber) ?? string.Empty, certificateNumberRegex)?.Groups[3]?.Value;
				var certIdCheckBoxValue = Regex.Match(ScrappedRecord.CleanHtml(certIdCheckBox) ?? string.Empty, checkBoxValueRegex)?.Groups[2]?.Value;

				if (!string.IsNullOrEmpty(certificateNumberValue)
					&& !string.IsNullOrEmpty(certIdCheckBoxValue))
				{
					return new CertificateData(certificateNumberValue, certIdCheckBoxValue);
				}
			}
			return null;
		}

		public static IEnumerable<ScrappedRecord> ScrapRecordsFromHtml(string rawHtml)
		{
			var htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(rawHtml);

			var tableRowCollection = htmlDoc.DocumentNode?.SelectNodes("//td[@class='TDOUTPUTSX' and position()=1]/ancestor::tr[1]");

			if (tableRowCollection == null)
			{
				return null;
			}

			var scrappedRecords = from row in tableRowCollection
								  select row.ChildNodes.ToArray().Where(x => x.Name == "td").ToArray()
							into nodes
								  let measure = nodes[0].InnerHtml
								  let requirement = nodes[1].InnerHtml
								  select new ScrappedRecord(measure, requirement);

			return scrappedRecords;
		}
	}
}
