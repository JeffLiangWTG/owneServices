using System.Data;
using System.Drawing;
using System.IO;
using Spire.Pdf;
using Spire.Pdf.Graphics;
using Spire.Pdf.Utilities;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public static class PdfHelper
	{
		public static DataTable ExtractTableToDataTable(byte[] data)
		{
			var dataTable = new DataTable();

			var doc = new PdfDocument();
			doc.LoadFromBytes(data);
			var extractor = new PdfTableExtractor(doc);

			for (int pageIndex = 0; pageIndex < doc.Pages.Count; pageIndex++)
			{
				var tableList = extractor.ExtractTable(pageIndex) ?? ExtractTableBySavingAndLoadingCurrentPage(doc.Pages[pageIndex]);
				if (tableList != null)
				{
					AddPdfTableToDataTable(tableList, dataTable);
				}
			}

			return dataTable;
		}

		static PdfTable[] ExtractTableBySavingAndLoadingCurrentPage(PdfPageBase currentPage)
		{
			var pdf = new PdfDocument();
			var page = pdf.Pages.Add(currentPage.Size, new PdfMargins(0));
			currentPage.CreateTemplate().Draw(page, new PointF(0, 0));
			using (var stream = new MemoryStream())
			{
				pdf.SaveToStream(stream);
				var doc = new PdfDocument();
				doc.LoadFromStream(stream);
				var extractor = new PdfTableExtractor(doc);
				return extractor.ExtractTable(0);
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
