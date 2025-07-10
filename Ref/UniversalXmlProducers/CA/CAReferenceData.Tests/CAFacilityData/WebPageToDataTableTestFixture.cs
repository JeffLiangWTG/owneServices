using System.IO;
using CargoWise.RefDbRepo.CAReferenceData.Business.CAFacilityData;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests
{
	[TestFixture]
	public class WebPageToDataTableTestFixture
	{
		[Test]
		public void WebPageToDataTableFixture()
		{
			var dataXpath = @"//html[1]//body[1]//main[1]//table[1]//tbody[1]//tr";
			var headerXpath = @"//html[1]//body[1]//main[1]//table[1]//thead[1]//tr";
			using (var stream = TestHelper.GetTestInputFile("CustomsOfficeGenericSublocationCode.html"))
			using (var htmlStream = new StreamReader(stream))
			{
				var html = htmlStream.ReadToEnd();
				var webPageToDataTable = new WebpageTableToDataTable(html);
				var dt = webPageToDataTable.ExtractDatatableFromGrid(headerXpath, dataXpath);
				Assert.IsTrue(dt.Rows.Count == 210, $"210 rows expected but was {dt.Rows.Count}");
				Assert.IsTrue(dt.Columns[0].ColumnName == "Province", $"Column name expected: Province but was {dt.Columns[0].ColumnName}");
				Assert.IsTrue(dt.Columns[1].ColumnName == "Port", $"Column name expected: Port but was {dt.Columns[1].ColumnName}");
				Assert.IsTrue(dt.Columns[2].ColumnName == "Sublocation code", $"Column name expected: Sublocation code but was {dt.Columns[2].ColumnName}");
				Assert.IsTrue(dt.Columns[3].ColumnName == "Location", $"Column name expected: Location but was {dt.Columns[3].ColumnName}");
				Assert.IsTrue(dt.Rows[0]["Province"].ToString() == "Alberta", $"Expected value for row 1 column 1: Alberta but was {dt.Rows[0]["Province"].ToString()}");
				Assert.IsTrue(dt.Rows[0]["Port"].ToString() == "0701", $"Expected value for row 1 column 2: 0701 but was {dt.Rows[0]["Port"].ToString()}");
				Assert.IsTrue(dt.Rows[0]["Sublocation code"].ToString() == "9701", $"Expected value for row 1 column 3: 9701 but was {dt.Rows[0]["Sublocation code"].ToString()}");
				Assert.IsTrue(dt.Rows[0]["Location"].ToString() == "Calgary", $"Expected value for row 1 column 4: Calgary but was {dt.Rows[0]["Location"].ToString()}");
			}
		}
	}
}
