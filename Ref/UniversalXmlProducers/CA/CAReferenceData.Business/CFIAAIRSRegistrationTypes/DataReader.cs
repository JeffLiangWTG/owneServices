using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CFIAAIRSRegistrationTypes
{
	public class DataReader
	{
		public DataReader(string englishFilePath, string frenchFilePath, string exportFilePath, IHttpClientHelper herlper = null, string matchURL = "")
		{
			this.englishFilePath = englishFilePath;
			this.frenchFilePath = frenchFilePath;
			RefCusCodeLists = new List<RefCusCodeList>();
			this.exportFilePath = exportFilePath;
			this.entityMatcherCountryUri = string.IsNullOrEmpty(matchURL) ? string.Format(CultureInfo.InvariantCulture, ApplicationConfig.EntityMatcherUri, Constants.EntityMatcherCOUNTRYUri) : matchURL;
			this.helper = herlper ?? new HttpClientHelper();
		}

		private List<RefCusCodeList> RefCusCodeLists;
		private string exportFilePath;
		readonly string entityMatcherCountryUri;
		readonly IHttpClientHelper helper;

		const string CAART_CODE = "CAART";
		const string CALPC_CODE = "CALPC";
		const string FORMAT_NAME = "FORMAT";
		const string MATERIALIZED_NAME = "MATERIALIZED";
		const string MATERIALIZEDCOUNTRY_NAME = "MATERIALIZEDCOUNTRY";
		const string DEMATERIALIZEDCOUNTRY_NAME = "DEMATERIALIZEDCOUNTRY";
		const string UNKNOWNDUPLICATECODE_NAME = "UNKNOWNDUPLICATECODE";
		const string FRANCE_CODE = "FR";
		const string CALPCM_CODE = "CALPCM";
		const string CALPCD_CODE = "CALPCD";
		const string MATERIALIZED_ABBR = "M";
		const char ALPHANUMERIC = 'A';
		const char NUMERIC = 'N';
		const char CONFIRMATION = 'C';

		public bool ReadHtmlAndExportXML()
		{
			var htmlDoc = new HtmlDocument();
			htmlDoc.Load(englishFilePath, Encoding.UTF8);

			var table = htmlDoc.DocumentNode.SelectSingleNode("//table[caption[contains(text(), 'AIRS Registration types and LPCO (materialized and de-materialized) code, descriptions and format')]]");
			if (table != null)
			{
				var rows = table.SelectNodes(".//tbody/tr");
				if (rows != null)
				{
					var currentTextBuilder = new StringBuilder();
					foreach (var row in rows)
					{
						var cells = row.SelectNodes("td");
						if (cells?.Count >= 8)
						{
							var code = cells[0].InnerText.Trim();
							var description = HttpUtility.HtmlDecode(cells[1].InnerText.Trim().Replace("&nbsp;", " "));
							var format = cells[2].InnerText.Trim();
							var paperAIRS = cells[3].SelectSingleNode(".//span[contains(@class, 'glyphicon-ok')]") != null ? "" : "";
							var paperFax = cells[4].SelectSingleNode(".//span[contains(@class, 'glyphicon-ok')]") != null ? "" : "";
							var iidAIRS = cells[5].SelectSingleNode(".//span[contains(@class, 'glyphicon-ok')]") != null ? "" : "";
							var iidMat = cells[6].SelectSingleNode(".//span[contains(@class, 'glyphicon-ok')]") != null ? "" : "";
							var iidDeMat = cells[7].SelectSingleNode(".//span[contains(@class, 'glyphicon-ok')]") != null ? "" : "";
							currentTextBuilder.Append(code + " " + description + " " + format + " " + paperAIRS + " " + paperFax + " " + iidAIRS + " " + iidMat + " " + iidDeMat + " \n");
						}
					}

					ExtractData(currentTextBuilder.ToString(0, currentTextBuilder.Length - 1));
					AddFrenchDescription();
				}
			}

			if (RefCusCodeLists.Any())
			{
				var records = new List<RefDataRepoModelEntityType>();

				var xmlWriter = new XmlWriter(XmlWriterHelper.GetRefCusCodelistConfiguration());
				xmlWriter.SetPublicationTime(DateTime.Now);
				xmlWriter.SetUpdateType(UpdateType.Full);
				xmlWriter.SetDataSource(Constants.DataSource.CFIAAIRSRegistrationTypes);

				records.AddRange(RefCusCodeLists);

				records.ForEach(x => xmlWriter.PopulateData(x));
				xmlWriter.SaveXml(exportFilePath);
			}

			return true;
		}

		void ExtractData(string text)
		{
			var pattern = new Regex("\n[0-9]+ ");
			var matches = pattern.Matches(text);
			var currentText = text;

			for (var i = matches.Count - 1; i >= 0; i--)
			{
				var match = matches[i];
				currentText = CreateRefCusCodeList(match, currentText);
			}

			if (currentText.Length > 0)
			{
				var first = Regex.Match(currentText, "[0-9]+ ");
				if (first != null && first.Length > 0 && first.Index == 0)
				{
					CreateRefCusCodeList(first, currentText);
				}
			}
		}

		void AddFrenchDescription()
		{
			var frenchDic = new Dictionary<string, string>();

			var htmlDoc = new HtmlDocument();
			htmlDoc.Load(frenchFilePath, Encoding.UTF8);

			var table = htmlDoc.DocumentNode.SelectSingleNode("//table[caption[contains(text(), \"Codes, descriptions, et format des types d'enregistremen et LPCA (matérialisés et dématérialisés) du SARI\")]]");
			if (table != null)
			{
				var rows = table.SelectNodes(".//tbody/tr");
				if (rows != null)
				{
					foreach (var row in rows)
					{
						var cells = row.SelectNodes("td");
						if (cells == null || cells.Count < 8)
							continue;

						var key = cells[0].InnerText.Trim();
						var description = HttpUtility.HtmlDecode(cells[1].InnerText.Trim().Replace("&nbsp;", " "));
						if (frenchDic.TryGetValue(key, out var frenchDesc))
						{
							var descs = frenchDesc.Split('(');
							if (descs.Length > 0 && !string.IsNullOrEmpty(descs[0]))
							{
								frenchDic.Remove(key);
								frenchDic.Add(key, descs[0]);
							}
						}
						else
						{
							frenchDic.Add(key, description);
						}

					}
				}
			}

			foreach (var code in RefCusCodeLists)
			{
				string frenchDesc;
				if (frenchDic.TryGetValue(code.ZZD_Code, out frenchDesc))
				{
					var lan = code.RefCusCodeListLanguages?.FirstOrDefault(x => x.ZXA_ZX6_NKLanguage == FRANCE_CODE);
					if (lan == null)
					{
						var language = new RefCusCodeListLanguage();
						language.ZXA_ZX6_NKLanguage = FRANCE_CODE;
						language.ZXA_Description = frenchDesc;

						if (code.RefCusCodeListLanguages == null)
						{
							code.RefCusCodeListLanguages = new RefCusCodeListLanguage[] { language };
						}
						else
						{
							_ = code.RefCusCodeListLanguages.Append(language);
						}
					}
					else
					{
						lan.ZXA_Description = frenchDesc;
					}
				}
			}
		}

		string CreateRefCusCodeList(Match match, string text)
		{
			var formatIndex = GetFormatIndex(text);
			var flags = text.Substring(formatIndex + 1, text.Length - formatIndex - 1).TrimStart();
			var format = text[formatIndex];
			var description = text.Substring(match.Index + match.Length, formatIndex - match.Index - match.Length).Trim().Replace("\r", "").Replace("\n", "").Replace("\t", "");
			var leftText = text.Substring(0, match.Index);
			var codeType = GetCodeType(flags);
			if (string.IsNullOrEmpty(codeType))
			{
				throw new ArgumentException($"Unrecognized flags for {match.Value}");
			}
			var type = codeType.Substring(0, 5);
			var codeString = match.Value.Trim();
			var existing = RefCusCodeLists.FirstOrDefault(x => x.ZZD_Code == codeString);
			if (existing != null) // Duplicate code exists.
			{
				// Extract country codes from existing code.
				var descriptionAndCountryCodes = ExtractCountryCodes(existing.ZZD_Description.Replace("\r", "").Replace("\n", "").Replace("\t", ""));
				var newDescription = descriptionAndCountryCodes.Item1;
				var countryCodes = descriptionAndCountryCodes.Item2;

				if (string.IsNullOrEmpty(countryCodes)) // Does not include country codes.
				{
					// Extract country codes from read code.
					descriptionAndCountryCodes = ExtractCountryCodes(description);
					countryCodes = descriptionAndCountryCodes.Item2;

					// Read code also does not contain country codes, then add description to existing code's UNKNOWNDUPLICATECODE attribute.
					if (string.IsNullOrEmpty(countryCodes))
					{
						var unknownAttribute = new RefCusCodeListAttribute();
						unknownAttribute.ZZE_ZXE_NKName = UNKNOWNDUPLICATECODE_NAME;
						unknownAttribute.ZZE_Value = description;
						AddtoCusCodeListAttributes(existing, unknownAttribute);
					}
					else
					{
						var dematerializedAttribute = new RefCusCodeListAttribute();
						var existingAttribute = existing.RefCusCodeListAttributes.FirstOrDefault(x => x.ZZE_ZXE_NKName == MATERIALIZED_NAME);
						if (existingAttribute.ZZE_Value == "Y")
						{
							dematerializedAttribute.ZZE_ZXE_NKName = DEMATERIALIZEDCOUNTRY_NAME;
						}
						else
						{
							dematerializedAttribute.ZZE_ZXE_NKName = MATERIALIZEDCOUNTRY_NAME;
						}
						dematerializedAttribute.ZZE_Value = countryCodes;
						existing.ZZD_Description = newDescription;
						AddtoCusCodeListAttributes(existing, dematerializedAttribute);
					}
				}
				else
				{
					var newAttribute = new RefCusCodeListAttribute();
					var existingAttribute = existing.RefCusCodeListAttributes.FirstOrDefault(x => x.ZZE_ZXE_NKName == MATERIALIZED_NAME);
					if (existingAttribute.ZZE_Value == "Y")
					{
						newAttribute.ZZE_ZXE_NKName = MATERIALIZEDCOUNTRY_NAME;
						existingAttribute.ZZE_Value = "N";
					}
					else
					{
						newAttribute.ZZE_ZXE_NKName = DEMATERIALIZEDCOUNTRY_NAME;
						existingAttribute.ZZE_Value = "Y";
					}
					newAttribute.ZZE_Value = countryCodes;
					existing.ZZD_Description = newDescription;
					AddtoCusCodeListAttributes(existing, newAttribute);
				}
			}
			else
			{
				var code = new RefCusCodeList();
				code.ZZD_Code = codeString;
				code.ZZD_Description = description;
				code.ZZD_ZZK_NKCodeType = type;

				var attributeList = new List<RefCusCodeListAttribute>();

				var formatAttribute = new RefCusCodeListAttribute();
				formatAttribute.ZZE_ZXE_NKName = FORMAT_NAME;
				formatAttribute.ZZE_Value = format.ToString();
				attributeList.Add(formatAttribute);

				if (type == CALPC_CODE)
				{
					var typeAttribute = new RefCusCodeListAttribute();
					typeAttribute.ZZE_ZXE_NKName = MATERIALIZED_NAME;
					if (codeType.EndsWith(MATERIALIZED_ABBR, StringComparison.Ordinal))
					{
						typeAttribute.ZZE_Value = "Y";
					}
					else
					{
						typeAttribute.ZZE_Value = "N";
					}
					attributeList.Add(typeAttribute);
				}

				code.RefCusCodeListAttributes = attributeList.ToArray();

				RefCusCodeLists.Add(code);
			}

			return leftText;
		}

		static void AddtoCusCodeListAttributes(RefCusCodeList code, RefCusCodeListAttribute attribute)
		{
			var existingAttributes = code.RefCusCodeListAttributes.ToList();
			existingAttributes.Add(attribute);
			code.RefCusCodeListAttributes = existingAttributes.ToArray();
		}

		public (string, string) ExtractCountryCodes(string text)
		{
			var texts = text.Split('(');
			if (texts.Length > 1)
			{
				var description = texts[0];
				texts = texts[1].Split(')');
				if (texts.Length > 0)
				{
					var countries = texts[0].Split(new string[] { " and ", "," }, StringSplitOptions.None).Where(x => !string.IsNullOrWhiteSpace(x));
					var sanitizedCountryNames = countries.Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
					var results = Task.Run(async () => await helper.GetMatchedEntityCodesAsync(entityMatcherCountryUri, sanitizedCountryNames))?.Result;
					if (results != null)
					{
						if (results.Length != countries.Count())
						{
							return (null, null);
						}
						return (description, string.Join(",", results));
					}
				}
			}
			return (null, null);
		}

		static string GetCodeType(string text)
		{
			var result = string.Empty;
			var head = text.Substring(0, 3);
			var tail = text.Substring(3);
			if (head.Equals(" ", StringComparison.OrdinalIgnoreCase))
			{
				switch (tail)
				{
					case " \n \n \n \n":
					case " \n \n \n ":
					case " \n \n \n  ":
					case " \n \n \n \n ":
					case "    ":
						result = CALPCM_CODE;
						break;
					case " \n  \n \n":
					case " \n  \n ":
					case "    ":
						result = CALPCD_CODE;
						break;
					default:
						result = "";
						break;
				}
			}
			else if (head.Equals("  ", StringComparison.OrdinalIgnoreCase))
			{
				switch (tail)
				{
					case " \n  \n":
					case " \n  ":
					case "   \n":
					case "   ":
						result = CAART_CODE;
						break;
					case "   \n":
					case "   ":
						result = CALPCM_CODE;
						break;
					case "   \n":
					case "   ":
					case "\n  \n \n":
					case "\n  \n ":
					case "\n \n  \n":
					case "\n \n  ":
						result = CALPCD_CODE;
						break;
					default:
						break;
				}
			}
			return result;
		}

		static int GetFormatIndex(string text)
		{
			var indexOfA = text.LastIndexOf(ALPHANUMERIC);
			var indexOfC = text.LastIndexOf(CONFIRMATION);
			var indexOfN = text.LastIndexOf(NUMERIC);
			return (indexOfA > indexOfC) ? (indexOfA > indexOfN ? indexOfA : indexOfN) : (indexOfC > indexOfN ? indexOfC : indexOfN);
		}

		readonly string englishFilePath, frenchFilePath;
	}
}
