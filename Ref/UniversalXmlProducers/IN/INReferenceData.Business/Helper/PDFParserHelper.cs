using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using Spire.Pdf;
using Spire.Pdf.Graphics;
using Spire.Pdf.Texts;
using Spire.Pdf.Utilities;

namespace CargoWise.RefDbRepo.INReferenceData.Business
{
	public static class PDFParserHelper
	{
		#region ExtractTableToDataTable
		public static DataTable ExtractTableToDataTable(byte[] data)
		{
			var dataTable = new DataTable();

			using (var doc = new Spire.Pdf.PdfDocument())
			{
				doc.LoadFromBytes(data);
				using (var extractor = new PdfTableExtractor(doc))
				{
					for (int pageIndex = 0; pageIndex < doc.Pages.Count; pageIndex++)
					{
						var tableList = extractor.ExtractTable(pageIndex) ?? ExtractTableBySavingAndLoadingCurrentPage(doc.Pages[pageIndex]);

						if (tableList != null)
						{
							AddPdfTableToDataTable(tableList, dataTable);
						}
					}
				}
			}
			return dataTable;
		}

		static PdfTable[] ExtractTableBySavingAndLoadingCurrentPage(PdfPageBase currentPage)
		{
			var result = new PdfTable[] { };
			using (var pdf = new Spire.Pdf.PdfDocument())
			{
				var page = pdf.Pages.Add(currentPage.Size, new PdfMargins(0));
				currentPage.CreateTemplate().Draw(page, new PointF(0, 0));
				using (var stream = new MemoryStream())
				{
					pdf.SaveToStream(stream);
					using (var doc = new Spire.Pdf.PdfDocument())
					{
						doc.LoadFromStream(stream);
						using (var extractor = new PdfTableExtractor(doc))
						{
							result = extractor.ExtractTable(0);
							return result;
						}
					}
				}
			}
		}

		static void AddPdfTableToDataTable(PdfTable[] tableList, DataTable dataTable)
		{
			foreach (var table in tableList)
			{
				var column = table.GetColumnCount();

				while (dataTable.Columns.Count < column)
				{
					dataTable.Columns.Add();
				}

				var row = table.GetRowCount();
				for (var i = 0; i < row; i++)
				{
					var dataRow = dataTable.Rows.Add();
					for (var j = 0; j < column; j++)
					{
						dataRow[j] = table.GetText(i, j);
					}
				}
			}
		}
		#endregion

		#region ExtractTextFromPdfUseSpire

		public static string ExtractTextFromPdfUseSpire(byte[] data, int startingPageNumber = 1, int pageCountNeedToExtract = 0)
		{
			var text = new StringBuilder();

			using (var doc = new Spire.Pdf.PdfDocument())
			{
				doc.LoadFromBytes(data);
				var pageCount = doc.Pages.Count;
				if (startingPageNumber > pageCount)
				{
					throw new ArgumentOutOfRangeException(nameof(startingPageNumber), $"The number of starting pages is greater than the total number of pages. StartingPageNumber: {startingPageNumber}. Total number of pages: {pageCount}");
				}
				var startingPageNumberIndex = startingPageNumber - 1;
				var endPageNumber = pageCount;
				if (pageCountNeedToExtract != 0 && startingPageNumberIndex + pageCountNeedToExtract <= pageCount)
				{
					endPageNumber = startingPageNumberIndex + pageCountNeedToExtract;
				}

				for (int pageIndex = startingPageNumberIndex; pageIndex < endPageNumber; pageIndex++)
				{
					var extractor = new PdfTextExtractor(doc.Pages[pageIndex]);
					var pdfTextExtractOptions = new PdfTextExtractOptions()
					{
						IsSimpleExtraction = true,
						IsExtractAllText = false,
						IsShowHiddenText = false
					};
					var textContent = extractor.ExtractText(pdfTextExtractOptions);
					if (!string.IsNullOrEmpty(textContent))
					{
						text.AppendLine(textContent);
					}
				}
			}
			return text.ToString();
		}

		#endregion
	}
}
