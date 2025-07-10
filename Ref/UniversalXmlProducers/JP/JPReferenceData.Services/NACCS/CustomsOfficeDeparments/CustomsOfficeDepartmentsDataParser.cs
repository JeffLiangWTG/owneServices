using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Spire.Pdf;
using Spire.Pdf.Utilities;
using Path = System.IO.Path;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public class CustomsOfficeDepartmentsDataParser
	{
		public CustomsOfficeDepartmentsDataParser(string[] downloadUrls)
		{
			this.downloadUrls = downloadUrls;
			codeRegex = new Regex(codeRegexPattern);
			codeDescriptionDictionary = new Dictionary<string, CustomsOfficeDepartmentsDescription>();
		}

		readonly Dictionary<string, CustomsOfficeDepartmentsDescription> codeDescriptionDictionary;
		readonly string[] downloadUrls;
		readonly Regex codeRegex;
		const string codeRegexPattern = @"[0-9A-Z]{2}";

		public List<RefCusCodeList> Parse(IHttpClientHelper httpClientHelper, out DateTime publishDateTime)
		{
			var result = new List<RefCusCodeList>();
			publishDateTime = DateTime.MinValue;

			foreach (string url in downloadUrls)
			{
				var downloadFilePath = Path.GetTempFileName();

				try
				{
					var successfullyDownloaded = FileDownloader.TryDownload(httpClientHelper, url, downloadFilePath).GetAwaiter().GetResult();
					if (!successfullyDownloaded)
					{
						throw new UnhandledApplicationException(string.Format(CultureInfo.InvariantCulture, "Fail to download file from {0}.", url));
					}

					var downloadAbsolutePath = Path.GetFullPath(downloadFilePath);
					using (var pdfDocument = new PdfDocument(downloadAbsolutePath))
					{
						pdfDocument.LoadFromFile(downloadAbsolutePath);

						publishDateTime = GetLatestPublishDateTime(publishDateTime, pdfDocument);

						using (var extractor = new PdfTableExtractor(pdfDocument))
						{
							var cachedPortDescription = string.Empty;
							var cachedOfficeCode = string.Empty;
							var cachedOfficeDescription = string.Empty;
							var cachedDepartmentCodes = new[] { string.Empty };
							var cachedDepartmentDescription = string.Empty;

							var pageCount = pdfDocument.Pages.Count;
							if (pageCount > 9)
							{
								throw new UnhandledApplicationException("There are more than 9 pages in PDF file, and this will cause failure of Spire.Pdf.");
							}

							for (var pageIndex = 0; pageIndex < pageCount; pageIndex++)
							{
								var table = ExtractAndValidateTable(extractor, pageIndex, url, out var rowCount);

								var columnIndexDic = GetColumnIndexDic(table);
								var portDescriptionIndex = columnIndexDic["税関"];
								var officeDescriptionIndex = columnIndexDic["官署名"];
								var officeCodeIndex = columnIndexDic["官署コード"];
								var departmentDescriptionIndex = columnIndexDic["部門名"];
								var departmentCodeIndex = columnIndexDic["部門コード"];

								for (var rowIndex = 1; rowIndex < rowCount; rowIndex++)
								{
									cachedPortDescription = UpdateCachedValue(cachedPortDescription, table, rowIndex, portDescriptionIndex, true, false);
									cachedOfficeDescription = UpdateCachedValue(cachedOfficeDescription, table, rowIndex, officeDescriptionIndex, true, false);
									cachedDepartmentDescription = UpdateCachedValue(cachedDepartmentDescription, table, rowIndex, departmentDescriptionIndex, true, false);

									cachedOfficeCode = UpdateCachedValue(cachedOfficeCode, table, rowIndex, officeCodeIndex, false, true);
									cachedDepartmentCodes = UpdateCachedMultipleValue(cachedDepartmentCodes, table, rowIndex, departmentCodeIndex);

									foreach (var cachedDepartmentCode in cachedDepartmentCodes)
									{
										var code = cachedOfficeCode + cachedDepartmentCode;
										AddToDictionary(code, cachedPortDescription, cachedOfficeDescription, cachedDepartmentDescription);
									}
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					throw new UnhandledApplicationException($"Unable to load JP custom office departments from the following URL: {url} /r/n {ex.Message}");
				}
				finally
				{
					File.Delete(downloadFilePath);
				}
			}

			CreateAndAddRefCusCodeList(result);
			return result;
		}

		static DateTime GetLatestPublishDateTime(DateTime storedValue, PdfDocument pdfDocument)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			return pdfDocument.DocumentInformation.ModificationDate > storedValue ? pdfDocument.DocumentInformation.ModificationDate : storedValue;
#pragma warning restore CS0618 // Type or member is obsolete
		}

		static Dictionary<string, int> GetColumnIndexDic(PdfTable table)
		{
			return new Dictionary<string, int>
			{
				{ table.GetText(0, 0).Trim(), 0 },
				{ table.GetText(0, 1).Trim(), 1 },
				{ table.GetText(0, 2).Trim(), 2 },
				{ table.GetText(0, 3).Trim(), 3 },
				{ table.GetText(0, 4).Trim(), 4 }
			};
		}

		void CreateAndAddRefCusCodeList(List<RefCusCodeList> result)
		{
			foreach (var code in codeDescriptionDictionary.Keys)
			{
				result.Add(new RefCusCodeList
				{
					ZZD_Code = code,
					ZZD_Description = codeDescriptionDictionary[code].GetFullDescription()
				});
			}
		}

		void AddToDictionary(string code, string portDescription, string officeDescription, string departmentDescription)
		{
			if (codeDescriptionDictionary.TryGetValue(code, out var customsOfficeDepartmentsDescription))
			{
				if (!customsOfficeDepartmentsDescription.AddDepartmentDescription(portDescription, officeDescription, departmentDescription))
				{
					throw new UnhandledApplicationException("Unexpected format: Records with same code should have same port description and office description.");
				}
			}
			else
			{
				var customsOfficeDepartmentsDescriptionCollection = new CustomsOfficeDepartmentsDescription(portDescription, officeDescription, departmentDescription);
				codeDescriptionDictionary.Add(code, customsOfficeDepartmentsDescriptionCollection);
			}
		}

		string[] UpdateCachedMultipleValue(string[] cachedString, PdfTable pdfTable, int rowIndex, int columnIndex)
		{
			var textFromTable = pdfTable.GetText(rowIndex, columnIndex).Trim();
			if (string.IsNullOrWhiteSpace(textFromTable))
			{
				return cachedString;
			}
			else
			{
				return ValidateAndTranslateCode(textFromTable);
			}
		}

		string[] ValidateAndTranslateCode(string codeString)
		{
			var halfWidthString = JapaneseLocalHelper.ConvertFullWidthToHalfWidth(codeString);
			var codes = halfWidthString.Split('、');
			for (var i = 0; i < codes.Length; i++)
			{
				codes[i] = codes[i].Replace('o', '0').Replace('O', '0');
				if (!codeRegex.IsMatch(codes[i]))
				{
					throw new UnhandledApplicationException($"Unexpected code format. The code string is: {codeString}");
				}
			}

			return codes;
		}

		string UpdateCachedValue(string cachedString, PdfTable pdfTable, int rowIndex, int columnIndex, bool shouldTreatAsZero, bool isCode)
		{
			var textFromTable = pdfTable.GetText(rowIndex, columnIndex).Trim();
			textFromTable = textFromTable.Replace("\n", "").Replace("　", "").Replace(" ", "");
			if (string.IsNullOrWhiteSpace(textFromTable))
			{
				return cachedString;
			}
			else
			{
				return ValidateAndCorrect(textFromTable, shouldTreatAsZero, isCode);
			}
		}

		string ValidateAndCorrect(string descriptionString, bool shouldTreatAsZero, bool isCode)
		{
			var halfWidthString = JapaneseLocalHelper.ConvertFullWidthToHalfWidth(descriptionString);
			halfWidthString = halfWidthString.Replace("\n", "");

			if (shouldTreatAsZero)
			{
				halfWidthString =  halfWidthString.Replace('o', '0').Replace('O', '0');
			}

			if (isCode && !codeRegex.IsMatch(halfWidthString))
			{
				throw new UnhandledApplicationException(string.Format(CultureInfo.InvariantCulture, "Unexpected code format when parse code from: {0}.", descriptionString));
			}

			return halfWidthString;
		}

		static PdfTable ExtractAndValidateTable(PdfTableExtractor pdfTableExtractor, int pageIndex, string url, out int rowCount)
		{
			var tables = pdfTableExtractor.ExtractTable(pageIndex);
			if (tables == null || tables.Length != 1)
			{
				throw new UnhandledApplicationException($"Unexpected format of PDF from {url}. Expect that there is one table in one page.");
			}

			var table = tables.First();
			rowCount = table.GetRowCount();
			if (rowCount <= 1)
			{
				throw new UnhandledApplicationException($"Unexpected format of PDF from {url}. Expected data is not exist in table.");
			}

			return table;
		}

		sealed class CustomsOfficeDepartmentsDescription
		{
			string PortDescription { get; }
			string OfficeDescription { get; }
			string DepartmentDescription { get; set; }

			public CustomsOfficeDepartmentsDescription(string portDescription, string officeDescription, string departmentDescription)
			{
				PortDescription = portDescription;
				OfficeDescription = officeDescription;
				DepartmentDescription = departmentDescription;
			}

			public bool AddDepartmentDescription(string portDescription, string officeDescription, string departmentDescription)
			{
				if (PortDescription.Equals(portDescription, StringComparison.Ordinal) && OfficeDescription.Equals(officeDescription, StringComparison.Ordinal))
				{
					DepartmentDescription = DepartmentDescription + "," + departmentDescription;
					return true;
				}
				else
				{
					return false;
				}
			}

			public string GetFullDescription()
			{
				return PortDescription + "*" + OfficeDescription + "*" + DepartmentDescription;
			}
		}
	}
}
