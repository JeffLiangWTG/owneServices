using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	[TestedType(typeof(UEMMessageChooserItem))]
	sealed class UEMMessageChooserItemTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLookup()
		{
			var header = Factory.New<USExportAsycudaManifestHeader>();
			header.AMA_ManifestType = "EXP";
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "123";
			var splitBillSelectionItem = new SplitBillSelectionItem(bill);
			var chooser = new UEMMessageChooser(header, new ISelectionItem[] { splitBillSelectionItem });
			var item = chooser.ChooserItems[0];
			AssertType("BillOfLadingActionType is USExportBillOfLadingActionCodeType", typeof(USExportBillOfLadingActionCodeType), item.BillOfLadingActionType);
		}

		public void TestProperties()
		{
			var header = Factory.New<USExportAsycudaManifestHeader>();
			header.AMA_ManifestType = "EXP";
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "123";
			bill.ABL_ManifestQty = 2;
			var splitBillSelectionItem = new SplitBillSelectionItem(bill);
			var chooser = new UEMMessageChooser(header, new ISelectionItem[] { splitBillSelectionItem });
			var item = chooser.ChooserItems[0];
			AssertEquals("ActionType", USExportBillOfLadingActionCodeType.Codes.A, item.ActionType);
			AssertEquals("ManifestQty", 2, item.ManifestQty);
			AssertXMLContains("<ActionCode>A</ActionCode>", item.Message);

			item.ActionType = USExportBillOfLadingActionCodeType.Codes.D;
			AssertEquals("ActionType", USExportBillOfLadingActionCodeType.Codes.D, item.ActionType);
			AssertXMLContains("<ActionCode>D</ActionCode>", item.Message);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<USExportAsycudaManifestHeader>();
			header.AMA_ManifestType = "EXP";
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "123";
			var splitBillSelectionItem = new SplitBillSelectionItem(bill);
			var chooser = new UEMMessageChooser(header, new ISelectionItem[] { splitBillSelectionItem });
			return new UEMMessageChooserItem(chooser, splitBillSelectionItem, true);
		}
	}
}
