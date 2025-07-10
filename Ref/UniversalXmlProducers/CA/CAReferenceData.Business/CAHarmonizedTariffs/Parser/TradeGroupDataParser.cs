using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs
{
	public class TradeGroupDataParser
	{
		public TradeGroupDataParser(DateTime publicationTime, string outPutFilePath, IHttpClientHelper herlper = null, string matchURL = "")
		{
			this.publicationTime = publicationTime;
			this.outPutFilePath = outPutFilePath;
			this.helper = herlper ?? new HttpClientHelper();
			this.entityMatcherCountryUri = string.IsNullOrEmpty(matchURL) ? string.Format(CultureInfo.InvariantCulture, ApplicationConfig.EntityMatcherUri, Constants.EntityMatcherCOUNTRYUri) : matchURL;
		}
		readonly string entityMatcherCountryUri;
		readonly IHttpClientHelper helper;
		DateTime publicationTime;
		readonly string outPutFilePath;
		Dictionary<string, List<string>> tradeDictionary;

		public void ParseTradeGroupData(string workingFileOrDirectory)
		{
			ReadAndParsePDF(workingFileOrDirectory);
			var countries = ExtractCountryCodes();
			if (countries != null)
			{
				ParseRefCusTradeGroupIntoXML(countries);
			}
		}

		void ParseRefCusTradeGroupIntoXML(Dictionary<string, string> countries)
		{
			Console.WriteLine("Start parsing Trade Group.");
			Console.WriteLine($"There will be {tradeDictionary.Count} trade groups populated into the final XML");
			var _XmlWriter = new XmlWriter(XMLWriterHelper.GetRefCusTradeGroupWriterConfiguration());
			_XmlWriter.SetDataSource(Constants.DataSource.TradeGroup);
			_XmlWriter.SetUpdateType(UpdateType.Full);

			foreach (var trade in tradeDictionary)
			{
				var ttCode = ParserHelper.TTCodeList.Where(x => x.Abbreviation == trade.Key).FirstOrDefault();
				if (ttCode != null)
				{
					var tradeGroup = new RefCusTradeGroup()
					{
						ZZA_Description = ttCode.Description,
						ZZA_TradeGroup = ttCode.Code
					};

					var tradeGroupCountries = new List<RefCusTradeGroupCountry>();
					foreach (var country in trade.Value)
					{
						tradeGroupCountries.Add(
							new RefCusTradeGroupCountry()
							{
								ZZB_Description = country,
								ZZB_RN_NKTradeGroupCountryCode = countries[country]
							});
					}
					tradeGroup.RefCusTradeGroupCountries = tradeGroupCountries.ToArray();

					_XmlWriter.PopulateData(tradeGroup);
				}
			}

			_XmlWriter.SetPublicationTime(publicationTime);
			_XmlWriter.SaveXml(outPutFilePath);
			Console.WriteLine($"Final XML populated {outPutFilePath}.");
		}

		void ReadAndParsePDF(string filePath)
		{
			tradeDictionary = new Dictionary<string, List<string>>();
			using (var pdfReader = new PdfReader(filePath))
			using (var pdfDocument = new PdfDocument(pdfReader))
			{
				for (var i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
				{
					var page = pdfDocument.GetPage(i);
					var currentText = PdfTextExtractor.GetTextFromPage(page, new SimpleTextExtractionStrategy());
					currentText = Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(currentText)));
					ExtractData(currentText);
				}
			}
		}

		void ExtractData(string text)
		{
			var textList = text.Split(new string[] { "MFN GPT LDCT Other" }, System.StringSplitOptions.RemoveEmptyEntries);
			if (textList.Length == 2)
			{
				var maintext = textList[1];
				var pattern = new Regex(@"\n\w* \nX");
				var matches = pattern.Matches(maintext);
				if (matches.Count > 0)
				{
					foreach (Match match in matches)
					{
						var texts = match.Value;
						var newText = string.Join("", texts.Split(new string[] { "\n" }, System.StringSplitOptions.RemoveEmptyEntries));
						maintext = Regex.Replace(maintext, texts, newText);
					}
				}

				var trades = maintext.Trim().Split(new string[] { "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
				foreach (var trade in trades)
				{
					var countryNameAndOthers = trade.Split(new string[] { " X", "  X", "  " }, System.StringSplitOptions.RemoveEmptyEntries);
					var countryName = string.Empty;
					var others = string.Empty;
					if (countryNameAndOthers.Length > 0)
					{
						countryName = countryNameAndOthers[0].Trim();
						if (countryNameAndOthers.Length == 2)
						{
							others = countryNameAndOthers[1].Trim();
						}
					}

					var otherTrades = others.Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries);
					if (otherTrades.Any())
					{
						foreach (var otherTrade in otherTrades)
						{
							AddValueToDictionary(otherTrade.Trim(), countryName);
						}
					}

					var treatments = trade.Trim().Split(new string[] { countryName, others }, System.StringSplitOptions.RemoveEmptyEntries);
					if (treatments.Length == 1)
					{
						var tariffTrade = treatments[0];
						tariffTrade = Regex.Replace(tariffTrade, @"\s\sX", " X");
						var treatmentLength = tariffTrade.Length;
						if (treatmentLength >= 2 && tariffTrade[1].Equals('X'))
						{
							AddValueToDictionary("MFN", countryName);
						}
						if (treatmentLength >= 4 && tariffTrade[3].Equals('X'))
						{
							AddValueToDictionary("GPT", countryName);
						}
						if (treatmentLength >= 6 && tariffTrade[5].Equals('X'))
						{
							AddValueToDictionary("LDCT", countryName);
						}
					}
				}
			}
		}

		void AddValueToDictionary(string key, string value)
		{
			if (!tradeDictionary.ContainsKey(key))
			{
				tradeDictionary.Add(key, new List<string>());
			}
			tradeDictionary[key].Add(value);
		}

		Dictionary<string, string> ExtractCountryCodes()
		{
			var countries = tradeDictionary.SelectMany(x => x.Value).Distinct().ToList();
			var sanitizedCountryNames = countries.Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
			var results = Task.Run(async () => await helper.GetMatchedEntityCodesAsync(entityMatcherCountryUri, sanitizedCountryNames))?.Result;

			if (results != null && results.Length == sanitizedCountryNames.Length)
			{
				return countries.Zip(results, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);
			}
			return null;
		}
	}
}
