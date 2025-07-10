using System.Linq;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	class PkgPackageItemDivotExtensionsTest : WhsTestCaseWithFactory
	{
		#region TestDeleteForRepacking

		public void TestDeleteForRepacking()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductWeightAndVolume(data.Part1, 10m, "KG", 1m, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var orderLine = order.Lines[0];
			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.Quantity = 4m;
			var releaseLineRed = orderLine.ReleaseLines.AddNew();
			releaseLineRed.PartAttribute1 = "RED";
			releaseLineRed.Quantity = 6m;
			var package = order.PackageJob.Packages.AddNew();
			var packedItem1 = package.Pack(releaseLine, 1m);
			var packedItem2 = package.Pack(releaseLine, 2m);
			var packedItem3 = package.Pack(releaseLineRed, 3m);

			AssertEquals("Should have 5 picklines, 3 from packing releaseLine, 2 from packing releaseLineRed.", 5, orderLine.PickLines.Count);
			AssertEquals("Should have 3 divots.", 3, package.PackedItemDivots.Count);
			AssertEquals("Package Weight should be correct.", 60m, package.KP_Weight);

			var packedItems = orderLine.PickLines.Where(pl => !pl.IsUnpacked(Factory));
			AssertContainsExactElementsInAnyOrder(packedItems, package.PackedItemDivots.Select(d => d.PackedItem));

			var divot1 = package.PackedItemDivots.Single(d => d.KI_PackedQty == 1m && d.KI_ParentTableCode == WhsPickLineSchema.Constants.Prefix);
			var divot2 = package.PackedItemDivots.Single(d => d.KI_PackedQty == 2m && d.KI_ParentTableCode == WhsPickLineSchema.Constants.Prefix);
			var divot3 = package.PackedItemDivots.Single(d => d.KI_PackedQty == 3m && d.KI_ParentTableCode == WhsPickLineSchema.Constants.Prefix);
			divot1.DeleteForRepacking(releaseLine);
			AssertEquals("Package Weight should be updated.", 50m, package.KP_Weight);
			AssertEquals("Divot should be deleted.", true, divot1.IsDeleted);
			AssertEquals("Picklines should not merge.", 5, orderLine.PickLines.Count);

			divot3.DeleteForRepacking(releaseLine);
			AssertEquals("Package Weight should be updated.", 20m, package.KP_Weight);
			AssertEquals("Divot should be deleted.", true, divot3.IsDeleted);
			AssertEquals("Picklines should not merge.", 5, orderLine.PickLines.Count);

			divot2.DeleteForRepacking(releaseLine);
			AssertEquals("Package Weight should be updated.", 0m, package.KP_Weight);
			AssertEquals("Divot should be deleted.", true, divot2.IsDeleted);
			AssertEquals("Picklines should not merge.", 5, orderLine.PickLines.Count);
		}

		#endregion
	}
}
