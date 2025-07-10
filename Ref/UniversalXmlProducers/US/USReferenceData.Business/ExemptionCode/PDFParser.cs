using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using iText.Kernel.Pdf;

namespace CargoWise.RefDbRepo.USReferenceData.Business.ExemptionCode
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
		List<RefCusCodeList> ExemptionCodeList = new List<RefCusCodeList>();
		static string XMLWriterDataSource => "US DDTC ITAR Exemption Codes";
		static string OutPutFileName => "Exemption_Codes.xml";

		public string ReadPDFAndExportXML()
		{
			ReadAndParsePDF();
			if (ExemptionCodeList.Any())
			{
				var xmlWriterConfiguration = XmlWriterHelper.GetRefCusCodeListWriterConfiguration(Constants.CodeType.ITAR, new DateTime(2000, 01, 01, 0, 0, 0), false);
				XmlWriterHelper.ExportToXMLFile(XMLWriterDataSource, System.IO.Path.Combine(exportFilePath, OutPutFileName), xmlWriterConfiguration, publishDate, ExemptionCodeList);

				return $"Processed {ExemptionCodeList.Count} Exemption Codes.";
			}
			return "No Exemption Number data defined in the file.";
		}

		void ReadAndParsePDF()
		{
			var startPage = 2;

			if (!Directory.Exists(exportFilePath))
			{
				Directory.CreateDirectory(exportFilePath);
			}

			using (var openFile = new PdfReader(filePath))
			using (var document = new PdfDocument(openFile))
			{
				var endPage = document.GetNumberOfPages();
				try
				{
					for (var idx = startPage; idx <= endPage; idx += 10)
					{
						PDFParserHelper.SplitAndSaveInterval(exportFilePath, filePath, "ExemptionNumbertemp_", idx, idx + 10);
					}
				}
				catch (Exception ex)
				{
					throw new InvalidOperationException($"Can not split a downloaded Document(NumberOfPages: {endPage}). " + ex.ToString());
				}

				var dir = new DirectoryInfo(exportFilePath);
				var files = dir.GetFiles("ExemptionNumbertemp_*");
				foreach (var file in files)
				{
					try
					{
						ExtractTable(file.FullName);
					}
					catch (Exception ex)
					{
						throw new InvalidOperationException($"Unable to extract table from {file.FullName}. " + ex.ToString());
					}
					finally
					{
						file.Delete();
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
					var exemptionNumber = GetColumnText(row[0]?.ToString() ?? string.Empty).Replace(" ", "");
					if (!string.IsNullOrEmpty(exemptionNumber)
						&& !exemptionNumber.StartsWith("DDTCITAR", StringComparison.CurrentCultureIgnoreCase)
						&& !exemptionNumber.StartsWith("Pub#", StringComparison.CurrentCultureIgnoreCase))
					{
						if (Regex.IsMatch(exemptionNumber, @"^\d{3}\.(\d+)([A-Z]?\d*)?$"))
						{
							ExemptionCodeList.Add(new RefCusCodeList()
							{
								ZZD_Code = exemptionNumber,
								ZZD_Description = GetColumnText(row[1]?.ToString() ?? string.Empty)
							});
						}
						else
						{
							throw new InvalidOperationException($"Can not add a valid Exemption Code({exemptionNumber}) from pdf file.");
						}
					}
				}
			}
		}

		static string GetColumnText(string text) => Encoding.UTF8.GetString(Encoding.Default.GetBytes(text)).Trim().Replace("\r\n", "").Replace("\n", "");
	}
}
