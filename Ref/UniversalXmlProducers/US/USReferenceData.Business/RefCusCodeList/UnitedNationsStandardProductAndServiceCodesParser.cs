using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.USReferenceData.Services;
using FlexCel.XlsAdapter;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Path = System.IO.Path;

namespace CargoWise.RefDbRepo.USReferenceData.Business
{
	public class UnitedNationsStandardProductAndServiceCodesParser : RefCusCodeListParser<CodeListWithAttributesProvider>
	{
		public UnitedNationsStandardProductAndServiceCodesParser(string outputFileDirectoryPath)
		{
			this.outputFileDirectoryPath = outputFileDirectoryPath;
		}

		readonly string outputFileDirectoryPath;

		protected override XmlWriterConfiguration XmlWriterConfiguration => RefCusCodeListParserHelper.GetWriterConfiguration_NoDefaultDate_HasAttributes(Constants.ProgramFunctions.UnitedNationsStandardProductAndServiceCodes);

		protected override string DataSourse => "United Nations Standard Product and Service Codes";

		protected override DateTime PublicationDateTime => publicationDateTime;
		DateTime publicationDateTime;

		protected override UpdateType UpdateType => UpdateType.Full;

		protected override string OutputFilePath => Path.Combine(outputFileDirectoryPath, "United_Nations_Standard_Product_and_Service_Codes.xml");

		protected static string InputPDFFilePath => Path.Combine(Path.GetTempPath(), "United Nations Standard Product and Service Codes.pdf");

		protected static string InputExcelFilePath => Path.Combine(Path.GetTempPath(), "Copy of CEMS Active UNSPSC Codes.xlsx");

		ExcelParserConfiguration ExcelParserConfiguration => excelParserConfiguration ?? (excelParserConfiguration = new ExcelParserConfiguration());
		ExcelParserConfiguration excelParserConfiguration;

		protected override List<CodeListWithAttributesProvider> GetCodeLists()
		{
			var result = new List<CodeListWithAttributesProvider>();

			DateTime currentCreatedOn;
			GetLastRowCreatedOn(InputExcelFilePath, false, out currentCreatedOn);
			var downloadedExcel = DownLoadExcelFile();

			var downloadedPDF = DownLoadFileAndGetPublicationDateTime();
			if (downloadedExcel && downloadedPDF)
			{
				if (!File.Exists(InputExcelFilePath))
				{
					AddLog("Can not find the excel file for downloading.");
				}
				else
				{
					if (!ShouldBeUpdated(currentCreatedOn))
					{
						AddLog("It doesn't need to be updated. The excel file is up to date.");
					}
					else
					{
						AddLog("Have got the Excel file and ready to start converting it to intermediate data.");
						try
						{
							var attributes = Parse(InputExcelFilePath);
							if (attributes.Item1.Count != 0)
							{
								CreateCodeListWithAttributesProvider(result, attributes.Item1, new List<Tuple<string, string>>() { new Tuple<string, string>(Constants.AttributeNames.USDA_AMS, Constants.AttributeValues.Y), new Tuple<string, string>(Constants.AttributeNames.USDA_AMS_PGM, Constants.AttributeValues.OTH) });
								AddLog("Intermediate data parsing is complete. (Set USDA_AMS_PGM OTH attribute)");
							}
							else
							{
								AddLog("No USDA_AMS_PGM OTH attribute is added.");
							}
							if (attributes.Item2.Count != 0)
							{
								CreateCodeListWithAttributesProvider(result, attributes.Item2, new List<Tuple<string, string>>() { new Tuple<string, string>(Constants.AttributeNames.USDA_AMS, Constants.AttributeValues.Y), new Tuple<string, string>(Constants.AttributeNames.USDA_AMS_PGM, Constants.AttributeValues.MO6) });
								AddLog("Intermediate data parsing is complete. (Set USDA_AMS_PGM MO6 attribute)");
							}
							else
							{
								AddLog("No USDA_AMS_PGM MO6 attribute is added.");
							}
						}
						catch (Exception ex)
						{
							throw new InvalidOperationException("Conversion failure" + ex.ToString());
						}

						using (var pages = new PdfReader(InputPDFFilePath))
						using (var pdfDocument = new PdfDocument(pages))
						{
							AddLog("Have got the PDF file and ready to start converting it to intermediate data.");
							var eggUNSPSCList = new List<string>();
							var peanutUNSPSCList = new List<string>();

							try
							{
								for (var page = 4; page <= pdfDocument.GetNumberOfPages(); page++)
								{
									ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
									var currentText = PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage(page), strategy);

									if (!EGG_UNSPSC_CODESHasBeenCaptured)
									{
										var textList = currentText.Split(new string[] { "EGG UNSPSC CODES:" }, StringSplitOptions.RemoveEmptyEntries);
										if (textList.Length == 2)
										{
											EGG_UNSPSC_CODESHasBeenCaptured = true;
											eggUNSPSCList.AddRange(MergeDataForNewLines(textList[1], true));
										}
									}
									else if (!PEANUT_UNSPSC_CODESHasBeenCaptured)
									{
										var textList = currentText.Split(new string[] { "PEANUT UNSPSC CODES:" }, StringSplitOptions.RemoveEmptyEntries);
										if (textList.Length == 2)
										{
											PEANUT_UNSPSC_CODESHasBeenCaptured = true;
											peanutUNSPSCList.AddRange(MergeDataForNewLines(textList[1], true));
										}
									}
								}
							}
							catch (Exception ex)
							{
								throw new InvalidOperationException("Conversion failure" + ex.ToString());
							}

							var eggUNSPSCDictionary = GetCodeAndDescriptionAndStartDate(eggUNSPSCList, true);
							var peanutUNSPSCDictionary = GetCodeAndDescriptionAndStartDate(peanutUNSPSCList, true);

							CreateCodeListWithAttributesProvider(result, eggUNSPSCDictionary, new List<Tuple<string, string>>() { new Tuple<string, string>(Constants.AttributeNames.USDA_AMS, Constants.AttributeValues.Y), new Tuple<string, string>(Constants.AttributeNames.USDA_AMS_PGM, Constants.AttributeValues.EG1) });
							CreateCodeListWithAttributesProvider(result, peanutUNSPSCDictionary, new List<Tuple<string, string>>() { new Tuple<string, string>(Constants.AttributeNames.USDA_AMS, Constants.AttributeValues.Y), new Tuple<string, string>(Constants.AttributeNames.USDA_AMS_PGM, Constants.AttributeValues.PN1) });

							AddLog("Intermediate data parsing is complete. (Set USDA_AMS_PGM EG1 and PN1 attribute)");
						}
					}
				}
			}

			return result;
		}

		protected bool ShouldBeUpdated(DateTime currentCreatedOn)
		{
			var result = false;

			DateTime latestCreatedOn;
			GetLastRowCreatedOn(InputExcelFilePath, true, out latestCreatedOn);

			if (currentCreatedOn != latestCreatedOn)
			{
				result = true;
				if (latestCreatedOn > publicationDateTime)
				{
					publicationDateTime = latestCreatedOn;
				}
			}

			return result;
		}

		protected void GetLastRowCreatedOn(string inputFilePath, bool isNewlyDownloaded, out DateTime lastRowCreatedOn)
		{
			DateTime result = RefCusCodeListParserHelper.MinSmallDateTimeValue;

			if (!File.Exists(inputFilePath))
			{
				lastRowCreatedOn = result;
				return;
			}

			if (CanOpenDownloadedExcel(isNewlyDownloaded))
			{
				var xls = new XlsFile(inputFilePath, false)
				{
					ActiveSheet = ExcelParserConfiguration.SheetIndex
				};

				var rowCount = xls.GetRowCount(xls.ActiveSheet);
				rowCount = Math.Min(rowCount, ExcelParserConfiguration.LastRow);

				var colCount = xls.ColCountInRow(rowCount);

				for (var col = 1; col <= colCount + 1; col++)
				{
					var header = xls.GetStringFromCell(ExcelParserConfiguration.HeaderRow, col);
					var cell = xls.GetStringFromCell(rowCount, col);

					if (header.Value == "Created On")
					{
						_ = DateTime.TryParse(cell, out result);
					}
				}

				lastRowCreatedOn = result;
			}
			else
			{
				lastRowCreatedOn = result;
			}
		}

		public (Dictionary<string, Tuple<string, DateTime>>, Dictionary<string, Tuple<string, DateTime>>) Parse(string inputFilePath)
		{
			var attributeOTH = new Dictionary<string, Tuple<string, DateTime>>();
			var attributeMO6 = new Dictionary<string, Tuple<string, DateTime>>();
			var result = (attributeOTH, attributeMO6);

			if (!File.Exists(inputFilePath))
			{
				return result;
			}

			var xls = new XlsFile(inputFilePath, false)
			{
				ActiveSheet = ExcelParserConfiguration.SheetIndex
			};

			var rowCount = xls.GetRowCount(xls.ActiveSheet);
			rowCount = Math.Min(rowCount, ExcelParserConfiguration.LastRow);

			for (var row = ExcelParserConfiguration.StartingRow; row <= rowCount; row++)
			{
				var colCount = xls.ColCountInRow(row);
				var isBlankRow = true;
				string code = "";
				string title = "";
				string variety = "";
				string subject = "";
				DateTime createdOn = RefCusCodeListParserHelper.MinSmallDateTimeValue;

				for (var col = 1; col <= colCount + 1; col++)
				{
					var header = xls.GetStringFromCell(ExcelParserConfiguration.HeaderRow, col);
					var cell = xls.GetStringFromCell(row, col);

					if (!string.IsNullOrEmpty(cell))
					{
						isBlankRow = false;
					}

					switch (header.Value)
					{
						case "UNSPSC":
							code = cell;
							break;
						case "Variety":
							variety = cell;
							break;
						case "Class Title":
							title = cell;
							break;
						case "Subject":
							subject = cell;
							break;
						case "Created On":
							_ = DateTime.TryParse(cell, out createdOn);
							break;
						default:
							break;
					}

					if (!isBlankRow && !string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(variety) && !string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(subject))
					{
						var description = variety + "; " + title;
						if (subject == "Yes")
						{
							if (!attributeOTH.ContainsKey(code))
							{
								attributeOTH.Add(code, new Tuple<string, DateTime>(description, createdOn));
							}
							else
							{
								attributeOTH[code] = new Tuple<string, DateTime>(description, createdOn);
							}
						}
						else if (subject == "No")
						{
							if (!attributeMO6.ContainsKey(code))
							{
								attributeMO6.Add(code, new Tuple<string, DateTime>(description, createdOn));
							}
							else
							{
								attributeMO6[code] = new Tuple<string, DateTime>(description, createdOn);
							}
						}
					}
				}
			}

			return (attributeOTH, attributeMO6);
		}

		protected const bool CODESHasBeenCapturedDefaultValue = false;
		bool EGG_UNSPSC_CODESHasBeenCaptured = CODESHasBeenCapturedDefaultValue;
		bool PEANUT_UNSPSC_CODESHasBeenCaptured = CODESHasBeenCapturedDefaultValue;

		static List<string> MergeDataForNewLines(string pageText, bool startWith50)
		{
			var result = new List<string>();
			var stringArray = pageText.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
			var record = "";
			foreach (var s in stringArray.Select(x => x.Trim()))
			{
				if (Regex.IsMatch(s, @"^[a-z].*") || (!startWith50 && Regex.IsMatch(s, @"^50")))
				{
					record = record + " " + s;
				}
				else
				{
					if (!string.IsNullOrEmpty(record))
					{
						result.Add(record);
						record = "";
					}
					record = s;
				}
			}
			if (!string.IsNullOrEmpty(record))
			{
				result.Add(record);
			}
			return result;
		}

		static Dictionary<string, Tuple<string, DateTime>> GetCodeAndDescriptionAndStartDate(List<string> records, bool startWith50)
		{
			var result = new Dictionary<string, Tuple<string, DateTime>>();
			foreach (var record in records)
			{
				var stringArray = Regex.Split(record, @"50[0-9]{6}");
				if (stringArray.Length == 2)
				{
					var code = record.Substring(stringArray[0].Length, 8);
					string description;
					if (startWith50)
					{
						description = stringArray[1].Trim().Substring(1);
					}
					else
					{
						description = string.Format(CultureInfo.InvariantCulture, "{0}; {1}", stringArray[0].Trim(), stringArray[1].Trim());
					}
					result.Add(code, new Tuple<string, DateTime>(description, RefCusCodeListParserHelper.MinSmallDateTimeValue));
				}
			}

			return result;
		}

		static void CreateCodeListWithAttributesProvider(List<CodeListWithAttributesProvider> codeLists, Dictionary<string, Tuple<string, DateTime>> codeAndDescriptionsAndCreatedOn, List<Tuple<string, string>> attributeNameAndValues)
		{
			foreach (var codeAndDescriptionAndCreatedOn in codeAndDescriptionsAndCreatedOn)
			{
				var code = codeAndDescriptionAndCreatedOn.Key;
				var codeList = codeLists.FirstOrDefault(x => x.Code == code);
				if (codeList == null)
				{
					codeList = new CodeListWithAttributesProvider();
					codeLists.Add(codeList);
				}
				codeList.Code = code;
				codeList.Description = codeAndDescriptionAndCreatedOn.Value.Item1;
				codeList.StartDate = codeAndDescriptionAndCreatedOn.Value.Item2;
				codeList.EndDate = RefCusCodeListParserHelper.MaxSmallDateTimeValue;

				var codeListAttributes = codeList.Attributes?.ToList() ?? new List<ICodeListAttribute>();
				foreach (var attributeNameAndValue in attributeNameAndValues)
				{
					var attributeName = attributeNameAndValue.Item1;
					var attributeValue = attributeNameAndValue.Item2;
					if (!codeListAttributes.Any(x => x.Name == attributeName && x.Value == attributeValue))
					{
						var codeListAttribute = new CodeListAttributeProvider();
						codeListAttribute.Name = attributeName;
						codeListAttribute.Value = attributeValue;
						codeListAttributes.Add(codeListAttribute);
					}
				}
				if (codeAndDescriptionAndCreatedOn.Value.Item1.ToUpper(CultureInfo.InvariantCulture).Contains("ORGANIC", StringComparison.Ordinal))
				{
					var attributeName = Constants.AttributeNames.USDA_AMS_PGM;
					var attributeValue = Constants.AttributeValues.OR1;
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

		protected virtual bool DownLoadExcelFile()
		{
			var result = false;

			try
			{
				result = ServiceClient.DownloadFile(ApplicationConfig.Instance.UnitedNationsStandardProductAndServiceCodesURLForExcel, InputExcelFilePath);
				if (!result || !CanOpenDownloadedExcel(true))
				{
					throw new InvalidOperationException("Failed to download correct Excel file.");
				}
			}
			catch (InvalidOperationException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException("Error while downloading the Excel file." + ex.ToString());
			}

			return result;
		}

		protected bool CanOpenDownloadedExcel(bool isNewlyDownloaded)
		{
			var result = false;

			try
			{
				var xls = new XlsFile(InputExcelFilePath, false)
				{
					ActiveSheet = ExcelParserConfiguration.SheetIndex
				};

				result = true;
			}
			catch (Exception ex)
			{
				if (!isNewlyDownloaded)
				{
					Console.Error.WriteLine("Error while opening the Excel file." + ex.ToString());
				}
				else
				{
					throw new InvalidOperationException("Error while opening the Excel file." + ex.ToString());
				}
			}

			return result;
		}

		protected virtual bool DownLoadFileAndGetPublicationDateTime()
		{
			var result = false;

			try
			{
				var dateTimeNode = ServiceClient.FindNode(ApplicationConfig.Instance.UnitedNationsStandardProductAndServiceCodesURLForPDF, node => node.Name == Constants.HtmlNodeNames.SPAN && node.InnerText.ToUpper(CultureInfo.InvariantCulture).Contains(DateTimeKeyWord));

				if (dateTimeNode != null)
				{
					var match = Regex.Match(dateTimeNode.InnerText, DateRegex);
					if (match.Success && DateTime.TryParse(match.Value, out publicationDateTime))
					{
						var htmlNode = ServiceClient.FindNode(ApplicationConfig.Instance.UnitedNationsStandardProductAndServiceCodesURLForPDF, node => node.Name == Constants.HtmlNodeNames.A && node.InnerText.Contains(PDFKeyWord));
						if (htmlNode != null)
						{
							result = ServiceClient.DownloadFile(htmlNode.OuterHtml, ApplicationConfig.Instance.CustomsBorderProtectionGoverment, InputPDFFilePath);
						}
						else
						{
							AddLog("The layout of the website has changed. Failed to get PDF download address.");
						}
					}
					else
					{
						AddLog("Failed to obtain publication time.");
					}
				}
				else
				{
					AddLog("The layout of the website has changed. Failed to obtain publication time.");
				}
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException("Error while downloading the PDF file or getting the publication date" + ex.ToString());
			}

			return result;
		}
		const string DateTimeKeyWord = "LAST MODIFIED:";
		const string PDFKeyWord = "AMS CATAIR Guidelines";
		const string DateRegex = @"[A-Za-z]+\s\d{1,2},\s\d{4}";

		public IDownLoadService ServiceClient
		{
			get
			{
				if (serviceClient == null)
				{
					serviceClient = new DownLoadService();
				}
				return serviceClient;
			}
			set
			{
				serviceClient = value;
			}
		}
		IDownLoadService serviceClient;
	}
}
