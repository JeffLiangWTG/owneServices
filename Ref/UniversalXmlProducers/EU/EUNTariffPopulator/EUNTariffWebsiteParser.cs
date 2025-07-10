using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffPopulator
{
	public class EUNTariffWebsiteParser
	{
		public EUNTariffWebsiteParser(IWebDriverHelper client, string saveFilePath, DateTime publicationDate, DateTime startDate, DateTime? endDate = null)
		{
			Argument.NotNull(client, nameof(client));
			Argument.NotNullOrEmpty(saveFilePath, nameof(saveFilePath));
			writer = new XmlWriter(GetWriterConfiguration());
			writer.SetDataSource("EU Daily Tariff");
			writer.SetPublicationTime(publicationDate);
			writer.SetUpdateType(UpdateType.Partial);
			this.startDate = startDate;
			this.endDate = endDate ?? startDate;
			this.saveFilePath = saveFilePath;
			this.client = client;
		}

		public void ProduceXML()
		{
			List<Task<EUNWebsiteData>> tasksToExecute = new List<Task<EUNWebsiteData>>();
			foreach (var link in GetTariffDetailLinks())
			{
				tasksToExecute.Add(GetDetailPageData(link.TariffLink, link.TariffCode, link.TariffDescription));
				if (tasksToExecute.Count == 5)
				{
					tasksToExecute.ForEach(x => x.Wait());
					foreach (var item in tasksToExecute)
					{
						item.Result.ConvertToRefCusTariff(writer);
					}
					tasksToExecute.Clear();
				}
			}
			if (tasksToExecute.Count != 0)
			{
				tasksToExecute.ForEach(x => x.Wait());
				foreach (var item in tasksToExecute)
				{
					item.Result.ConvertToRefCusTariff(writer);
				}
			}
			writer.SaveXml(saveFilePath);
		}

		string CompleteSearchString(int pageOffset)
		{
			return Constants.baseAddress + Constants.SearchPageWithDates.Replace("$StartDate$", startDate.ToString("yyyyMMdd")).Replace("$EndDate$", endDate.ToString("yyyyMMdd")).Replace("$PageOffSet$", pageOffset.ToString());
		}

		IEnumerable<TariffScrape> GetTariffDetailLinks()
		{
			int pageOffset = 0;
			var nextPageLink = CompleteSearchString(pageOffset);
			while (nextPageLink != null)
			{
				var htmlPageContainingLinks = client.GetWebPage(nextPageLink);
				HtmlDocument doc = new HtmlDocument();
				doc.LoadHtml(htmlPageContainingLinks);
				var nodes = doc.DocumentNode.Descendants("span")
					.Where(s => s.HasAttributes && s.Attributes.Count(r => r.Value.StartsWith("blue_link iframe_expand_all_goods_code_taric_")) != 0)
					.Select(s => s.Attributes).ToList();
				foreach (var node in nodes)
				{
					var onclickValue = node.Where(s => s.Name == "onclick")?.First().Value;
					if (onclickValue != null)
					{
						var findTariffStart = onclickValue.IndexOf("taric_");
						var findTariffEnd = onclickValue.IndexOf("',");
						var tariffCode = onclickValue.Substring(findTariffStart, findTariffEnd - findTariffStart).Split('_')?[1];
						var tariffDescription = GetTariffDescription(doc, tariffCode);
						yield return new TariffScrape() { TariffCode = tariffCode, TariffDescription = tariffDescription, TariffLink = BuildUri("measures_details.jsp", 3, onclickValue) };
					}
				}
				if (HasNextPage(doc))
				{
					pageOffset += nodes.Count;
					nextPageLink = CompleteSearchString(pageOffset);
				}
				else
				{
					nextPageLink = null;
				}
			}
		}

		public string GetTariffDescription(HtmlDocument document, string tariffCode)
		{
			Argument.NotNull(document, nameof(document));
			Argument.NotNull(tariffCode, nameof(tariffCode));
			foreach (var item in document.DocumentNode.Descendants().Where(x => x.Id == tariffCode + "-80").ToList())
			{
				foreach (var table in item.Descendants("table"))
				{
					foreach (var td in table.Descendants("td"))
					{
						if (td.Attributes.Count(x => x.Value == "description_td") != 0)
						{
							foreach (var textElement in td.ChildNodes.Where(v => v.Name == "#text"))
							{
								return textElement.InnerText.Trim();
							}
						}
					}
				}
			}
			return "";
		}

		public string BuildUri(string startOfLink, int endOfLinkTrim, string onClickValue, bool addBaseAddress = true)
		{
			Argument.NotNull(startOfLink, nameof(startOfLink));
			Argument.NotNull(onClickValue, nameof(onClickValue));
			var subStringStart = onClickValue.IndexOf(startOfLink);
			var substringEnd = onClickValue.Length - subStringStart - endOfLinkTrim;
			return (addBaseAddress ? Constants.baseAddress : "") + onClickValue.Substring(subStringStart, substringEnd);
		}

		static bool HasNextPage(HtmlDocument doc)
		{
			Argument.NotNull(doc, nameof(doc));
			var nodes = doc.DocumentNode.Descendants("div").Where(s => s.Id == "navigation");
			return nodes.Any(node => node.InnerText.Contains("Next") || node.InnerText.Contains("more than"));
		}

		async Task<EUNWebsiteData> GetDetailPageData(string detailPageUri, string tariffCode, string tariffDescription)
		{
			Argument.NotNull(detailPageUri, nameof(detailPageUri));
			Argument.NotNull(tariffCode, nameof(tariffCode));
			var tariff = new EUNWebsiteData
			{
				TariffCode = tariffCode,
				TariffDescription = tariffDescription
			};

			HtmlDocument htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(client.GetWebPage(detailPageUri));

			var htmlBody = htmlDoc.DocumentNode.SelectSingleNode("//body");

			HtmlNodeCollection childNodes = htmlBody.ChildNodes;

			foreach (var node in childNodes)
			{
				if (node.NodeType == HtmlNodeType.Element)
				{
					RateData rate = null;
					var geographicalArea = "";
					foreach (var div in node.Elements("div").ToList())
					{
						if (rate == null)
							rate = new RateData();
						if (div.Id == "geographical_area")
						{
							geographicalArea = GetGeographicalArea(geographicalArea, div);
							rate.GeographicalArea = geographicalArea;
						}
						else if (!string.IsNullOrEmpty(geographicalArea) && string.IsNullOrEmpty(rate.GeographicalArea))
						{ rate.GeographicalArea = geographicalArea; }
						if (div.Id.StartsWith("measure_"))
						{
							foreach (var td in div.Descendants().Where(s => s.NodeType == HtmlNodeType.Element && s.Name == "td" && s.InnerText.Replace(Environment.NewLine, "").Trim() != "").ToList())
							{
								if (td.HasClass("td_measure_description"))
								{
									foreach (var rateField in td.Descendants("span").Where(s => s.HasClass("duty_rate")))
									{
										rate.RateFormula = rateField.InnerText;
									}

									var orderNumber = td.Descendants("td").ToList().Where(x => x.InnerText.Contains("Order number")).Select(x => FormatString(x.InnerText));
									if (orderNumber.Count() != 0)
									{
										var orderNumbers = Regex.Matches(orderNumber.First(), orderNumberRegex);
										if (orderNumbers.Count != 0)
										{
											rate.OrderNumber = Regex.Match(FormatString(orderNumbers[0].Value), numericValuesRegex).Value;
										}
									}

									var measureDateRange = FormatString(td.Descendants("span").First().InnerText);
									var dates = Regex.Matches(measureDateRange, dateFindRegex);
									if (dates.Count != 0)
									{
										rate.StartDate = DateTime.ParseExact(dates[0].Groups[0].Value, "dd-MM-yyyy", CultureInfo.InvariantCulture);
										if (dates.Count != 1)
										{
											rate.EndDate = DateTime.ParseExact(dates[1].Groups[0].Value, "dd-MM-yyyy", CultureInfo.InvariantCulture);
										}
										else
										{
											rate.EndDate = DateTime.ParseExact("06-06-2079 23:59", "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture);
										}
									}

									if (rate.TariffTypeDescription == null)
									{
										var measureDescription = FormatString(string.Join("", td.Descendants("td").First().ChildNodes.Where(s => s.Name == "#text").Select(s => s.InnerText)));
										rate.TariffTypeDescription += measureDescription;
										rate.EunMapping = EUNDescriptionMapping.GetEUNMapping(rate.TariffTypeDescription);
									}
								}
								if (rate.EunMapping != null)
								{
									foreach (var excludedCountries in td.Descendants("span").Where(s => s.HasAttributes && s.HasClass("excluded_countries")))
									{
										var result = Regex.Matches(FormatString(excludedCountries.InnerText), countryCodeFindRegex);
										if (result.Count != 0)
										{
											rate.ExcludedCountries.Add(result[result.Count - 1].Groups[1].Value);
										}
									}
									if (td.HasClass("td_measure_conditions"))
									{
										foreach (var span in td.Descendants("span").Where(s => s.InnerText == "[Show conditions]"))
										{
											var conditionLink = BuildUri("measures_conditions.jsp", 3, span.GetAttributeValue("onclick", null));
											await Task.Run(() => { rate.Conditions.AddRange(GetCondition(conditionLink)); });
										}
									}
								}
							}
						}
						if (rate.EunMapping != null)
						{
							if (rate.RateFormula == null && rate.Conditions.Where(v => v.Columns.Count > 0 && v.Columns[0].StartsWith("V")).Count() != 0)
							{
								rate.RateFormula = GetRateFormulaFromCondition(rate);
								rate.Conditions = rate.Conditions.Where(v => !v.Columns[0].StartsWith("V")).ToList();
							}
							if (rate != null && rate.GeographicalArea != null && rate.TariffTypeDescription != null && rate.RateFormula != null && rate.EunMapping != null)
							{
								tariff.Rates.Add(rate);
							}
						}
						rate = null;
					}
				}
			}
			return tariff;
		}

		public string GetGeographicalArea(string geographicalArea, HtmlNode div)
		{
			Argument.NotNull(geographicalArea, nameof(geographicalArea));
			Argument.NotNull(div, nameof(div));
			var result = Regex.Matches(FormatString(div.InnerText), countryCodeFindRegex);
			if (result.Count != 0)
				geographicalArea = GeographicalAreaLookup.GetGeographicalCode(result[result.Count - 1].Groups[1].Value);
			return geographicalArea;
		}

		string GetRateFormulaFromCondition(RateData rate)
		{
			Argument.NotNull(rate, nameof(rate));
			Argument.NotNull(rate.Conditions, nameof(rate.Conditions));
			var result = "Cond:  ";
			foreach (var item in rate.Conditions)
			{
				if (result != "Cond:  ")
					result += ";";
				result += item.Columns[0].Substring(0, 1) + " ";
				result += item.Certificate;
			}
			return result;
		}

		List<RateCondition> GetCondition(string conditionsUri)
		{
			Argument.NotNull(conditionsUri, nameof(conditionsUri));
			string[] validConditionTypes = { "B", "V" };
			try
			{
				HtmlDocument htmlDoc = new HtmlDocument();
				htmlDoc.LoadHtml(client.GetWebPage(conditionsUri));
				var rateConditions = new List<RateCondition>();
				foreach (var table in htmlDoc.DocumentNode.Descendants("table").Where(s => s.HasClass("conditions")))
				{
					foreach (var innerTable in table.Descendants("table").Where(s => s.HasClass("condition_table")))
					{
						foreach (var tr in innerTable.Descendants("tr"))
						{
							var rateCondition = new RateCondition();
							foreach (var column in tr.Descendants("td").Where(s => s.HasClass("td_title") && s.HasChildNodes))
							{
								foreach (var txt in column.ChildNodes.Where(s => s.Name == "#text"))
								{
									if (FormatString(txt.InnerText) != string.Empty)
										rateCondition.Columns.Add(FormatString(txt.InnerText));
								}
								foreach (var certificate in column.Descendants("span").Where(s => s.HasClass("condition_bold")))
								{
									if (!string.IsNullOrEmpty(rateCondition.Certificate))
										rateCondition.Certificate += ":";
									rateCondition.Certificate += FormatString(certificate.InnerText).Replace(" ", string.Empty);
								}
							}
							if (rateCondition.Certificate != null && validConditionTypes.Any(x => x.StartsWith(rateCondition.Columns[0].Substring(0, 1))))
								rateConditions.Add(rateCondition);
						}
					}
				}
				return rateConditions;
			}
			catch (Exception) { throw; }
		}

		XmlWriterConfiguration GetWriterConfiguration()
		{
			var tariffConfiguration = new EntityTypeConfiguration<RefCusTariff>(true);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_TariffCode, true);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, "EUN");
			tariffConfiguration.IncludeColumn(x => x.ZZ1_ZZI_NKTariffType, true);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, "EUN");
			tariffConfiguration.IncludeColumn(x => x.RefCusRates, false);
			tariffConfiguration.IncludeColumn(x => x.RefCusConditions, false);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_StartDate, false);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_EndDate, false);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_Description, false);

			var rateConfiguration = new EntityTypeConfiguration<RefCusRate>(true);
			rateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZZZ_NKDataGrouping, true, "EUN");
			rateConfiguration.IncludeColumn(x => x.ZZ2_StartDate, false);
			rateConfiguration.IncludeColumn(x => x.ZZ2_EndDate, false);
			rateConfiguration.IncludeColumn(x => x.ZZ2_RateFormula, false);
			rateConfiguration.IncludeColumn(x => x.ZZ2_ZZS_NKPreference, true);
			rateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZZS_ZZZ_NKDataGrouping, true, "EUN");
			rateConfiguration.IncludeColumn(x => x.ZZ2_ZY1_NKRateCode, true);
			rateConfiguration.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_NKRateType, true, "DUT");
			rateConfiguration.IncludeColumn(x => x.RefCusApplicabilities, true);

			var applicabilityConfiguration = new EntityTypeConfiguration<RefCusApplicability>(true);
			applicabilityConfiguration.IncludeColumn(x => x.ZZT_OrderNumber, false);
			applicabilityConfiguration.IncludeColumn(x => x.ZZT_StartDate, false);
			applicabilityConfiguration.IncludeColumn(x => x.ZZT_EndDate, false);
			applicabilityConfiguration.IncludeColumn(x => x.ZZT_ZZA_NKTradeGroup, true);
			applicabilityConfiguration.IncludeColumnWithConstantValue(x => x.ZZT_ZZA_ZZZ_NKDataGrouping, true, "EUN");
			applicabilityConfiguration.IncludeColumn(x => x.RefCusExcludedTradeGroups, true);

			var excludedTradeGroupConfiguration = new EntityTypeConfiguration<RefCusExcludedTradeGroup>(true);
			excludedTradeGroupConfiguration.IncludeColumn(x => x.ZZC_ZZA_NKTradeGroup, true);
			excludedTradeGroupConfiguration.IncludeColumnWithConstantValue(x => x.ZZC_ZZA_ZZZ_NKDataGrouping, true, "EUN");

			var conditionConfiguration = new EntityTypeConfiguration<RefCusCondition>(true);
			conditionConfiguration.IncludeColumnWithConstantValue(x => x.ZX1_ZZZ_NKDataGrouping, true, "EUN");
			conditionConfiguration.IncludeColumn(x => x.ZX1_ZX2_NKConditionType, true);
			conditionConfiguration.IncludeColumn(x => x.ZX1_StartDate, false);
			conditionConfiguration.IncludeColumn(x => x.ZX1_EndDate, false);
			conditionConfiguration.IncludeColumnWithDefaultValue(x => x.ZX1_Source, false, "EU Taric");
			conditionConfiguration.IncludeColumn(x => x.ZX1_Comment, false);
			conditionConfiguration.IncludeColumn(x => x.ZX1_IsImport, false);
			conditionConfiguration.IncludeColumn(x => x.ZX1_IsExport, false);
			conditionConfiguration.IncludeColumn(x => x.ZX1_ConditionValueTrueMeansStop, false);
			conditionConfiguration.IncludeColumn(x => x.RefCusApplicabilities, true);
			conditionConfiguration.IncludeColumn(x => x.RefCusConditionValues, true);

			var conditionValueConfigiration = new EntityTypeConfiguration<RefCusConditionValue>(true);
			conditionValueConfigiration.IncludeColumnWithConstantValue(x => x.ZX3_LogicalORWithinGroup, false, 0);
			conditionValueConfigiration.IncludeColumn(x => x.ZX3_Value, true);
			conditionValueConfigiration.IncludeColumn(x => x.ZX3_ZX4_NKValueType, true);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(tariffConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(conditionConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(applicabilityConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(conditionValueConfigiration);
			writerConfiguration.IncludeEntityTypeConfiguration(rateConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(applicabilityConfiguration);
			writerConfiguration.IncludeEntityTypeConfiguration(excludedTradeGroupConfiguration);

			return writerConfiguration;
		}

		static string FormatString(string value)
		{
			Argument.NotNull(value, nameof(value));
			return Regex.Replace(WebUtility.HtmlDecode(value), @"\t|\n|\r", "").Trim();
		}

		readonly IWebDriverHelper client;
		readonly DateTime startDate;
		readonly DateTime endDate;
		readonly XmlWriter writer;
		readonly string saveFilePath;
		private static string countryCodeFindRegex = @"\((.*?)\)";
		private static string dateFindRegex = @"[0-9]{2}-[0-9]{2}-[0-9]{4}";
		private static string orderNumberRegex = @"\((Order number.*?\d+)\)";
		private static string numericValuesRegex = @"\d+";
	}

	class TariffScrape
	{
		public string TariffCode;
		public string TariffDescription;
		public string TariffLink;
	}
}
