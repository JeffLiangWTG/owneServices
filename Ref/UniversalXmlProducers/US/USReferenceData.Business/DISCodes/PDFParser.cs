using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace CargoWise.RefDbRepo.USReferenceData.Business.DISCodes
{
	public class PDFParser
	{
		public PDFParser(string filePath, string exportFilePath, DateTime publishDate)
		{
			this.filePath = filePath;
			this.exportFilePath = exportFilePath;
			this.publishDate = publishDate;
		}
		readonly string filePath;
		readonly string exportFilePath;
		readonly DateTime publishDate;
		List<DISCode> DISCodeList = new List<DISCode>();
		List<RefCusCodeList> refCusCodeLists = new List<RefCusCodeList>();

		public string ReadPDFAndExportXML()
		{
			ReadAndParsePDF();

			if (DISCodeList.Any())
			{
				PopulateRefCusCodeList();

				var xmlWriterConfiguration = XmlWriterHelper.GetRefCusCodeListWriterConfiguration(Constants.CodeType.USDDC, new DateTime(2000, 01, 01, 0, 0, 0));
				XmlWriterHelper.ExportToXMLFile(XMLWriterDataSource, System.IO.Path.Combine(exportFilePath, OutPutFileName), xmlWriterConfiguration, publishDate, refCusCodeLists);

				return $"Processed {DISCodeList.Count} DIS codes.";
			}
			return "No DIS code data defined in the file.";
		}

		static string XMLWriterDataSource => "US DIS Code";
		static string OutPutFileName => "DIS_Codes.xml";

		void ReadAndParsePDF()
		{
			var startPage = 1;
			var endPage = 1;

			if (!Directory.Exists(exportFilePath))
			{
				Directory.CreateDirectory(exportFilePath);
			}

			using (var openFile = new PdfReader(filePath))
			using (var document = new iText.Kernel.Pdf.PdfDocument(openFile))
			{
				var lastPage = document.GetNumberOfPages();

				for (var pageNum = lastPage > 20 ? 20 : 1; pageNum <= lastPage; pageNum++)
				{
					var contentOnePage = PdfTextExtractor.GetTextFromPage(document.GetPage(pageNum), new SimpleTextExtractionStrategy());
					var allString = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(contentOnePage)));
					if (allString.Contains("APPENDIX A: List of Supported Documents", StringComparison.OrdinalIgnoreCase))
					{
						startPage = pageNum;
					}
					else if (allString.Contains("APPENDIX B: General Guidelines for Documents Submitted", StringComparison.OrdinalIgnoreCase))
					{
						endPage = pageNum;
						break;
					}
				}

				if (startPage == 1 || endPage <= startPage)
				{
					throw new InvalidOperationException("Can not find a valid Table for Document Code.");
				}

				try
				{
					for (int idx = startPage; idx <= endPage; idx += 10)
					{
						PDFParserHelper.SplitAndSaveInterval(exportFilePath, filePath, "DISTemp_", idx, idx + 10);
					}
				}
				catch (Exception ex)
				{
					throw new InvalidOperationException("Can not split a downloaded Document. " + ex.ToString());
				}

				DirectoryInfo dir = new DirectoryInfo(exportFilePath);
				FileInfo[] Files = dir.GetFiles("DISTemp_*");
				try
				{
					foreach (FileInfo fileName in Files)
					{
						ExtractTable(fileName.FullName);
					}
				}
				finally
				{
					foreach (var item in Files)
					{
						item.Delete();
					}
				}
			}
		}

		void ExtractTable(string fileName)
		{
			var bytes = File.ReadAllBytes(fileName);
			using (var dataTable = PDFParserHelper.ExtractTableToDataTable(bytes))
			{
				foreach (DataRow row in dataTable.Rows)
				{
					var disCode = new DISCode();
					for (int columnNum = 0; columnNum < dataTable.Columns.Count; columnNum++)
					{
						var text = Encoding.UTF8.GetString(Encoding.Default.GetBytes(row[columnNum]?.ToString() ?? string.Empty)).Trim();
						text = text.Replace("\r\n", "").Replace("\n", "");

						switch (columnNum)
						{
							case 0:
								disCode.AgencyCode = text;
								break;
							case 1:
								disCode.DocumentDescription = text;
								break;
							case 2:
								disCode.DocumentType = text;
								break;
							case 3:
								disCode.DocumentLabelCode = text;
								break;
							case 4:
								disCode.DocCode = text;
								break;
							case 5:
								disCode.Metadata = text;
								break;
						}
					}
					if (!string.IsNullOrEmpty(disCode.DocCode) && !disCode.DocCode.StartsWith("DocCode", StringComparison.CurrentCultureIgnoreCase))
					{
						if (Regex.IsMatch(disCode.DocCode, @"^[A-Z]{3}\d{2,3}"))
						{
							DISCodeList.Add(disCode);
						}
						else
						{
							throw new InvalidOperationException("Can not add a valid Document Code from pdf file.");
						}
					}
				}
			}
		}

		void PopulateRefCusCodeList()
		{
			foreach (var disCode in DISCodeList)
			{
				var documentDescription = disCode.DocumentDescription;
				var metaData = disCode.Metadata;
				var description = documentDescription != null && !string.IsNullOrEmpty(documentDescription.Trim()) ? documentDescription.Trim() :
					(metaData != null && !string.IsNullOrEmpty(metaData.Trim()) ? metaData.Trim() : string.Empty);
				if (!string.IsNullOrEmpty(description))
				{
					var refCusCodeList = new RefCusCodeList()
					{
						ZZD_Code = disCode.DocCode.Trim(),
						ZZD_Description = description
					};

					if (metaData != null && metaData.ToUpper(CultureInfo.InvariantCulture).Contains("REMOVED"))
					{
						refCusCodeList.ZZD_EndDate = new DateTime(2000, 01, 01);
					}
					else
					{
						refCusCodeList.RefCusCodeListAttributes = ParserHelper.GetAttributes(disCode);
					}

					refCusCodeLists.Add(refCusCodeList);
				}
			}
		}
	}

}
