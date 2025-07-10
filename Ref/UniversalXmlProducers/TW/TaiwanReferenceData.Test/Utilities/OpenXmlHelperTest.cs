using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using static CargoWise.RefDbRepo.TaiwanReferenceData.OpenXmlHelper;

namespace CargoWise.RefDbRepo.TaiwanReferenceData.Test
{
	[TestFixture]
	public class OpenXmlHelperTest
	{
		[Test]
		public void TestExtractTable()
		{
			var unitsOfMeasurementPath = Path.Combine(FolderHelper.GetBinFolder(), "doc/RefCusCodeList/TWICI/TestInput/41-1基隆關及其分關查驗區.odt");
			var bytes = File.ReadAllBytes(unitsOfMeasurementPath);
			var document = OpenXmlHelper.GetDocument(bytes);
			var body = document.Root.Element(Namespaces.Office + "body").Elements().First();
			var tables = body.Descendants(Namespaces.Table + "table");
			var list = new List<(string code, string zone, string scope, string remark)>();
			foreach (var table in tables)
			{
				var rows = table.Descendants(Namespaces.Table + "table-row");
				foreach (var row in rows)
				{
					var cells = row.Descendants(Namespaces.Table + "table-cell");
					var code = cells.ElementAt(0).Elements(Namespaces.Text + "p").Last().Value;
					var zone = cells.ElementAt(1).Elements(Namespaces.Text + "p").Last().Value;
					var scope = cells.ElementAt(2).Elements(Namespaces.Text + "p").Last().Value;
					var remark = cells.ElementAt(3).Elements(Namespaces.Text + "p").Last().Value;
					list.Add((code, zone, scope, remark));
				}
			}
			Assert.AreEqual(58, list.Count);
			Assert.AreEqual("代 碼", list[0].code);
			Assert.AreEqual("基隆關進口組查驗二區", list[2].zone);
			Assert.AreEqual("西岸第27至33號碼頭", list[3].scope);
			Assert.AreEqual("修訂", list[3].remark);
		}
	}
}
