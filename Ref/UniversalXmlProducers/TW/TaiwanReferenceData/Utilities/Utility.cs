using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using HtmlAgilityPack;
using NPOI.HWPF;
using NPOI.HWPF.Extractor;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public static class Utility
	{
		static string tempDirectory;

		public static string TempDirectory
		{
			get
			{
				if (string.IsNullOrEmpty(tempDirectory))
				{
					tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
					Directory.CreateDirectory(tempDirectory);
				}
				return tempDirectory;
			}
		}

		public static DateTime TrimSeconds(this DateTime dt)
		{
			return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0, dt.Kind);
		}

		public static string SafeSubstring(this string value, int startIndex, int length)
		{
			return new string((value ?? string.Empty).Skip(startIndex).Take(length).ToArray());
		}

		public static bool TryConvertTaiwanDateStringToDateTime(string s, out DateTime result)
		{
			var culture = new CultureInfo(Constants.Cultures.Taiwan);
			culture.DateTimeFormat.Calendar = new TaiwanCalendar();
			return DateTime.TryParse(s, culture, DateTimeStyles.None, out result);
		}

		public static XElement LastNotEmpty(this IEnumerable<XElement> elements)
		{
			return elements.Where(m => !string.IsNullOrEmpty(m.Value)).LastOrDefault();
		}

		public static void CompileHsSectionsAndChaptersFromWord(MemoryStream file, Dictionary<int, int> hsChapterAndSection)
		{
			var document = new HWPFDocument(file);
			var extractor = new WordExtractor(document);
			int sectionNumber = 0;
			foreach (var paragraph in extractor.ParagraphText)
			{
				if (!string.IsNullOrEmpty(paragraph))
				{
					if (Regex.IsMatch(paragraph, @"(?<=Sec[\.|tion])(.|\n)*?(?=(Sec[\.|tion]|$))"))
					{
						sectionNumber += 1;
					}
					else if (Regex.IsMatch(paragraph, @"Ch\. ?\d"))
					{
						hsChapterAndSection.Add(Convert.ToInt32(Regex.Match(paragraph, @"(?<=Ch\.\s?)\d{1,2}").Value), sectionNumber);
					}
				}
			}
		}

		public static List<string> GetEnvironmentalProtectionTariffsFromExcel(string url, IWebClient webClient)
		{
			var result = new List<string>();

			var array = webClient.DownloadData(url);
			using (var file = new MemoryStream(array))
			{
				var document = OdsHelper.GetDocument(file);
				var nmsManager = OdsHelper.InitializeXmlNamespaceManager(document);
				foreach (XmlNode table in OdsHelper.GetTableNodes(document, nmsManager))
				{
					if (OdsHelper.GetTableName(table) == "鎖檔稅則")
					{
						var rows = OdsHelper.GetRowNodes(table, nmsManager);
						for (int i = 1; i < rows.Count; i++)
						{
							var cells = OdsHelper.GetCellNodes(rows[i], nmsManager);
							if (cells.Count == 0)
							{
								continue;
							}

							var data = OdsHelper.GetDataValue(cells[0]);
							if (Regex.IsMatch(data, @"\d{11}"))
							{
								result.Add(data);
							}
						}
					}
				}
			}

			return result;
		}

		public static List<T> GetDataFromFile<T>(string url, IWebClient webClient, Func<string, T> dataRow)
		{
			var list = new List<T>();

			using (webClient)
			using (var txtFile = new MemoryStream(webClient.DownloadData(url)))
			using (var reader = new StreamReader(txtFile, Encoding.UTF8))
			{
				while (!reader.EndOfStream)
				{
					list.Add(dataRow(reader.ReadLine()));
				}
			}

			return list;
		}

		public static string GetSubheadings(string hsSubheadings)
		{
			string result = string.Empty;

			if (!string.IsNullOrEmpty(hsSubheadings))
			{
				if (hsSubheadings == "00")
				{
					result = ".10";
				}
				else if (hsSubheadings.SafeSubstring(1, 1) == "0")
				{
					result = string.Concat(".", hsSubheadings.SafeSubstring(0, 1));
				}
				else
				{
					result = string.Concat(".", hsSubheadings.SafeSubstring(0, 1), !string.IsNullOrEmpty(hsSubheadings.SafeSubstring(1, 1)) ? string.Concat(".", hsSubheadings.SafeSubstring(1, 1)) : "");
				}
			}

			return result;
		}

		public static int GetValueAsInt(string str)
		{
			TryParseValueAsInt(str, out int result);
			return result;
		}

		public static bool TryParseValueAsInt(string str, out int result)
		{
			return int.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
		}

		public static decimal GetValueAsDecimal(string str)
		{
			decimal.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal result);
			return result;
		}

		public static string GetResponseString(string url)
		{
			using (var webClient = new WebClientWrapper())
			using (var ms = new MemoryStream(webClient.DownloadData(new Uri(url))))
			using (var reader = new StreamReader(ms))
			{
				return reader.ReadToEnd();
			}
		}

		public static string GetPackingHouseListFileUrlFromHtml(string html)
		{
			var baseUrl = AppConfig.CodeLists.TaiwanPackingHouse.Url.BaseUrl;
			var regex = new Regex(AppConfig.CodeLists.TaiwanPackingHouse.Regex.DownloadFileNameRegex, RegexOptions.IgnoreCase);
			Func<Match, string> getrelativeUrl = (match) => match.Groups[1].Value;
			var errorMessage = $"Packing house list file url can not be found from {html}.";

			return GetUrlStringFromHtml(baseUrl, html, regex, getrelativeUrl, errorMessage);
		}

		public static string GetUnitsOfMeasurementFileUrlFromHtml(string html)
		{
			var regex = new Regex(AppConfig.CodeLists.TaiwanUnitsOfMeasurement.Regex.DownloadFileNameRegex, RegexOptions.IgnoreCase);
			Func<Match, string> getrelativeUrl = (match) => match.Groups[1].Value;
			var errorMessage = $"Units of measurement file url can not be found from {html}.";

			return GetUrlStringFromHtml(AppConfig.CodeLists.Shared.BaseUrl, html, regex, getrelativeUrl, errorMessage);
		}

		public static string GetAttachmentFileUrlFromHtml(string html, string fileType = "file-odt")
		{
			var htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(html);
			var fileLinkNode = htmlDocument.DocumentNode.SelectSingleNode($"//a[@class='{fileType}' and not(@style='display: none')]");
			var relativeUrl = fileLinkNode?.Attributes["href"]?.Value ?? string.Empty;
			if (string.IsNullOrEmpty(relativeUrl))
			{
				throw new Exception($"file url can not be found from {html}.");
			}

			return $"{AppConfig.CodeLists.Shared.BaseUrl}{relativeUrl}";
		}

		static string DateRegexString => "\\d{3,4}(-|/)\\d{2}(-|/)\\d{2}";

		static string DateTimeRegexString => "\\d{3,4}(-|/)\\d{2}(-|/)\\d{2} \\d{2}:\\d{2}";

		public static DateTime GetPublicationDateTimeUseRegex(string html)
		{
			var publicationDateRegex = new Regex($"(發布日期|更新日期)：{DateRegexString}");
			var matches = publicationDateRegex.Matches(html).Cast<Match>();
			return matches.Any() ? GetPublicationDateTimeFromDateString(matches.First().Value.Split('：')[1]) : SystemContext.Now();
		}

		static DateTime GetPublicationDateTimeFromDateString(string dateString)
		{
			var formats = new string[] { "yyyy-MM-dd HH:mm", "yyyy-MM-dd", "yyyy/MM/dd HH:mm", "yyyy/MM/dd" };
			if (DateTime.TryParseExact(dateString, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
			{
				return dateTime;
			}
			else
			{
				var taiwanCulture = new CultureInfo("zh-TW");
				taiwanCulture.DateTimeFormat.Calendar = new TaiwanCalendar();
				if(DateTime.TryParse(dateString, taiwanCulture, DateTimeStyles.None, out var taiWanDateTime))
				{
					return taiWanDateTime;
				}
			}

			return SystemContext.Now();
		}

		public static DateTime GetTaiwanAircraftPartCAACodeCategoryLastUpdateDateTimeFromHtml(string html)
		{
			return GetPublicationDateTimeUseHtmlDocument(html, (htmlDocument) => htmlDocument.DocumentNode.SelectNodes("//div[@class='lastupdated']")?.FirstOrDefault());
		}

		public static DateTime GetTariffLastUpdateDateTimeFromHtml(string html)
		{
			return GetPublicationDateTimeUseHtmlDocument(html, (htmlDocument) => htmlDocument.DocumentNode.SelectNodes("//font[@color='blue']")?.FirstOrDefault(c => c.InnerText.Contains("最後更新時間")));
		}

		static DateTime GetPublicationDateTimeUseHtmlDocument(string html, Func<HtmlDocument, HtmlNode> getLastUpdatedDateNode)
		{
			var htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(html);
			var lastUpdatedDateNode = getLastUpdatedDateNode(htmlDocument);
			var lastUpdatedDateInnerText = lastUpdatedDateNode?.InnerText.Trim() ?? "";
			var publicationDateRegex = new Regex(DateTimeRegexString);
			var matches = publicationDateRegex.Matches(lastUpdatedDateInnerText).Cast<Match>();
			return matches.Any() ? GetPublicationDateTimeFromDateString(matches.First().Value) : SystemContext.Now();
		}

		public static string GetDischargingStoringFileUrlsFromHtml(string html, string baseUrl)
		{
			var regex = new Regex("<a\\s+class.*href\\s*=\\s*\"(.+)\"\\s*.*\\n*.*title.*卸存地點.*.od[ts]", RegexOptions.IgnoreCase);
			Func<Match, string> getrelativeUrl = (match) => match.Groups[1].Value;
			var errorMessage = $"Discharging Storing file url can not be found from {html}.";

			return GetUrlStringFromHtml(baseUrl, html, regex, getrelativeUrl, errorMessage);
		}

		static string GetUrlStringFromHtml(string baseUrl, string html, Regex regex, Func<Match, string> getrelativeUrl, string errorMessage)
		{
			var matches = regex.Matches(html).Cast<Match>();
			if (matches.Any())
			{
				var slash = '/';
				return string.Join(";", matches.Select(match => $"{baseUrl.TrimEnd(slash)}{slash}{getrelativeUrl(match).TrimStart(slash)}"));
			}

			throw new Exception(errorMessage);
		}

		static long CharToNumber(char c)
		{
			switch (c)
			{
				case '一':
					return 1L;
				case '二':
					return 2L;
				case '三':
					return 3L;
				case '四':
					return 4L;
				case '五':
					return 5L;
				case '六':
					return 6L;
				case '七':
					return 7L;
				case '八':
					return 8L;
				case '九':
					return 9L;
				case '零':
					return 0L;
				default:
					return -1L;
			}
		}

		static long CharToUnit(char c)
		{
			switch (c)
			{
				case '十':
					return 10L;
				case '百':
					return 100L;
				case '千':
					return 1000L;
				case '萬':
					return 10000L;
				case '億':
					return 100000000L;
				default:
					return 1L;
			}
		}

		public static string ParseCnToIntString(string cnum)
		{
			cnum = Regex.Replace(cnum, "\\s+", "");
			var firstUnit = 1L;
			var secondUnit = 1L;
			var result = 0L;
			for (var i = cnum.Length - 1; i > -1; --i)
			{
				var tmpUnit = CharToUnit(cnum[i]);
				if (tmpUnit > firstUnit)
				{
					firstUnit = tmpUnit;
					secondUnit = 1;
					if (i == 0)
					{
						result += firstUnit;
					}
					continue;
				}
				if (tmpUnit > secondUnit)
				{
					secondUnit = tmpUnit;
					continue;
				}
				result += firstUnit * secondUnit * CharToNumber(cnum[i]);
			}
			return result.ToString();
		}
	}
}
