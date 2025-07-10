using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using FlexCel.XlsAdapter;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.USSIMDataPopulator
{
	public static class FileParser
	{
		public static List<string> ParsePDF(string fileName)
		{
			var codes = new List<string>();
			using (var pdfReader = new PdfReader(fileName))
			{
				var search = @"\b([A-Z]{3})\b";
				using (var pdfDocument = new PdfDocument(pdfReader))
				{
					for (var i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
					{
						var page = pdfDocument.GetPage(i);
						ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
						var currentText = PdfTextExtractor.GetTextFromPage(page, strategy);
						var matches = Regex.Matches(currentText, search);

						codes.AddRange(matches.Cast<Match>().Select(match => match.Value).ToList());
					}
				}
			}
			return codes;
		}

		public static List<RefCusCodeList> ParseXLS(string fileName, List<string> mandatoryDataCodes, DateTime publishedDate)
		{
			Argument.NotNull(fileName, nameof(fileName));
			Argument.NotNull(mandatoryDataCodes, nameof(mandatoryDataCodes));

			var xls = new XlsFile(fileName, false);
			var cusCodeLists = new List<RefCusCodeList>();

			for (var row = 2; row <= xls.RowCount; row++)
			{
				var code = xls.GetCellValue(row, 3)?.ToString();
				var scName = xls.GetCellValue(row, 4)?.ToString();
				var enName = xls.GetCellValue(row, 5)?.ToString();

				if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(scName))
				{
					var description = !string.IsNullOrEmpty(enName) ? $"{scName}/{enName}" : scName;
					var cusCodeList = new RefCusCodeList()
					{
						ZZD_Code = code,
						ZZD_Description = description,
						ZZD_StartDate = publishedDate
					};
					if (mandatoryDataCodes.Contains(code))
					{
						var cusCodeListAttributes = new List<RefCusCodeListAttribute>();
						var cusCodeListAttribute = new RefCusCodeListAttribute()
						{
							ZZE_ZXE_NKName = "RequiresFullData",
							ZZE_Value = "Yes"
						};
						cusCodeListAttributes.Add(cusCodeListAttribute);
						cusCodeList.RefCusCodeListAttributes = cusCodeListAttributes.ToArray();
					}
					cusCodeLists.Add(cusCodeList);
				}
			}
			return cusCodeLists;
		}
	}
}
