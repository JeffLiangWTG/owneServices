using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	abstract class WhsPickingSecureServiceTestCase : WhsSecureServiceTestCase
	{
		#region Asserts

		protected void AssertPickLines(IEnumerable<WhsPickLine> expectedPickLines, WhsPickLineInfoCollection lines)
		{
			AssertPickLines("", new WhsPickLineInfoCollection(expectedPickLines, new WhsPickInfo()), lines);
		}

		protected void AssertPickLines(WhsPickLineInfoCollection expectedPickLines, WhsPickLineInfoCollection lines)
		{
			AssertPickLines("", expectedPickLines, lines);
		}

		protected void AssertPickLines(string message, IEnumerable<WhsPickLine> expectedPickLines, WhsPickLineInfoCollection lines)
		{
			AssertPickLines(message, new WhsPickLineInfoCollection(expectedPickLines, new WhsPickInfo()), lines);
		}

		protected void AssertPickLines(string message, WhsPickLineInfoCollection expectedPickLines, WhsPickLineInfoCollection lines)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("Line Count", expectedPickLines.Count, lines.Count);
				foreach (var expectedLine in expectedPickLines)
				{
					var lineFound = false;
					foreach (var line in lines)
					{
						if (line.PKs.Contains(expectedLine.PKs[0]))
						{
							lineFound = true;
							AssertPickLine(expectedLine, line);
							break;
						}
					}
					AssertEquals("Expected Pick Line not in the list, PKs: " + string.Join(", ", expectedLine.PKs), true, lineFound);
				}
			});
		}

		protected void AssertPickLine(WhsPickLineInfo expectedPickLine, WhsPickLineInfo line)
		{
			AssertContainsExactElementsInAnyOrder("Pick Line PK", expectedPickLine.PKs, line.PKs);
			AssertEquals("Pick Line ExpiryDate", expectedPickLine.ExpiryDate, line.ExpiryDate);
			AssertEquals("Pick Line ExpiryDate Kind", DateTimeKind.Unspecified, line.ExpiryDate.Kind);
			AssertEquals("Pick Line Attribute1", expectedPickLine.Attribute1, line.Attribute1);
			AssertEquals("Pick Line Attribute1Caption", expectedPickLine.PartAttributes.Attribute1Caption, line.PartAttributes.Attribute1Caption);
			AssertEquals("Pick Line Attribute2", expectedPickLine.Attribute2, line.Attribute2);
			AssertEquals("Pick Line Attribute2Caption", expectedPickLine.PartAttributes.Attribute2Caption, line.PartAttributes.Attribute2Caption);
			AssertEquals("Pick Line Attribute3", expectedPickLine.Attribute3, line.Attribute3);
			AssertEquals("Pick Line Attribute3Caption", expectedPickLine.PartAttributes.Attribute3Caption, line.PartAttributes.Attribute3Caption);
			AssertEquals("Pick Line Location", expectedPickLine.Location, line.Location);
			AssertEquals("Pick Line LocationBarcode", expectedPickLine.LocationBarcode, line.LocationBarcode);
			AssertEquals("Pick Line PackingDate", expectedPickLine.PackingDate, line.PackingDate);
			AssertEquals("Pick Line PackingDate Kind", DateTimeKind.Unspecified, line.PackingDate.Kind);
			AssertEquals("Pick Line PalletID", expectedPickLine.PalletID, line.PalletID);
			AssertEquals("Pick Line Units", expectedPickLine.Units, line.Units);
			AssertEquals("Pick Line UnitsUQ", expectedPickLine.UnitsUQ, line.UnitsUQ);
			AssertEquals("Pick Line ProductPK", expectedPickLine.ProductPK, line.ProductPK);
		}

		protected void AssertProductUnitRates(WhsUnitRateInfoCollection expectedUnitRates, WhsUnitRateInfoCollection unitRates)
		{
			AssertEquals(expectedUnitRates.Count, unitRates.Count);
			foreach (WhsUnitRateInfo expectedUnitRate in expectedUnitRates)
			{
				bool rateFound = false;
				foreach (WhsUnitRateInfo unitRate in unitRates)
				{
					if (expectedUnitRate.Package == unitRate.Package && expectedUnitRate.Parent == unitRate.Parent)
					{
						rateFound = true;
						AssertEquals("Product Unit Conversion rate, Package: " + expectedUnitRate.Package + ", Parent: " + expectedUnitRate.Parent, expectedUnitRate.Units, unitRate.Units);
						break;
					}
				}
				Assert("Product Unit Rate is not in the list, Package: " + expectedUnitRate.Package + ", Parent: " + expectedUnitRate.Parent + ", Units: " + expectedUnitRate.Units, rateFound);
			}
		}

		#endregion

	}
}
