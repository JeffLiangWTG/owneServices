using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using HtmlAgilityPack;
using static CargoWise.RefDbRepo.AUReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class NomenclatureParser
	{
		public NomenclatureParser()
		{
			HttpClientHelper = new HttpClientHelper();
		}

		public void Parse(DateTime publishTime)
		{
			var dataManager = new ProcessingDataManager<NomenclatureProcessingData>("NomenclatureProcessingData.json");
			var processingData = dataManager.ProcessingData;

			var maxAttempts = int.Parse(ApplicationConfig.AUNomenclatureWebsiteMaxAttempts, NumberFormatInfo.CurrentInfo);
			try
			{
				var lastRunDateTime = DateTime.MinValue;
				if (ApplicationConfig.AUNomenclatureForceDownloaded ||
					!(DateTime.TryParse(processingData.AUNomenclatureParserLastRun, out lastRunDateTime) &&
					  lastRunDateTime.Date == DateTime.Today))
				{
					var lastUpdatedDateTime = GetWebsiteLastUpdatedDateTime(ApplicationConfig.NomenclatureBasePath);
					if (lastUpdatedDateTime > lastRunDateTime)
					{
						var nomenclatureWriter = new XmlWriter(NomenclatureXMLWriterConfigurationBuilder.Build());
						nomenclatureWriter.SetDataSource("AU Customs Nomenclature");
						nomenclatureWriter.SetUpdateType(UpdateType.Full);

						ParseSections();
						nomenclatureWriter.SetPublicationTime(publishTime);

						foreach (var group in nomenclatureGroups)
						{
							TransformBeforeWriting(group);
							nomenclatureWriter.PopulateData(group);
						}

						nomenclatureWriter.SaveXml(nomenclatureOutputPath);
						PrintSummery();

						OutputTariffsToFile();

						processingData.AUNomenclatureParserLastRun = DateTime.Now.ToString("s");
						processingData.AUNomenclatureRetryCount = 0;
						dataManager.SaveData();
					}
				}
			}
			catch (Exception ex)
			{
				processingData.AUNomenclatureRetryCount++;
				dataManager.SaveData();

				var emailGroup = ApplicationConfig.EmailGroup;
				if (!string.IsNullOrEmpty(emailGroup) && processingData.AUNomenclatureRetryCount == maxAttempts)
				{
					var emailBody = $"The AU Nomenclature Parser process has failed after multiple retries.\n\n" +
									$"Base URL: {ApplicationConfig.NomenclatureBasePath}\n\n" +
									$"Recent Process Exception Message: {ex.Message}\n\n" +
									"Please verify that the process is functioning correctly and that the necessary data is available.";
					SendEmail(emailGroup, "AU Nomenclature Parser Process Failed", emailBody);
				}
				throw;
			}
		}

		void OutputTariffsToFile()
		{
			tariffs.ForEach(NomenclatureParser.TransformBeforeWriting);
			var existingTariffData = new Dictionary<string, TariffOutput>();
			if (File.Exists(tariffsOutputPath))
			{
				var existingJson = File.ReadAllText(tariffsOutputPath);
				var existingList = JsonSerializer.Deserialize<List<TariffOutput>>(existingJson, jsonOptions);
				existingTariffData = existingList.ToDictionary(t => t.Tariff, t => t);
			}
			else
			{
				var directoryPath = Path.GetDirectoryName(tariffsOutputPath);
				if (directoryPath != null)
				{
					Directory.CreateDirectory(directoryPath);
				}
			}

			foreach (var newTariff in tariffs)
			{
				if (existingTariffData.TryGetValue(newTariff.ZZ1_TariffCode, out var existingTariff) && !existingTariff.AllowOverride)
				{
					continue;
				}
				if (existingTariff == null)
				{
					existingTariff = new TariffOutput
					{
						Tariff = newTariff.ZZ1_TariffCode,
						CompositeKey = newTariff.ZZ1_CompositeKeyOnZZ5,
						AllowOverride = true
					};
					existingTariffData[newTariff.ZZ1_TariffCode] = existingTariff;
				}
				existingTariff.TariffDescription = newTariff.ZZ1_Description;
			}

			var tariffJson = JsonSerializer.Serialize(existingTariffData.Values.ToList(), jsonOptions);
			File.WriteAllText(tariffsOutputPath, tariffJson);
		}

		internal void ParseSections()
		{
			Console.WriteLine($"Retrieving section list from {baseUri}");
			var htmlSectionList = new HtmlDocument();
			htmlSectionList.LoadHtml(RetrieveUri(baseUri));
			var titleRow = htmlSectionList.DocumentNode.SelectSingleNode("//tr[th[contains(text(), 'Section title')]][th[contains(text(), 'Chapter')]]");

			QuitIf(titleRow == null, "Could not find the title row with captions of 'Section title' and 'Chapter'");

			HtmlNode sectionRow = titleRow;
			int count = 0;
			while ((sectionRow = sectionRow.NextSibling) != null)
			{
				if (sectionRow.Name != "tr")
				{
					continue;
				}
				var th = sectionRow.Element("th");
				var linkSectionLeft = th?.Element("a");
				var linkSectionRight = sectionRow.Elements("td").LastOrDefault()?.Element("a");
				var sectionUri = new Uri(baseUri, linkSectionLeft?.Attributes["href"]?.Value);
				var sectionRomanNumber = linkSectionLeft?.InnerText.Trim();
				var sectionTitle = linkSectionRight?.InnerText.Trim();
				Console.WriteLine($"Parsing section row {sectionRomanNumber}");
				QuitIf(linkSectionLeft?.Attributes["href"].Value != linkSectionRight?.Attributes["href"].Value
					|| string.IsNullOrWhiteSpace(sectionRomanNumber)
					|| string.IsNullOrWhiteSpace(sectionTitle)
					|| !sectionUri.AbsoluteUri.EndsWith(sectionRomanNumber, true, CultureInfo.InvariantCulture),
					$"Could not parse section {sectionRomanNumber}");
				var sectionNumber = string.Format(null, "{0:d2}", Utils.ConvertRomanNumeralToInt(sectionRomanNumber));
				var newGroup = new RefCusNomenclatureGroup()
				{
					ZZ5_Value = sectionNumber,
					ZZ5_Description = Normalize(sectionTitle),
					ZZ5_CompositeKey = sectionNumber,
				};
				nomenclatureGroups.Add(newGroup);
				sectionCount++;
				Console.WriteLine($"Added section {sectionNumber} {sectionTitle}");
				ParseChapters(sectionUri, newGroup);
				count++;
			}
			Console.WriteLine($"Finished retrieving section list of {count} from {baseUri}");
		}

		void ParseChapters(Uri uri, RefCusNomenclatureGroup parent)
		{
			Console.WriteLine($"Retrieving chapter list from {uri} for section {parent.ZZ5_Description}");
			var htmlChapterList = new HtmlDocument();
			htmlChapterList.LoadHtml(RetrieveUri(uri));

			var chapterLines = htmlChapterList.DocumentNode.SelectNodes("//main//dt").Where(dt => dt.Element("a")?.InnerText.StartsWith("Chapter ", StringComparison.InvariantCulture) ?? false).ToList();
			QuitIf(chapterLines.Count == 0, $"Found no chapters from {uri}");
			foreach (var dt in chapterLines)
			{
				var dd = dt.SelectSingleNode("./following-sibling::dd[1]");
				var linkChapter = dt.Element("a");
				var linkTitle = dd?.Element("a");
				_ = int.TryParse(linkChapter.InnerText.Trim().Replace("Chapter ", ""), out var chapterNumber);
				var chapterUri = new Uri(uri, linkChapter?.Attributes["href"]?.Value);
				QuitIf(dd?.Name != "dd" || linkChapter?.Attributes["href"].Value != linkTitle?.Attributes["href"].Value || !(chapterNumber > 0) || string.IsNullOrWhiteSpace(linkTitle.InnerText) || !chapterUri.AbsoluteUri.EndsWith(chapterNumber.ToString(NumberFormatInfo.CurrentInfo), StringComparison.InvariantCulture),
					$"Could not parse chapter {linkTitle}");
				var chapterNumberStr = string.Format(null, "{0:d2}", chapterNumber);
				var newGroup = new RefCusNomenclatureGroup()
				{
					ZZ5_Value = chapterNumberStr,
					ZZ5_Description = HttpUtility.HtmlDecode(linkTitle.InnerText),
					ZZ5_CompositeKey = string.Join(".", parent.ZZ5_CompositeKey, chapterNumberStr),
				};
				nomenclatureGroups.Add(newGroup);
				chapterCount++;
				Console.WriteLine($"Added chapter {chapterNumber} {linkTitle.InnerText}");
				ParseGroups(chapterUri, newGroup);
			}
			Console.WriteLine($"Finished retrieving chapter list of {chapterLines.Count} from {uri} for section {parent.ZZ5_Description}");
		}

		void ParseGroups(Uri uri, RefCusNomenclatureGroup parent)
		{
			Console.WriteLine($"Retrieving group list from {uri} for chapter {parent.ZZ5_Description}");
			var htmlGroupList = new HtmlDocument();
			htmlGroupList.LoadHtml(RetrieveUri(uri));

			var headings = htmlGroupList.DocumentNode.SelectNodes("//main//dt").Where(dt => Normalize(dt.GetDirectInnerText()).StartsWith(parent.ZZ5_Value, StringComparison.InvariantCulture) && !Normalize(dt.NextSibling.Element("a")?.InnerText).Equals(EmptyHeading, StringComparison.OrdinalIgnoreCase)).ToList();
			QuitIf(headings.Count == 0, $"Found no headings from {uri}");
			int groupCount = 0;
			foreach (var dt in headings)
			{
				var heading = Normalize(dt.GetDirectInnerText());
				Console.WriteLine($"Retrieving group list for heading {heading}");

				HtmlNodeCollection groupRows = null;
				RefCusNomenclatureGroup newHeadingGroup = null;
				try
				{
					newHeadingGroup = new RefCusNomenclatureGroup()
					{
						ZZ5_Value = heading,
						ZZ5_Description = Normalize((dt.NextSibling.Element("a") ?? dt.ParentNode.SelectSingleNode($"//a[@href='#{heading}']"))?.InnerText),
						ZZ5_CompositeKey = string.Join(".", parent.ZZ5_CompositeKey, heading.Substring(2)),
					};
					QuitIf(!Regex.IsMatch(newHeadingGroup.ZZ5_Value, "^[0-9]{4}$") || !Regex.IsMatch(newHeadingGroup.ZZ5_CompositeKey, "^[0-9][0-9\\.]*[0-9]$") || string.IsNullOrEmpty(newHeadingGroup.ZZ5_Description), $"Invalid format value parsed {newHeadingGroup.ZZ5_Value} {newHeadingGroup.ZZ5_Description}, key {newHeadingGroup.ZZ5_CompositeKey}");
					nomenclatureGroups.Add(newHeadingGroup);
					headingCount++;
					Console.WriteLine($"Added group {newHeadingGroup.ZZ5_Value} {newHeadingGroup.ZZ5_Description}");

					groupRows = htmlGroupList.DocumentNode.SelectNodes($"//tbody[./tr/th[1][normalize-space()='{heading}']]//tr") ??
						(htmlGroupList.DocumentNode.SelectNodes($"//div[@id='{heading}']/table[1]/tbody/tr[th[@scope='row']]") ??
						(htmlGroupList.DocumentNode.SelectNodes($"//div[@id='{heading}']/table[1]/tbody[tr[td[1][normalize-space()='{heading}.00.00']]]/tr") ??
						(htmlGroupList.DocumentNode.SelectNodes($"//div[@id='{heading}']//tbody//tr[th[@scope='row']]") ??
						(htmlGroupList.DocumentNode.SelectNodes($"//div[not(@id)]//tbody//tr[@id='{heading}'][th[@scope='row']]") ??
						(htmlGroupList.DocumentNode.SelectNodes($"//div[@id='{heading}.00.00']//tbody//tr[th[@scope='row']]") ??
						htmlGroupList.DocumentNode.SelectNodes($"//tbody[.//th[normalize-space()='{heading}.00.00']][not(ancestor::div[1]/@id)]//tr"))))));
					// current heading might be read after its own groups hence skip the searching of groups
					if ((groupRows?.Count ?? 0) == 0)
					{
						QuitIf(groupCount == 0, $"Found no groups for heading {heading}");
						groupCount = 0;
						continue;
					}
				}
				catch (EntryParsingException ex)
				{
					skippedGroups.Add($"Heading {heading} skipped for reason: {ex.Message}. Content: {string.Join(" ", dt.ParentNode.InnerText.Substring(0, Math.Min(20, dt.ParentNode.InnerText.Length)))}");
					continue;
				}
				finally
				{
					groupCount = 0;
				}

				ParseHeading(headings, dt, heading, groupRows, groupCount, newHeadingGroup);

				Console.WriteLine($"Added heading {heading} with {groupCount} groups");

				// reset the count if there was not heading change. Otherwise leave it so that the count is known when reading the heading after its groups
				if (heading == dt.InnerText)
				{
					groupCount = 0;
				}
			}
			Console.WriteLine($"Finished retrieving group list of {headings.Count} from {uri} for chapter {parent.ZZ5_Description}");
		}

		void ParseHeading(List<HtmlNode> headings, HtmlNode dt, string heading, HtmlNodeCollection groupRows, int groupCount, RefCusNomenclatureGroup newHeadingGroup)
		{
			var (code, compositeKey) = (newHeadingGroup.ZZ5_Value, newHeadingGroup.ZZ5_CompositeKey);
			foreach (var groupRow in groupRows.Where(r => !IsKnownIllFormed(r)))
			{
				try
				{
					// The html node structure is not consistent for all groups, e.g. 9033.00.00
					var shouldSkipExtraColumn = groupRow.Element("th") == null && groupRow.Elements("td").Count() > 5;
					var extraSkipCount = shouldSkipExtraColumn ? 1 : 0;
					var referenceNumberElement = shouldSkipExtraColumn ? groupRow.Elements("td").FirstOrDefault() : groupRow.Element("th");

					var referenceNumber = Normalize(referenceNumberElement?.InnerText);
					var statisticalCode = Normalize(groupRow.Elements("td").Skip(extraSkipCount).FirstOrDefault()?.InnerText);
					var unit = groupRow.Elements("td").Skip(1 + extraSkipCount).FirstOrDefault()?.InnerText.Trim().ToUpperInvariant();
					var goods = Normalize(groupRow.Elements("td").Skip(2 + extraSkipCount).FirstOrDefault()?.InnerText.Trim());

					// groups that differ only by statistical code may not have reference number displayed, such as 0203.29.00 30.
					// has to use the previous parsed reference number
					if (referenceNumber == heading)
					{
						continue;
					}

					// Unfortunately there are exceptions a group is not listed under its own heading, e.g. 0504.00.00
					if (!string.IsNullOrEmpty(referenceNumber) && !referenceNumber.StartsWith(heading, StringComparison.InvariantCulture))
					{
						Console.WriteLine($"Added heading {heading} with {groupCount} groups");
						groupCount = 0;
						var currentHeadingIdx = headings.IndexOf(dt);
						if (headings.Count > currentHeadingIdx + 1)
						{
							var newHeading = headings[headings.IndexOf(dt) + 1].InnerText;
							QuitIf(!referenceNumber.StartsWith(newHeading, StringComparison.InvariantCulture), $"Invalid reference number {referenceNumber}");
							heading = newHeading;
						}
					}

					QuitIf(!(string.IsNullOrEmpty(referenceNumber) || referenceNumber.StartsWith(heading, StringComparison.InvariantCulture)) ||
						!(string.IsNullOrEmpty(statisticalCode) || Regex.IsMatch(statisticalCode, "[0-9]{2}")) ||
						string.IsNullOrEmpty(goods),
						$"Could not parse group {referenceNumber} {statisticalCode} {goods}");
					if (string.IsNullOrEmpty(referenceNumber) && string.IsNullOrEmpty(statisticalCode))
					{
						Console.WriteLine($"Warning: Empty line under {code}");
						continue;
					}

					if (string.IsNullOrEmpty(referenceNumber))
					{
						referenceNumber = code.Split(' ')[0];
					}

					var newKey = GetNewKey(compositeKey, referenceNumber, statisticalCode);
					QuitIf(heading == dt.InnerText && !newKey.StartsWith(newHeadingGroup.ZZ5_CompositeKey, StringComparison.InvariantCulture), $"Parse error resulting in key {newKey} following {compositeKey}");

					var groupCode = string.IsNullOrEmpty(statisticalCode) ? referenceNumber : string.Join(" ", referenceNumber, statisticalCode);
					var groupDescription = HttpUtility.HtmlDecode(goods);
					var groupCompositeKey = newKey;

					if (string.IsNullOrEmpty(statisticalCode))
					{
						var newNomenclatureGroup = new RefCusNomenclatureGroup()
						{
							ZZ5_Value = groupCode,
							ZZ5_Description = groupDescription,
							ZZ5_CompositeKey = groupCompositeKey,
						};
						nomenclatureGroups.Add(newNomenclatureGroup);
					}
					else
					{
						var newTariff = new RefCusTariff
						{
							ZZ1_TariffCode = groupCode,
							ZZ1_Description = groupDescription,
							ZZ1_CompositeKeyOnZZ5 = groupCompositeKey,
							RefCusTariffUOMs = new RefCusTariffUOM[]
							{
								new RefCusTariffUOM
								{
									ZZ8_UOM = TransformUnit(unit, groupCode)
								}
							}
						};
						tariffs.Add(newTariff);
					}

					groupCount++;
					QuitIf(!Regex.IsMatch(groupCode, @"^[0-9]{4}[0-9\.\s]*[0-9]$") || !Regex.IsMatch(groupCompositeKey, @"^[0-9][0-9\.]*[0-9]$"), $"Invalid format value parsed {groupCode}, key {groupCompositeKey}");
					Console.WriteLine($"Added group {groupCode} {groupDescription}");
					code = groupCode;
					compositeKey = groupCompositeKey;
				}
				catch (EntryParsingException ex)
				{
					skippedGroups.Add($"Group below {code} skipped for reason: {ex.Message}. Heading: {heading}; Content: {string.Join(" ", groupRow.InnerText.Substring(0, Math.Min(20, groupRow.InnerText.Length)))}");
					continue;
				}
			}
		}

		static string GetNewKey(string compositeKey, string referenceNumber, string statisticalCode)
		{
			int idxOnCurrentValue = 0;
			int countMatchedParts = 0;
			string newKey = string.Empty;
			foreach (var part in compositeKey.Split('.').Skip(1))
			{
				if (referenceNumber.Substring(idxOnCurrentValue).StartsWith(part, StringComparison.InvariantCulture))
				{
					idxOnCurrentValue += part.Length;
					if (idxOnCurrentValue < referenceNumber.Length && referenceNumber[idxOnCurrentValue] == '.')
					{
						idxOnCurrentValue++;
					}
					countMatchedParts++;
				}
				else
				{
					break;
				}
			}

			newKey = string.Join(".", compositeKey.Split('.').Take(countMatchedParts + 1).Concat(idxOnCurrentValue < referenceNumber.Length ? new[] { referenceNumber.Substring(idxOnCurrentValue) } : Array.Empty<string>()));
			if (!string.IsNullOrEmpty(statisticalCode))
			{
				newKey = string.Join(".", newKey, statisticalCode);
			}
			return newKey;
		}

		static void QuitIf(bool condition, string error)
		{
			if (condition)
			{
				Console.WriteLine(error);
				throw new EntryParsingException(error);
			}
		}

		string Normalize(string s)
		{
			s = s ?? string.Empty;
			s = HttpUtility.HtmlDecode(s);
			foreach (var c in SpecialCharacters)
			{
				s = s.Replace(c, ' ');
				while (true)
				{
					int len = s.Length;
					s = s.Replace("  ", " ");
					if (len == s.Length)
					{
						break;
					}
				}
			}
			return s.Trim();
		}

		static string TransformUnit(string unit, string groupCode)
		{
			unit = unit.Replace("&#160;", "");
			unit = unit.Replace("​..", ".."); // Different dot for 8704.90.90 90
			if (unit.Equals("..", StringComparison.Ordinal) || string.IsNullOrWhiteSpace(unit))
			{
				return TariffUOMTypes.NR;
			}

			var unitLength = unit.IndexOf("\r\n", StringComparison.CurrentCulture); // The unit field may has multiple lines, e.g. 2009.29.00
			var result = unit.Split("\r\n").FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)).Trim(); //8461.50.50 23 Only have one line but it contains '\r\n'

			var stringBuilder = new StringBuilder();
			foreach (var c in result)
			{
				if (char.IsLetter(c) || c == ' ')
				{
					stringBuilder.Append(c);
				}
			}
			result = stringBuilder.ToString();

			if (!string.IsNullOrEmpty(result))
			{
				switch (result)
				{
					case "L AL":
						result = "LA";
						break;
					case "KGTSS":   // The unit field of '2009.11.00 39' has multiple lines but is retrieved as one line
						result = "KG";
						break;
					default:
						result = result.Trim();
						break;
				}
				if (unitLength > 0)
				{
					var errorMessage = $"Warning: unit value for group {groupCode} has multiple lines and is updated to {result}.";
					Console.WriteLine(errorMessage);
				}
			}
			else
			{
				var errorMessage = $"Warning: unit value '{unit}' for group {groupCode} is invalid.";
				Console.WriteLine(errorMessage);
				return TariffUOMTypes.ERR;
			}

			return result;
		}

		static bool IsKnownIllFormed(HtmlNode node)
		{
			switch (node.Name)
			{
				case "tr":
					// invisible row
					return string.IsNullOrEmpty(node.InnerHtml);
				default:
					return false;
			}
		}

		internal static void TransformBeforeWriting(RefDataRepoModelEntityType entity)
		{
			if (entity is RefCusNomenclatureGroup group)
			{
				group.ZZ5_Value = group.ZZ5_Value.Replace(".", "").Replace(" ", "");
				group.ZZ5_Description = Regex.Replace(group.ZZ5_Description, "^\\s*[-‑.]*\\s*", "");
				if (group.ZZ5_CompositeKey.Length > 5)
				{
					group.ZZ5_CompositeKey = group.ZZ5_CompositeKey.Insert(5, ".");
				}
			}
			else if (entity is RefCusTariff tariff)
			{
				tariff.ZZ1_TariffCode = tariff.ZZ1_TariffCode.Replace(".", "").Replace(" ", "");
				tariff.ZZ1_Description = Regex.Replace(tariff.ZZ1_Description, "^\\s*[-‑.]*\\s*", "");
				tariff.ZZ1_Description = Regex.Replace(tariff.ZZ1_Description, "\\u00A0", " "); // no-break space
				tariff.ZZ1_CompositeKeyOnZZ5 = tariff.ZZ1_CompositeKeyOnZZ5.Insert(5, ".");
			}
		}

		string RetrieveUri(Uri uri)
		{
			return RetrieveUri(uri, retryTimes);
		}

		string RetrieveUri(Uri uri, int retryTimes)
		{
			try
			{
				return HttpClientHelper.GetWebPageAsync(uri.AbsoluteUri).Result;
			}
			catch (AggregateException ex)
			{
				if (ex.InnerExceptions.Any(e => e is HttpRequestException) && retryTimes > 0)
				{
					Thread.Sleep(retryWaitSeconds * 1000);
					return RetrieveUri(uri, retryTimes - 1);
				}
				else
				{
					var errorMessage = $"Cannot retrieve {uri}. Program will end. Please check the exceptions for the reason.";
					Console.Error.WriteLine(errorMessage);
					throw;
				}
			}
		}

		public virtual void PrintSummery()
		{
			Console.WriteLine("**********************************");
			Console.WriteLine("Summary:");
			Console.WriteLine($"--- {sectionCount} sections read");
			Console.WriteLine($"--- {chapterCount} chapters read");
			Console.WriteLine($"--- {headingCount} headings read");
			Console.WriteLine($"--- {nomenclatureGroups.Count + tariffs.Count - sectionCount - chapterCount - headingCount} groups read");
			Console.WriteLine($"--- {skippedGroups.Count} skipped");
			Console.WriteLine("**********************************");
			foreach (var line in skippedGroups)
			{
				Console.WriteLine(line);
			}
		}

		public virtual DateTime GetWebsiteLastUpdatedDateTime(string baseUri)
		{
			var websiteContent = HttpClientHelper.GetWebPageAsync(baseUri).Result;

			var webSiteDoc = new HtmlDocument();
			webSiteDoc.LoadHtml(websiteContent);

			string lastUpdatedText = webSiteDoc.DocumentNode
				.SelectSingleNode("//span[@id='pageModified']")?.InnerText.Trim() ?? string.Empty;
			if (string.IsNullOrEmpty(lastUpdatedText))
			{
				throw new InvalidOperationException($"Website accessed, but 'Last updated' text not found. URL: {baseUri}");
			}

			if (!DateTime.TryParse(lastUpdatedText, CultureInfo.CreateSpecificCulture("en-AU"), DateTimeStyles.None,
				    out DateTime lastUpdatedDateTime))
			{
				throw new FormatException(
					$"Failed to parse 'Last updated' text as DateTime. Value: '{lastUpdatedText}', URL: {baseUri}");
			}

			return lastUpdatedDateTime;
		}

		public virtual void SendEmail(string emailGroup, string subject, string body)
		{
			EmailService.SendEmail(emailGroup, subject, body);
		}

		public IHttpClientHelper HttpClientHelper { get; set; }

		readonly List<RefCusNomenclatureGroup> nomenclatureGroups = new List<RefCusNomenclatureGroup>();

		internal readonly List<RefCusTariff> tariffs = new List<RefCusTariff>();

		readonly List<string> skippedGroups = new List<string>();

		int sectionCount;

		int chapterCount;

		int headingCount;

		readonly Uri baseUri = new Uri(ApplicationConfig.NomenclatureBasePath);

		readonly string nomenclatureOutputPath = Path.Combine(ApplicationConfig.OutputDirectory, "AU Customs Nomenclature.xml");

		readonly string tariffsOutputPath = ApplicationConfig.AUTariffsOutputPath;

		readonly int retryTimes = int.Parse(ApplicationConfig.NomenclatureRetryTimes, NumberFormatInfo.CurrentInfo);

		readonly int retryWaitSeconds = int.Parse(ApplicationConfig.NomenclatureRetryWaitSeconds, NumberFormatInfo.CurrentInfo);

		const string EmptyHeading = "No heading";

		readonly char[] SpecialCharacters = new[] { (char)0x200B, '*', '#', '\r', '\n' };

		readonly JsonSerializerOptions jsonOptions =  new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
			PropertyNameCaseInsensitive = true,
			WriteIndented = true,
			Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		};
}
}
