using System.Data;
using System.Drawing;
using System.IO;
using iText.Kernel.Pdf;
using Spire.Pdf;
using Spire.Pdf.Graphics;
using Spire.Pdf.Utilities;

namespace CargoWise.RefDbRepo.USReferenceData.Business
{
	public static class PDFParserHelper
	{
		public static void SplitAndSaveInterval(string exportFilePath, string pdfFilePath, string pdfPrefix, int startPage, int endPage)
		{
			using (var reader = new PdfReader(pdfFilePath))
			using (var document = new iText.Kernel.Pdf.PdfDocument(reader))
			{
				var numberOfPages = document.GetNumberOfPages();
				var fileName = Path.Combine(exportFilePath, pdfPrefix + startPage + ".pdf");
				using(var writer = new PdfWriter(fileName))
				using (var copy = new iText.Kernel.Pdf.PdfDocument(writer))
				{
					document.CopyPagesTo(System.Math.Max(startPage,1), System.Math.Min(endPage - 1, numberOfPages), copy);
				}
			}
		}

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
	}
}
