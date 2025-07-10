using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.CAReferenceData.Model;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CASIMAData
{
	public class WebSIMAParserV2 : IWebSIMAParserV2
	{
		readonly IWebDriverHelper client;
		readonly ISIMATextExtractor textExtractor;
		public WebSIMAParserV2(IWebDriverHelper client, ISIMATextExtractor textExtractor)
		{
			Argument.NotNull(textExtractor, nameof(textExtractor));
			Argument.NotNull(client, nameof(client));
			this.client = client;
			this.textExtractor = textExtractor;
			this.PublishedDate = DateTime.Today;
		}

		Dictionary<string, string> GetDetailLinks(HtmlNode documentNode)
		{
			var linkNodes = documentNode.SelectNodes("//tbody/tr/td[1]/a");
			return linkNodes.ToDictionary(x => x.InnerText, x => x.Attributes["href"].Value);
		}

		DateTime GetPublishedDate(HtmlNode documentNode)
		{
			var dateNodes = documentNode.SelectSingleNode("//time[@property='dateModified']");
			return DateTime.Parse(dateNodes.InnerText);
		}

		public IEnumerable<WebSIMAText> GetWebSIMAText(string simaUrl, string simaBaseUrl)
		{
			var htmlPageContainingLinks = client.GetWebPage(simaUrl);
			var htmlMenuDoc = new HtmlDocument();
			htmlMenuDoc.LoadHtml(htmlPageContainingLinks);
			PublishedDate = GetPublishedDate(htmlMenuDoc.DocumentNode);

			foreach (var keyValuePair in GetDetailLinks(htmlMenuDoc.DocumentNode))
			{
				var htmlDoc = new HtmlDocument();

				var result = new WebSIMAText();
				result.Description = new string(CharsToTitleCase(keyValuePair.Key).ToArray());
				var aLink = keyValuePair.Value.StartsWith(simaBaseUrl) ? keyValuePair.Value : simaBaseUrl + keyValuePair.Value;
				var htmlPage = client.GetWebPage(aLink);
				if (htmlPage == null && aLink != simaUrl)
				{
					htmlPage = htmlPageContainingLinks;
				}

				if (htmlPage != null)
				{
					htmlDoc.LoadHtml(htmlPage);

					var detailNode = htmlDoc.DocumentNode;
					var detailDate = GetPublishedDate(detailNode);
					if (detailDate > PublishedDate)
					{
						PublishedDate = detailDate;
					}

					var investigationNode = detailNode
						.SelectSingleNode("descendant::*[self::dt or self::h2][contains(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'investigation') or contains(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'investigations')]");
					var determinationNode = investigationNode?.SelectSingleNode("following-sibling::dd | parent::section") ?? null;
					result.DeterminationDate = determinationNode?.InnerText.ExtractDeterminationDate(textExtractor) ?? null;
					var referenceNode = determinationNode?.SelectSingleNode(@"descendant::a[
contains(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'),'preliminary determination') or
contains(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'),'preliminary determinations') or
contains(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'),'preliminary decisions') or
contains(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'final determination')]")
						?? determinationNode?.SelectSingleNode("descendant::a[contains(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 're-investigation')]") ?? null;
					result.CBSAReferenceNumber = referenceNode?.Attributes["href"]?.Value?.ExtractDumpingCase(textExtractor) ?? string.Empty;
					result.ClassificationNumbers = GetClassificationNumbers(detailNode);

					var duties = new List<WebSIMADuty>();
					var dutyLiabilityNode = detailNode.SelectNodes($"descendant::*[self::dt or self::h2][contains(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'duty liability')]") ?? null;

					if (dutyLiabilityNode != null)
					{
						foreach (var dutyHeader in dutyLiabilityNode)
						{
							var dutyNode = dutyHeader.SelectSingleNode("following-sibling::dd | parent::section");
							var dutyTypes = dutyHeader.InnerText.ExtractDutyTypes(textExtractor).ToArray();
							var dutyTables = dutyNode.SelectNodes("descendant::table[contains(translate(thead/tr/th/., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'country of origin or export')]");

							if (dutyTables == null)
							{
								if (dutyTables == null)
								{
									dutyTables = dutyNode.SelectNodes("descendant::table[contains(translate(thead/tr/th/., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'source of origin or export')]");
								}
							}
							if (dutyTables != null)
							{
								foreach (var dutyTable in dutyTables)
								{
									var dutyTextMultiParts = dutyTable.SelectNodes("descendant::tbody/tr").Select(x => x.InnerText).ToArray();
									var dutyTableHeader = dutyTable.SelectSingleNode("descendant::thead/tr").InnerText;
									foreach (var dutyTextMultiPart in dutyTextMultiParts)
									{
										var duty = new WebSIMADuty();
										duty.DutyType = dutyTypes.Length == 1 ? dutyTypes[0] : (dutyTable.InnerText.Contains("dumping") ? Constants.AntiDumping : Constants.Countervailing);
										duty.EffectiveDate = dutyNode.InnerText.ExtractEffectiveDate(textExtractor);
										var countryFromInnderTable = new[] { dutyTextMultiPart }.ExtractCountryOfOriginOrExport(textExtractor);
										if (countryFromInnderTable.Length > 0)
										{
											duty.CountryOfOriginOrExport = countryFromInnderTable;
										}
										var dutyFormula = (dutyTableHeader + dutyTextMultiPart).ExtractDutyRate(textExtractor);
										duty.DutyValue = dutyFormula.Item1;
										duty.DutyCurrency = dutyFormula.Item2;
										if (duty.DutyValue != "0")
										{
											duties.Add(duty);
										}
									}
								}
							}
							else
							{
								var paragraphs = dutyNode.ChildNodes;
								WebSIMADuty dumpingDuty = null;
								WebSIMADuty counterDuty = null;
								foreach (var paragraph in paragraphs)
								{
									var paragraphInnerText = paragraph.InnerText.ToLower().Replace('‑', '-');
									if (paragraphInnerText.ToLower().Contains("country of origin or export"))
									{
										if (dumpingDuty != null && dumpingDuty.DutyValue != "0")
										{
											duties.Add(dumpingDuty);
										}
										if (counterDuty != null && counterDuty.DutyValue != "0")
										{
											duties.Add(counterDuty);
										}
										dumpingDuty = new WebSIMADuty() { DutyValue = "0" };
										counterDuty = new WebSIMADuty() { DutyValue = "0" };
										var countryNames = paragraphInnerText.Replace("country of origin or export:", "").Trim()
											.Split(new[] { ",", " and ", " & " }, StringSplitOptions.RemoveEmptyEntries);
										var countryList = countryNames.ExtractCountryOfOriginOrExport(textExtractor);
										dumpingDuty.CountryOfOriginOrExport = countryList;
										counterDuty.CountryOfOriginOrExport = countryList;
									}
									var dutyValue = string.Empty;
									if (!paragraphInnerText.EndsWith("in the table below:") && (paragraphInnerText.Contains("anti-dumping duty is") || paragraphInnerText.Contains("countervailing duty is") ||
										paragraphInnerText.Contains("duty are equal to:") ||
										paragraphInnerText.Contains("information relating to the anti-dumping duty") || paragraphInnerText.Contains("information relating to the countervailing duty")))
									{
										if (dutyNode.SelectSingleNode("table") != null)
										{
											var exporterNode = dutyNode.SelectNodes("descendant::table/tbody/tr[contains(td, 'All other exporters')]");
											var drInnerTexts = exporterNode?.Select(x => x.InnerText).ToArray();
											if (drInnerTexts != null)
											{
												foreach (var one in drInnerTexts)
												{
													dutyValue += one;
												}
											}
										}
										var duty = paragraphInnerText.Contains("anti-dumping") ? dumpingDuty : counterDuty;
										duty.DutyType = paragraphInnerText.Contains("anti-dumping") ? Constants.AntiDumping : Constants.Countervailing;
										var dutyFormula = (dutyValue + paragraph.InnerText).ExtractDutyRate(textExtractor);
										duty.DutyValue = dutyFormula.Item1;
										duty.DutyCurrency = dutyFormula.Item2;
									}
									if (paragraphInnerText.Contains("or after") || paragraphInnerText.Contains("commencing"))
									{
										var effectiveDate = paragraphInnerText.ExtractEffectiveDate(textExtractor);
										dumpingDuty.EffectiveDate = effectiveDate;
										counterDuty.EffectiveDate = effectiveDate;
									}
								}
								if (dumpingDuty != null && dumpingDuty.DutyValue != "0")
								{
									duties.Add(dumpingDuty);
								}
								if (counterDuty != null && counterDuty.DutyValue != "0")
								{
									duties.Add(counterDuty);
								}
							}

							if (!duties.Any() || duties.Any(x => x.DutyValue == Constants.DefaultValues.Undefined) && dutyTables == null)
							{
								var countryUnderTitle = new string[] { };
								foreach (var dutyHeader2 in detailNode.SelectNodes($"descendant::*[self::dt or self::h2][contains(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'duty liability')]"))
								{
									dutyNode = dutyHeader2.SelectSingleNode("following-sibling::dd | parent::section");
									dutyTypes = dutyHeader2.InnerText.ExtractDutyTypes(textExtractor).ToArray();
									dutyTables = dutyNode.SelectNodes("descendant::table[contains(translate(thead/tr/th/., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'exporter')]");

									if (dutyTables != null)
									{
										foreach (var paragraph in dutyNode.ChildNodes)
										{
											var paragraphInnerText = paragraph.InnerText.ToLower().Replace('‑', '-');
											if (paragraphInnerText.Contains("country of origin or export:"))
											{
												var countryNames = paragraphInnerText.Replace("country of origin or export:", "").Trim()
																				.Split(new[] { ",", " and ", " & " }, StringSplitOptions.RemoveEmptyEntries);
												countryUnderTitle = countryNames.ExtractCountryOfOriginOrExport(textExtractor);
												break;
											}
										}

										foreach (var dutyTable in dutyTables)
										{
											if (dutyTable.SelectSingleNode("descendant::thead/tr").InnerText.Replace("\r", "\t").Replace("\t", "").Trim().Split('\n').Length <= 2)
											{
												var dutyTextMultiParts = dutyTable.SelectNodes("descendant::tbody/tr").Select(x => x.InnerText).ToArray();
												var dutyTableHeader = dutyTable.SelectSingleNode("descendant::thead/tr").InnerText;
												foreach (var dutyTextMultiPart in dutyTextMultiParts)
												{
													var duty = new WebSIMADuty();
													duty.DutyType = dutyTypes.Length == 1 ? dutyTypes[0] : (dutyTable.InnerText.Contains("dumping") ? Constants.AntiDumping : Constants.Countervailing);
													duty.EffectiveDate = dutyNode.InnerText.ExtractEffectiveDate(textExtractor);
													var countryFromInnderTable = new[] { dutyTextMultiPart }.ExtractCountryOfOriginOrExport(textExtractor);
													if (countryFromInnderTable.Length > 0)
													{
														duty.CountryOfOriginOrExport = countryFromInnderTable;
													}
													else
													{
														duty.CountryOfOriginOrExport = countryUnderTitle;
													}
													var dutyFormula = (dutyTableHeader + dutyTextMultiPart).ExtractDutyRate(textExtractor);
													duty.DutyValue = dutyFormula.Item1;
													duty.DutyCurrency = dutyFormula.Item2;
													if (duty.DutyValue != "0")
													{
														duties.Add(duty);
													}
												}
											}
										}
									}
								}
							}
						}
					}
					result.Duties = duties.ToArray();
					if (result.Duties.Length > 0)
					{
						yield return result;
					}
				}
			}
		}

		IEnumerable<char> CharsToTitleCase(string s)
		{
			bool newWord = true;
			foreach (char c in s)
			{
				if (newWord)
				{ yield return char.ToUpper(c); newWord = false; }
				else
					yield return char.ToLower(c);
				if (c == ' ')
					newWord = true;
			}
		}

		string[] GetClassificationNumbers(HtmlNode detailNode)
		{
			var classificationHeaderNode = detailNode.SelectSingleNode("descendant::*[self::dt or self::h2][contains(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'tariff classification number') or contains(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'harmonized system')]");
			var classificationUls = classificationHeaderNode?.SelectSingleNode("following-sibling::dd | parent::section");
			return classificationUls?.InnerText.ExtractClassificationNumbers(textExtractor) ?? new string[] { };
		}

		public DateTime PublishedDate { get; set; }
	}
}
