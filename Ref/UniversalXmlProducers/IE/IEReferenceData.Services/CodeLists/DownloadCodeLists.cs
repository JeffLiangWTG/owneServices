using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services.NCTS;
using CargoWise.RefDbRepo.IEReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using FlexCel.XlsAdapter;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using static iText.Kernel.Pdf.Colorspace.PdfSpecialCs;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services
{
	public class DownloadCodeLists
	{
		public DownloadCodeLists(IRevenueCodeListDetails[] codeListsToExtract)
		{
			if (codeListsToExtract == null || codeListsToExtract.Length == 0)
			{
				throw new ArgumentException("The codes to extract should not be null or empty", nameof(codeListsToExtract));
			}
			CodeListsToExtract = codeListsToExtract.GroupBy(x => x.ApplicationType).ToDictionary(x => x.Key, y => y.ToArray());
		}
		readonly Dictionary<ApplicationType, IRevenueCodeListDetails[]> CodeListsToExtract;

		enum FileType
		{
			PDF,
			XLSX
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
		public (string Error, IReadOnlyDictionary<(string DataGrouping, string Code), IRevenueExtractedCodeList> ExtractedCodeList) Download((ApplicationType applicationType, string url)[] downloadData, bool forceLoadFromFilePath = false)
		{
			var errorText = string.Empty;
			var extractedCodeLists = new Dictionary<(string, string), IRevenueExtractedCodeList>();
			foreach ((ApplicationType applicationType, string url) in downloadData)
			{
				try
				{
					if (string.IsNullOrWhiteSpace(url))
					{
						throw new InvalidOperationException("Invalid URI: The URI is empty.");
					}
					else
					{
						var fileType = GetFileType(url);
						if (fileType == FileType.XLSX)
						{
							if (forceLoadFromFilePath)
							{
								var xls = new XlsFile(url);
								var (versionNumber, versionDate) = GetVersionInfoFromXlsx(xls);
								ExtractCodeListDetails(xls, extractedCodeLists, versionNumber, versionDate, applicationType);
							}
							else
							{
								using (var client = new HttpClient())
								{
									client.DefaultRequestHeaders.Add("User-Agent", ApplicationConfig.Instance.DefaultHttpUserAgent);
									using (var streamAsync = client.GetStreamAsync(new Uri(url)))
									using (var memoryStream = new MemoryStream())
									{
										streamAsync.Result.CopyTo(memoryStream);
										memoryStream.Position = 0;
										var xls = new XlsFile();
										xls.Open(memoryStream);
										var (versionNumber, versionDate) = GetVersionInfoFromXlsx(xls);
										ExtractCodeListDetails(xls, extractedCodeLists, versionNumber, versionDate, applicationType);
									}
								}
							}
						}
						else if(fileType == FileType.PDF)
						{
							if (forceLoadFromFilePath)
							{
								using (var pdfReader = new PdfReader(url))
								using (var pdfDocument = new PdfDocument(pdfReader))
								{
									var versionNumber = GetVersionNumber(pdfDocument);
									var versionDate = GetVersionDate(pdfReader, pdfDocument, versionNumber, out var lastTableOfContentsPageNumber);

									ExtractCodeListDetails(pdfReader, pdfDocument, lastTableOfContentsPageNumber, extractedCodeLists, versionNumber, versionDate, applicationType);
								}
							}
							else
							{
								using (var client = new HttpClient())
								{
									client.DefaultRequestHeaders.Add("User-Agent", ApplicationConfig.Instance.DefaultHttpUserAgent);
									using (var streamAsync = client.GetStreamAsync(new Uri(url)))
									using (var pdfReader = new PdfReader(streamAsync.Result))
									using (var pdfDocument = new PdfDocument(pdfReader))
									{
										var versionNumber = GetVersionNumber(pdfDocument);
										var versionDate = GetVersionDate(pdfReader, pdfDocument, versionNumber, out var lastTableOfContentsPageNumber);

										ExtractCodeListDetails(pdfReader, pdfDocument, lastTableOfContentsPageNumber, extractedCodeLists, versionNumber, versionDate, applicationType);
									}
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					errorText = $"Unable to Download the IE CodeLists PDF from the following URL: {url} /r/n {ex.Message}";
				}
			}
			return (errorText, extractedCodeLists);
		}

		public (string Error, IReadOnlyDictionary<(string DataGrouping, string Code), IRevenueExtractedCodeList> ExtractedCodeList) Download(ApplicationType applicationType, string url, bool forceLoadFromFilePath = false)
		{
			return Download(new[] { (applicationType, url) }, forceLoadFromFilePath);
		}

		static (string VersionNumber, DateTime VersionDate) GetVersionInfoFromXlsx(XlsFile xls)
		{
			xls.ActiveSheet = 2; // Second sheet
			var versionNumber = xls.GetStringFromCell(2, 1).Trim();
			var versionDateStr = xls.GetStringFromCell(2, 2).Trim();

			var formats = new[] { "dd/MM/yyyy", "dd-MM-yyyy" };
			if (!DateTime.TryParseExact(versionDateStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var versionDate))
			{
				throw new InvalidOperationException($"Cannot parse the current version date for version {versionNumber}. Expected format dd/MM/yyyy or dd-MM-yyyy");
			}

			return (versionNumber, versionDate);
		}

		internal void ExtractCodeListDetails(XlsFile xls, Dictionary<(string DataGrouping, string Code), IRevenueExtractedCodeList> extractedCodeLists, string versionNumber, DateTime versionDate, ApplicationType applicationType)
		{
			// Read the code list index from the first sheet
			xls.ActiveSheet = 1;
			int rowCount = xls.RowCount;
			var codeListIndex = new List<(string Name, string Description, int SheetNumber)>();

			for (int row = 2; row <= rowCount; row++)
			{
				string name = xls.GetStringFromCell(row, 1).Trim();
				string description = xls.GetStringFromCell(row, 2).Trim();
				int sheetNumber = int.Parse(xls.GetStringFromCell(row, 4).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture) + 2;

				codeListIndex.Add((name, description, sheetNumber));
			}

			// Iterate over CodeListsToExtract and find the corresponding code list in codeListIndex
			if (CodeListsToExtract.TryGetValue(applicationType, out var codeLists))
			{
				foreach (var codeListDetails in codeLists)
				{
					var matchingCodeList = codeListIndex.FirstOrDefault(cl => cl.Description == codeListDetails.NameInFile);
					if (matchingCodeList != default)
					{
						xls.ActiveSheet = matchingCodeList.SheetNumber;
						int sheetRowCount = xls.RowCount;
						var codeDescriptionPairList = new Dictionary<string, IRevenueCodeDescriptionPair>();

						var additionalFilter = codeListDetails as IHaveAsyncAdditionalFilter;
						for (int row = 2; row <= sheetRowCount; row++)
						{
							string codeValue = xls.GetStringFromCell(row, 1).Trim();
							string descriptionValue = xls.GetStringFromCell(row, 2).Trim();

							var codeDescriptionPair = new RevenueCodeDescriptionPairProvider(codeValue, descriptionValue);
							if ((!string.IsNullOrEmpty(codeValue) && !string.IsNullOrEmpty(descriptionValue)) && (additionalFilter == null || additionalFilter.Filter(codeDescriptionPair).GetAwaiter().GetResult()))
							{
								codeDescriptionPairList.Add(codeValue, codeDescriptionPair);
							}
						}

						if (codeDescriptionPairList.Any())
						{
							var extractedCodeListProvider = new RevenueExtractedCodeListProvider(
								versionNumber,
								versionDate,
								codeListDetails.UpdateType,
								dataGrouping: applicationType.ToDataGrouping(),
								codeDescriptionPairList.Values.OrderBy(x => x.Code)
							);

							if (codeListDetails.AllowCombination && extractedCodeLists.TryGetValue((extractedCodeListProvider.DataGrouping, codeListDetails.Code), out var existingCodeList))
							{
								var combinedCodeLists = existingCodeList.CodeList.Union(extractedCodeListProvider.CodeList, new RevenueCodeListComparer()).OrderBy(x => x.Code);
								extractedCodeLists[(extractedCodeListProvider.DataGrouping, codeListDetails.Code)] = new RevenueExtractedCodeListProvider(
									versionNumber,
									versionDate,
									codeListDetails.UpdateType,
									dataGrouping: applicationType.ToDataGrouping(),
									combinedCodeLists
								);
							}
							else
							{
								extractedCodeLists.Add((extractedCodeListProvider.DataGrouping, codeListDetails.Code), extractedCodeListProvider);
							}
						}
					}
				}
			}
		}

		static internal string GetVersionNumber(PdfDocument pdfDocument)
		{
			var currentText = PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage(2), new SimpleTextExtractionStrategy());
			var versionNumbers = new Regex(@"\s+Version (\d+(?:\.\d+)+)\s+").Match(currentText);
			string result;
			if (versionNumbers.Success)
			{
				result = versionNumbers.Groups[1].Value;
			}
			else
			{
				throw new InvalidOperationException("Can not find the current version number on Page 2.");
			}
			return result;
		}

		static internal DateTime GetVersionDate(PdfReader pdfReader, PdfDocument pdfDocument, string versionNumber, out int lastTableOfContentsPageNumber)
		{
			DateTime result;
			var (contentFound, snippetText, lastPageNo) = GetPdfSnippet(pdfReader, pdfDocument, 2, "TABLE OF CONTENTS");
			if (!contentFound)
			{
				// "TABLE OF CONTENTS" is currently removed from the Revenue provided AIS file
				(contentFound, snippetText, lastPageNo) = GetPdfSnippet(pdfReader, pdfDocument, 2, @"\sINTRODUCTION[ ]{0,1}\.{5,}");
			}
			if (contentFound)
			{
				lastTableOfContentsPageNumber = lastPageNo;
				var versionDate = new Regex(versionNumber + @"\s+([0-9]{2}/[0-9]{2}/[0-9]{4})").Match(snippetText);
				if (versionDate.Success)
				{
					if (!DateTime.TryParseExact(versionDate.Groups[1].Value, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
					{
						throw new InvalidOperationException($"Can not parse the current version date for version {versionNumber}. Expected format dd/MM/yyyy");
					}
				}
				else
				{
					throw new InvalidOperationException($"Can not find the current version date for version {versionNumber}.");
				}
			}
			else
			{
				throw new InvalidOperationException("PDF structure has changed cannot location the TABLE OF CONTENTS starting on Page 2.");
			}
			return result;
		}

		internal void ExtractCodeListDetails(PdfReader pdfReader, PdfDocument pdfDocument, int startPageNo, Dictionary<(string DataGrouping, string Code), IRevenueExtractedCodeList> extractedCodeLists, string versionNumber, DateTime versionDate, ApplicationType applicationType)
		{
			var (tableContentReached, tableContentPdfSnippet, _) = GetPdfSnippet(pdfReader, pdfDocument, startPageNo, @"2. CODE LISTS\s*\n");
			if (!tableContentReached)
			{
				(tableContentReached, tableContentPdfSnippet, _) = GetPdfSnippet(pdfReader, pdfDocument, startPageNo, @"3. CODE LISTS\s*\n");
			}
			if (tableContentReached)
			{
				var pdfParserList = GetPdfPageNoList(tableContentPdfSnippet, applicationType);
				foreach (var pdfParserItem in pdfParserList)
				{
					var codeListCleanedUpPdfTxt = GetCodeListText(pdfReader, pdfDocument, pdfParserItem, applicationType, versionNumber);
					if (!string.IsNullOrEmpty(codeListCleanedUpPdfTxt))
					{
						var codeListDetails = pdfParserItem.CodeListDetails;
						var pattern = codeListDetails is IDoNotRegexEscapeTableTitleInPdf ? codeListDetails.TableTitleInFile : Regex.Escape(codeListDetails.TableTitleInFile + Environment.NewLine);
						var regex = new Regex(pattern);
						var match = regex.Match(codeListCleanedUpPdfTxt);
						if (match.Success)
						{
							var codeDescriptionPairList = new Dictionary<string, IRevenueCodeDescriptionPair>();

							var codeListPdfText = Regex.Replace(codeListCleanedUpPdfTxt.Substring(match.Index + match.Length), pattern, string.Empty);
							if (codeListDetails.Code == Constants.CommonCodeTypes.AdditionalProcedure && applicationType == ApplicationType.AIS)
							{
								codeListPdfText = CleanUpUnwantedSubHeadings(codeListPdfText, codeListDetails.CodeFormattingRegularExpression);
							}
							var regexLookahead = Environment.NewLine + "(?!" + codeListDetails.CodeFormattingRegularExpression + @"\s)(?<nextLine>(\S|\w)+)";
							codeListPdfText = Regex.Replace(codeListPdfText, regexLookahead, "${nextLine}", RegexOptions.ExplicitCapture);
							using (var reader = new StringReader(codeListPdfText))
							{
								string line;
								var additionalFilter = codeListDetails as IHaveAsyncAdditionalFilter;
								while ((line = reader.ReadLine()) != null)
								{
									var codeListItem = GetCodeListItem(line, codeListDetails.CodeFormattingRegularExpression, needsDescriptionCleanup: applicationType == ApplicationType.AIS);
									if (codeListItem != null && (additionalFilter == null || additionalFilter.Filter(codeListItem).GetAwaiter().GetResult()))
									{
										try
										{
											codeDescriptionPairList.Add(codeListItem.Code, codeListItem);
										}
										catch (ArgumentException)
										{
											// do nothing as we don't care about duplicate; we will use the first record
										}
									}
								}
							}

							bool requireCodeListAttributes = false;
							if (codeListDetails is ICodeListAttributeDetails codeListAttributeDetails)
							{
								requireCodeListAttributes = codeListAttributeDetails.IsCodeListAttributeNecessary;
							}

							if (codeDescriptionPairList.Any())
							{
								var extractedCodeListProvider = new RevenueExtractedCodeListProvider(
									versionNumber,
									versionDate,
									codeListDetails.UpdateType,
									dataGrouping: applicationType.ToDataGrouping(),
									codeDescriptionPairList.Values.OrderBy(x => x.Code),
									isCodeListAttributeNeeded: requireCodeListAttributes
								);

								if (codeListDetails.AllowCombination && extractedCodeLists.TryGetValue((extractedCodeListProvider.DataGrouping, codeListDetails.Code), out var value))
								{
									var combinedCodeLists = value.CodeList.Union(extractedCodeListProvider.CodeList, new RevenueCodeListComparer()).OrderBy(x => x.Code);
									extractedCodeLists[(extractedCodeListProvider.DataGrouping, codeListDetails.Code)] = new RevenueExtractedCodeListProvider(
										versionNumber,
										versionDate,
										codeListDetails.UpdateType,
										dataGrouping: applicationType.ToDataGrouping(),
										combinedCodeLists,
										isCodeListAttributeNeeded: requireCodeListAttributes
									);
								}
								else
								{
									extractedCodeLists.Add((extractedCodeListProvider.DataGrouping, codeListDetails.Code), extractedCodeListProvider);
								}
							}
						}
					}
				}
			}
			else
			{
				throw new InvalidOperationException(@"PDF structure has changed unable to locate the heading '2. CODE LISTS' or '3. CODE LISTS' starting after Page {startPageNo}.");
			}
		}

		static string CleanUpUnwantedSubHeadings(string codeListPdfText, string codeFormattingRegularExpression)
		{
			var regexMatchingPattern = @"^\s*(\(?\w+\)?[ \t]*){1,7}[:]\s*[A-F]xx\s*$";
			codeListPdfText = Regex.Replace(codeListPdfText, regexMatchingPattern, string.Empty, RegexOptions.Multiline);
			return codeListPdfText;
		}

		static (bool isFound, string snippetText, int lastProcessedPageNo) GetPdfSnippet(PdfReader pdfReader, PdfDocument pdfDocument, int startingPage, string contentToReach, bool allowEndOfDocument = false)
		{
			var contentIsReached = false;
			var pageNo = startingPage;
			var stringBuilder = new StringBuilder();

			var numberOfPages = pdfDocument.GetNumberOfPages();
			if (pdfReader != null && !string.IsNullOrEmpty(contentToReach) && startingPage <= numberOfPages)
			{
				var regex = new Regex(contentToReach);

				while (!contentIsReached && pageNo <= numberOfPages)
				{
					var page = pdfDocument.GetPage(pageNo);
					var pdfPageText = PdfTextExtractor.GetTextFromPage(page, new SimpleTextExtractionStrategy());

					var match = regex.Match(pdfPageText);
					if (match.Success || contentToReach.IndexOf(@"\(", StringComparison.InvariantCulture) > 10 && new Regex(contentToReach.Substring(0, contentToReach.IndexOf(@"\(", StringComparison.InvariantCulture))).Match(pdfPageText).Success)
					{
						contentIsReached = true;
					}

					stringBuilder.AppendLine(pdfPageText);

					pageNo++;
				}

				contentIsReached = contentIsReached || allowEndOfDocument;
			}

			var pdfSnippetText = stringBuilder.ToString();

			return (contentIsReached, pdfSnippetText, pageNo - 1);
		}

		IEnumerable<PdfPositionDetails> GetPdfPageNoList(string pdfText, ApplicationType applicationType)
		{
			var pdfParserList = new List<PdfPositionDetails>();

			if (CodeListsToExtract.TryGetValue(applicationType, out var codeLists))
			{
				foreach (var codeListDetails in codeLists)
				{
					if (string.IsNullOrEmpty(codeListDetails.NameInFile))
					{
						throw new InvalidOperationException($"The code list name should not be empty. Type: {codeListDetails.GetType()}.");
					}

					var safeCodeListName = Regex.Escape(codeListDetails.NameInFile);

					if (!codeListDetails.IsPublished)
					{
						// If unpublished, we don't know the full name. Match on code + any type of hyphen + any text 
						safeCodeListName = safeCodeListName + @"\s*[\p{Pd}]";
					}

					var codeListInTableContent = @"([1-9]{1}.[0-9]{1,2})\s*" + safeCodeListName + @"\s*([a-zA-z\s(),]*){0,1}[.]+\s*([0-9]{1,3})";
					var match = new Regex(codeListInTableContent, RegexOptions.IgnoreCase).Match(pdfText);
					if (match.Success)
					{
						var codeListIndexNumber = match.Groups[1].Value;

						if (int.TryParse(match.Groups[3].Value, out var codeListStartingPageNumber))
						{
							var codeListNextIndexNumber = GetNextCodeListNumber(codeListIndexNumber);

							var nextCodeListIndex = pdfText.IndexOf(codeListNextIndexNumber, StringComparison.CurrentCulture);
							if (nextCodeListIndex > -1)
							{
								var firstDotIndex = pdfText.IndexOf('.', nextCodeListIndex + codeListNextIndexNumber.Length);
								if (firstDotIndex > -1)
								{
									codeListNextIndexNumber = pdfText.Substring(nextCodeListIndex, firstDotIndex - nextCodeListIndex);
								}
							}
							pdfParserList.Add(new PdfPositionDetails(codeListDetails, codeListStartingPageNumber, codeListIndexNumber, codeListNextIndexNumber));
						}
					}
				}
			}
			return pdfParserList.OrderBy(item => item.PageNoInPdf);
		}

		static string GetNextCodeListNumber(string currentCodeListNo)
		{
			const string strRegex = "([1-9]{1}.)([0-9]{1,2})";
			var regex = new Regex(strRegex);
			var match = regex.Match(currentCodeListNo);
			if (match.Success)
			{
				var partOne = match.Groups[1].Value;
				var partTwo = match.Groups[2].Value;

				if (int.TryParse(partTwo, out var chapterCounter))
				{
					return partOne + (chapterCounter + 1);
				}

				throw new InvalidOperationException("Not correctly formatted code list chapter number.");
			}

			throw new InvalidOperationException("Not a correct code list chapter number.");
		}

		static string GetCodeListText(PdfReader pdfReader, PdfDocument pdfDocument, PdfPositionDetails pdfParserItem, ApplicationType applicationType, string versionNumber)
		{
			var result = string.Empty;

			if (pdfReader != null && pdfParserItem != null)
			{
				var (codeListContentReached, codeListContentPdfSnippet, _) = GetPdfSnippet(pdfReader, pdfDocument, pdfParserItem.PageNoInPdf, Regex.Escape(pdfParserItem.NextCodeListIndexNumberInPdf), true);
				if (codeListContentReached)
				{
					var startIndex = codeListContentPdfSnippet.IndexOf(pdfParserItem.CodeListIndexNumberInPdf + " " + pdfParserItem.CodeListDetails.NameInFile, StringComparison.CurrentCulture);
					if (startIndex >= 0)
					{
						codeListContentPdfSnippet = codeListContentPdfSnippet.Substring(startIndex);
					}

					var endIndex = codeListContentPdfSnippet.IndexOf(pdfParserItem.NextCodeListIndexNumberInPdf, StringComparison.CurrentCulture);
					if (endIndex < 0 && pdfParserItem.NextCodeListIndexNumberInPdf.IndexOf("(", StringComparison.InvariantCulture) > 10)
					{
						endIndex = codeListContentPdfSnippet.IndexOf(pdfParserItem.NextCodeListIndexNumberInPdf.Substring(0, pdfParserItem.NextCodeListIndexNumberInPdf.IndexOf("(", StringComparison.InvariantCulture)), StringComparison.CurrentCulture);
					}
					if (endIndex >= 0)
					{
						codeListContentPdfSnippet = codeListContentPdfSnippet.Substring(0, endIndex);
					}

					var pageHeaderRegex = $@"\s*{applicationType.ToPageHeaderText()}\s*([\w()]+\s)*Code\s*[lL]ists\s*Version\s*{versionNumber}\s*";
					codeListContentPdfSnippet = Regex.Replace(codeListContentPdfSnippet, @"^\s*©\s*Revenue\s*Commissioners\s*Page\s*[0-9]{1,3}\s*of\s*[0-9]{1,3}\s*$\n|\r", string.Empty, RegexOptions.Multiline);
					codeListContentPdfSnippet = Regex.Replace(codeListContentPdfSnippet, $@"^{pageHeaderRegex}$\n|\r", string.Empty, RegexOptions.Multiline);
					codeListContentPdfSnippet = Regex.Replace(codeListContentPdfSnippet, $@"{pageHeaderRegex}", string.Empty);
					codeListContentPdfSnippet = Regex.Replace(codeListContentPdfSnippet, @"^\s*$\n|\r", string.Empty, RegexOptions.Multiline).TrimEnd();
					codeListContentPdfSnippet = Regex.Replace(codeListContentPdfSnippet, @"\n", Environment.NewLine, RegexOptions.Multiline);

					result = codeListContentPdfSnippet;
				}
			}

			return result;
		}

		static IRevenueCodeDescriptionPair GetCodeListItem(string pdfLine, string formatting, bool needsDescriptionCleanup = false)
		{
			string code = null;
			string description = null;

			if (!string.IsNullOrEmpty(pdfLine))
			{
				var regex = new Regex("(?<code>" + formatting + @")\s(?<desc>.*)", RegexOptions.ExplicitCapture);
				var match = regex.Match(pdfLine);	
				if (match.Success)
				{
					if (match.Groups.Count > 1)
					{
						code = match.Groups[1].Value.TrimEnd();
					}

					if (match.Groups.Count > 2)
					{
						description = match.Groups[2].Value.Trim();
						if (needsDescriptionCleanup)
						{
							var unwantedLineRegex = @"\(\*\)\s*\w.+$";
							description = Regex.Replace(description, unwantedLineRegex, string.Empty).Trim();
						}
					}
				}
			}
			return (code == null && description == null) ? null : new RevenueCodeDescriptionPairProvider(code, description);
		}

		static FileType GetFileType(string url)
		{
			if (url.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
			{
				return FileType.PDF;
			}
			else if (url.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
			{
				return FileType.XLSX;
			}
			else
			{
				throw new InvalidOperationException("Unsupported file type.");
			}
		}
	}
}
