using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.GUI.Testing
{
	[TestedType(typeof(USChildInBondCommodityForm))]
	sealed class USChildInBondCommodityFormTest : ZFormBasherTest
	{
		public void TestFormLayoutBasedOnParentPartDetails()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			var commodity = container.Commodities.AddNew();
			commodity.BY_PartNumber = "PT324";
			using (var form = new USChildInBondCommodityForm(commodity))
			{
				form.Show();
				var foundControls = form.Controls.Find("CommodityRelationShipSplitContainer", true);
				var commodityRelationShipSplitContainer = foundControls.Length > 0 ? (SplitContainer)foundControls[0] : null;
				AssertEquals("commodityRelationShipSplitContainer.Panel2Collapsed should not be shown when parent has part", true, commodityRelationShipSplitContainer.Panel2Collapsed);
				foundControls = commodityRelationShipSplitContainer.Panel1.Controls.Find("ClassificationGrid", true);
				var classificationGrid = foundControls.Length > 0 ? (ZGrid)foundControls[0] : null;
				AssertEndsWith("classificationGrid.GridId", "PART", classificationGrid.GridId);
				AssertEquals("classificationGrid.GridId should not end with 'NON-PART'", false, classificationGrid.GridId.EndsWith("NON-PART"));
				AssertEquals("BY_OH_Supplier should not be available when parent has part", true, classificationGrid.GetColumnStyle(CusInBondCargoDesc.Schema.BY_OH_Supplier).IsUnavailable);
				AssertEquals("BY_PartNumberForBinding should not be available when parent has part", true, classificationGrid.GetColumnStyle(CusInBondCargoDesc.Schema.BY_PartNumberForBinding).IsUnavailable);
				AssertNull("BY_PartAttrib1 should not be visible when parent has part", classificationGrid.GetColumnStyle(CusInBondCargoDesc.Schema.BY_PartAttrib1));
				AssertNull("BY_PartAttrib2 should not be visible when parent has part", classificationGrid.GetColumnStyle(CusInBondCargoDesc.Schema.BY_PartAttrib2));
				AssertNull("BY_PartAttrib3 should not be visible when parent has part", classificationGrid.GetColumnStyle(CusInBondCargoDesc.Schema.BY_PartAttrib3));
				AssertEquals("BY_WarehouseEntryNumber should not be available when parent has part", true, classificationGrid.GetColumnStyle(CusInBondCargoDesc.Schema.BY_WarehouseEntryNumber).IsUnavailable);
				AssertEquals("BY_WarehouseEntryLineNo should not be available when parent has part", true, classificationGrid.GetColumnStyle(CusInBondCargoDesc.Schema.BY_WarehouseEntryLineNo).IsUnavailable);
				AssertEquals("BY_InvoiceQuantity should not be available when parent has part", true, classificationGrid.GetColumnStyle(CusInBondCargoDesc.Schema.BY_InvoiceQuantity).IsUnavailable);
			}

			commodity.BY_PartNumber = "";
			using (var form = new USChildInBondCommodityForm(commodity))
			{
				form.Show();
				var foundControls = form.Controls.Find("CommodityRelationShipSplitContainer", true);
				var commodityRelationShipSplitContainer = foundControls.Length > 0 ? (SplitContainer)foundControls[0] : null;
				AssertEquals("commodityRelationShipSplitContainer.Panel2Collapsed should be shown when parent has no part", false, commodityRelationShipSplitContainer.Panel2Collapsed);
				foundControls = commodityRelationShipSplitContainer.Panel1.Controls.Find("ClassificationGrid", true);
				var classificationGrid = foundControls.Length > 0 ? (ZGrid)foundControls[0] : null;
				AssertEndsWith("classificationGrid.GridId", "NON-PART", classificationGrid.GridId);
				AssertEquals("BY_OH_Supplier should be available when parent has no part", false, classificationGrid.GetColumnStyle(CusInBondCargoDesc.Schema.BY_OH_Supplier).IsUnavailable);
				AssertEquals("BY_PartNumberForBinding should be available when parent has no part", false, classificationGrid.GetColumnStyle(CusInBondCargoDesc.Schema.BY_PartNumberForBinding).IsUnavailable);
				AssertNotNull("BY_PartAttrib1 should be visible when parent has no part", classificationGrid.GetColumnStyle(CusInBondCargoDesc.Schema.BY_PartAttrib1));
				AssertNotNull("BY_PartAttrib2 should be visible when parent has no part", classificationGrid.GetColumnStyle(CusInBondCargoDesc.Schema.BY_PartAttrib2));
				AssertNotNull("BY_PartAttrib3 should be visible when parent has no part", classificationGrid.GetColumnStyle(CusInBondCargoDesc.Schema.BY_PartAttrib3));
				AssertNotNull("BY_WarehouseEntryNumber should be available when parent no part", classificationGrid.GetColumnStyle(CusInBondCargoDesc.Schema.BY_WarehouseEntryNumber));
				AssertNotNull("BY_WarehouseEntryLineNo should be available when parent no part", classificationGrid.GetColumnStyle(CusInBondCargoDesc.Schema.BY_WarehouseEntryLineNo));
				AssertNotNull("BY_InvoiceQuantity should be available when parent no part", classificationGrid.GetColumnStyle(CusInBondCargoDesc.Schema.BY_InvoiceQuantity));
			}
		}

		public void TestPartAttributeCaptions()
		{
			var org = Factory.New<OrgHeader>();
			org.MiscServ.OM_IMPartAttrib1Name = "VIN1";
			org.MiscServ.OM_IMPartAttrib2Name = "VIN2";
			org.MiscServ.OM_IMPartAttrib3Name = "VIN3";
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = org.MainAddress.PK;
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			var commodity = container.Commodities.AddNew();
			commodity.BY_PartNumber = "";
			using (var form = new USChildInBondCommodityForm(commodity))
			{
				form.Show();
				var foundControls = form.Controls.Find("ClassificationGrid", true);
				var classificationGrid = foundControls.Length > 0 ? (ZGrid)foundControls[0] : null;
				AssertEndsWith("classificationGrid.GridId", "PART", classificationGrid.GridId);
				AssertEquals("BY_PartAttrib1 caption", "VIN1", classificationGrid.GetColumnStyle(CusInBondCargoDesc.Schema.BY_PartAttrib1).Caption);
				AssertEquals("BY_PartAttrib2 caption", "VIN2", classificationGrid.GetColumnStyle(CusInBondCargoDesc.Schema.BY_PartAttrib2).Caption);
				AssertEquals("BY_PartAttrib3 caption", "VIN3", classificationGrid.GetColumnStyle(CusInBondCargoDesc.Schema.BY_PartAttrib3).Caption);
				AssertEquals("Serial Number caption", "Serial Number", classificationGrid.GetColumnStyle(CusInBondCargoDesc.Schema.BY_SerialNumber).Caption);
			}

			header.ImporterOrgPK = ZGuid.Empty;
			using (var form = new USChildInBondCommodityForm(commodity))
			{
				form.Show();
				var foundControls = form.Controls.Find("ClassificationGrid", true);
				var classificationGrid = foundControls.Length > 0 ? (ZGrid)foundControls[0] : null;
				AssertEndsWith("classificationGrid.GridId", "PART", classificationGrid.GridId);
				AssertEquals("BY_PartAttrib1 caption", "Part Attrib. 1", classificationGrid.GetColumnStyle(CusInBondCargoDesc.Schema.BY_PartAttrib1).Caption);
				AssertEquals("BY_PartAttrib2 caption", "Part Attrib. 2", classificationGrid.GetColumnStyle(CusInBondCargoDesc.Schema.BY_PartAttrib2).Caption);
				AssertEquals("BY_PartAttrib3 caption", "Part Attrib. 3", classificationGrid.GetColumnStyle(CusInBondCargoDesc.Schema.BY_PartAttrib3).Caption);
				AssertEquals("Serial Number caption", "Serial Number", classificationGrid.GetColumnStyle(CusInBondCargoDesc.Schema.BY_SerialNumber).Caption);
			}
		}

		public void TestSerialNumberColumn()
		{
			var org = Factory.New<OrgHeader>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = org.MainAddress.PK;
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			var commodity = container.Commodities.AddNew();
			commodity.BY_PartNumber = "";
			using (var form = new USChildInBondCommodityForm(commodity))
			{
				form.Show();
				var foundControls = form.Controls.Find("ClassificationGrid", true);
				var classificationGrid = foundControls.Length > 0 ? (ZGrid)foundControls[0] : null;
				AssertEndsWith("classificationGrid.GridId", "PART", classificationGrid.GridId);
				AssertEquals("Column exists only when registry setting enabled", true, classificationGrid.Columns.Contains(CusInBondCargoDesc.Schema.BY_SerialNumber));
			}
		}

		protected override Form GetFormToBashCore()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var container = moveDetail.Containers.AddNew();
			var commodity = container.Commodities.AddNew();
			Factory.Save();
			var result = new USChildInBondCommodityForm(commodity);
			return result;
		}

		protected override bool AllowSaveOnFormForTestHasChanges => false; //this is not a form that needs to be saved
	}
}
