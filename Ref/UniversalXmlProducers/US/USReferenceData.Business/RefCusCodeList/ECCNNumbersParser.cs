using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using CargoWise.RefDbRepo.USReferenceData.Services;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.USReferenceData.Business
{
	public class ECCNNumbersParser : RefCusCodeListParser<CodeListWithAttributesProvider>
	{
		public ECCNNumbersParser(string url, string outputPath)
		{
			Argument.NotNullOrEmpty(url, nameof(url));
			Argument.NotNullOrEmpty(outputPath, nameof(outputPath));
			this.OutputFileDirectoryPath = outputPath;
			this.menuListURL = url;
		}
		readonly string menuListURL;
		string baseURL = ApplicationConfig.Instance.EARFilesBaseURL;
		string keyWord = "Category ";

		string OutputFileDirectoryPath { get; }

		public virtual IHttpClientHelper ServiceClient
		{
			get
			{
				if (downLoadService == null)
				{
					downLoadService = new HttpClientHelper();
				}
				return downLoadService;
			}
		}
		IHttpClientHelper downLoadService;

		public ICsvParser CSVParser
		{
			get
			{
				if (csvparser == null)
				{
					csvparser = new CsvParser();
				}
				return csvparser;
			}
		}
		ICsvParser csvparser;

		protected override XmlWriterConfiguration XmlWriterConfiguration => RefCusCodeListParserHelper.GetWriterConfiguration_HasDefaultDate_HasAttributes(Constants.ProgramFunctions.ECCNNumbers, new DateTime(1900, 01, 01), new DateTime(2079, 06, 06, 23, 59, 00));

		protected override string DataSourse => "US Export Administration Regulations ECCN Numbers";

		protected override DateTime PublicationDateTime => new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);

		protected override UpdateType UpdateType => UpdateType.Full;

		protected override string OutputFilePath => Path.Combine(OutputFileDirectoryPath, "US_EAR_ECCNNumbers.xml");

		protected override List<CodeListWithAttributesProvider> GetCodeLists()
		{
			var result = new List<CodeListWithAttributesProvider>();

			var meuList = ECCNAttributeList.GetMEU(CSVParser);
			var licenseList = ECCNAttributeList.GetLicenseType(CSVParser);
			var attributeList = meuList.Concat(licenseList).ToList();

			var eccnDic = new Dictionary<string, string>();
			var pageLink = GetCategoryPageLink();
			ParseCategoryPage(pageLink, eccnDic);

			CreateCodeListWithAttributes(result, eccnDic, attributeList);
			return result;
		}

		string GetCategoryPageLink()
		{
			var page = new HtmlDocument();
			page.LoadHtml(ServiceClient.GetWebPageAsync(menuListURL).Result);
			var firstNode = page.DocumentNode.Descendants().FirstOrDefault(node => node.Name == Constants.HtmlNodeNames.A && node.InnerText.Contains(keyWord));
			var link = firstNode.GetAttributeValue("href", string.Empty);
			link = link.StartsWith(baseURL, StringComparison.OrdinalIgnoreCase) ? link : baseURL + link;
			return link;
		}

		void ParseCategoryPage(string pageLink, Dictionary<string, string> codeAndDescriptions)
		{
			var page = new HtmlDocument();
			page.LoadHtml(ServiceClient.GetWebPageAsync(pageLink).Result);
			var appendixNode = page.DocumentNode.Descendants().Where(node => node.Name == "div" && node.GetAttributeValue("class", "") == "appendix").FirstOrDefault();

			if (appendixNode != null)
			{
				Console.WriteLine("Start parsing ECCN Number from class and style...");
				ParseCodeAndDescriptionFromHTMLBTag(appendixNode, codeAndDescriptions);
				var count = codeAndDescriptions.Count;
				Console.WriteLine($"Parse {count} ECCN Numbers from class and style");
			}
			else
			{
				Console.WriteLine("No appendix node found in the HTML document.");
			}

			void ParseCodeAndDescriptionFromHTMLBTag(HtmlNode appendixNode, Dictionary<string, string> codeAndDescriptions)
			{
				var nodes = appendixNode.Descendants("p").Where(p => p.GetAttributeValue("class", "").StartsWith("mb-4",StringComparison.OrdinalIgnoreCase)).SelectMany(p => p.Descendants("strong"));
				var codeStartNumber = string.Empty;
				var previousCode = string.Empty;
				foreach (var node in nodes)
				{
					var text = node.InnerText.Trim();
					var code = text.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
					ParseCodeAndDescription(code, text, code.StartsWith(codeStartNumber, StringComparison.OrdinalIgnoreCase), codeAndDescriptions, ref previousCode);
				}
			}
		}

		static bool IsECCNNumber(string code)
		{
			return code != null && code.Trim().Length == 5 && Regex.IsMatch(code, @"^[0-9]{1}[A-Z]{1}[0-9]{3}");
		}

		static bool IsLastLine(string text)
		{
			return text.EndsWith(".", StringComparison.CurrentCultureIgnoreCase) || text.EndsWith(")", StringComparison.CurrentCultureIgnoreCase);
		}

		static string ParseDescription(string text)
		{
			var description = text.Trim().Replace("\r\n", "").Replace("\n", "");
			if (description.Length > 3997)
			{
				description = description.Substring(0, 3997) + "...";
			}
			return Regex.Replace(description, @"\s+", " ");
		}

		static void ParseCodeAndDescription(string code, string text, bool otherConditions, Dictionary<string, string> codeAndDescriptions, ref string previousCode)
		{
			var textDecoded = WebUtility.HtmlDecode(text);
			if (IsECCNNumber(code) && otherConditions)
			{
				if (!codeAndDescriptions.ContainsKey(code))
				{
					var description = ParseDescription(textDecoded).Substring(5).TrimStart();
					codeAndDescriptions.Add(code, description);
					previousCode = IsLastLine(description) ? string.Empty : code;
				}
			}
			else if (!string.IsNullOrEmpty(previousCode))
			{
				codeAndDescriptions.TryGetValue(previousCode, out var description);
				description += textDecoded;
				codeAndDescriptions[previousCode] = ParseDescription(description);
				previousCode = IsLastLine(description) ? string.Empty : previousCode;
			}
		}

		static void CreateCodeListWithAttributes(List<CodeListWithAttributesProvider> codeLists, Dictionary<string, string> codeAndDescriptions, IEnumerable<ECCNAttribute> attributeNameAndValues)
		{
			foreach (var codeAndDescription in codeAndDescriptions)
			{
				try
				{
					var code = codeAndDescription.Key;
					var codeList = codeLists.FirstOrDefault(x => x.Code == code);
					if (codeList == null)
					{
						codeList = new CodeListWithAttributesProvider();
						codeList.Code = code;
						codeList.Description = codeAndDescription.Value;
						codeLists.Add(codeList);
					}

					var codeListAttributes = codeList.Attributes?.ToList() ?? new List<ICodeListAttribute>();
					if (attributeNameAndValues.Any(x => x.Code == code))
					{
						var relatedAttributes = attributeNameAndValues.Where(x => x.Code == code);
						foreach (var attributeNameAndValue in relatedAttributes)
						{
							var attributeName = attributeNameAndValue.AttributeName;
							var attributeValue = attributeNameAndValue.AttributeValue;
							if (!codeListAttributes.Any(x => x.Name == attributeName && x.Value == attributeValue))
							{
								var codeListAttribute = new CodeListAttributeProvider();
								codeListAttribute.Name = attributeName;
								codeListAttribute.Value = attributeValue;
								codeListAttributes.Add(codeListAttribute);
							}
						}
						codeList.Attributes = codeListAttributes;
					}
				}
				catch (Exception ex)
				{
					Console.Error.WriteLine(string.Format(CultureInfo.InvariantCulture, "Creating Code List [key-{0}] failure: {1}", codeAndDescription.Key, ex.ToString()));
				}
			}
		}
	}
}
