using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.CAReferenceData.Business.CASurtax;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests.CASurtaxData
{
	[TestFixture]
	class WebSurTaxParserFixture
	{
		[Test]
		public void Parse()
		{
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(html)))
			{
				var results = WebSurTaxParser.Parse(stream).ToArray();
				Assert.AreEqual(2, results[0].Classifications.Length);
				Assert.True(results[0].Table.Contains("Table 1"));
				Assert.AreEqual("7206.10.00", results[0].Classifications[0]);
				Assert.AreEqual("7206.90.00", results[0].Classifications[1]);
				Assert.AreEqual(1, results[1].Classifications.Length);
				Assert.True(results[1].Table.Contains("Table 3"));
				Assert.AreEqual("0403.10.10", results[1].Classifications[0]);
			}
		}

		const string html = @"
<html>
<div id=""mainContent"" class=""inner"">
<article>
<table class=""table table-bordered"">
	<caption class=""alignLeft"">
	Table 1 – Steel Products
</caption>
	
  <tbody><tr class=""rh"">
	<th class=""alignLeft""><strong>Tariff Item</strong></th>
	<th class=""alignLeft""><strong>Description</strong></th>
  </tr>
  <tr class=""r1"">
	<td>7206.10.00</td>
	<td>Ingots</td>
  </tr>
  <tr class=""r2"">
	<td>7206.90.00</td>
	<td>Other primary forms</td>
  </tr>
</tbody></table>

<table class=""table table-bordered"">
<caption class=""alignLeft"">Table 3 – Other products</caption>
  <tbody><tr class=""rh"">
	<th class=""alignLeft"">Tariff Item</th>
	<th class=""alignLeft"">Description</th>
  </tr>
  <tr class=""r1"">
	<td>0403.10.10</td>
	<td>Yogourt: Within access commitment</td>
  </tr>
</tbody></table>
</article>
</div>
</html>
";
	}
}
