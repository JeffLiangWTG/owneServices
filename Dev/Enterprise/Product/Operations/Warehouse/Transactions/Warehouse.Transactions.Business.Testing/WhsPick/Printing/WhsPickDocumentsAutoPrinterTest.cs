using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.Business.Printing.Testing
{
	public class WhsPickDocumentsAutoPrinterTest : WhsDocumentPrinterTest
	{
		#region New Tests

		#region TestUnPublishedDocBuilderDocumentDoesNotThrowException

		public void TestUnPublishedDocBuilderDocumentDoesNotThrowException()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderWarehouseDocumentsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var newFactory = new BusinessObjectFactory();
			var command = newFactory.Load<DocumentCommand>(GetExpectedDocBuilderMenuItemPK());
			command.SU_IsPublished = false;

			newFactory.Save();

			var buffer = new TestNotificationBuffer();
			AssertNoExceptionThrown(() => new WhsPickDocumentsAutoPrinter(Pick, buffer).PrintDocument());
			AssertEquals("Error: Unable to find Document to print. Please make sure the 'Pick Documents Pack' Document is published.", buffer.LastEvent.Message);
		}

		#endregion

		#region TestUnPublishedLegacyDocumentDoesNotThrowException

		public void TestUnPublishedLegacyDocumentDoesNotThrowException()
		{
			using (DocumentsDataRegistry.Instance.UseNewDocBuilderWarehouseDocumentsOnly.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var newFactory = new BusinessObjectFactory();
				var command = newFactory.Load<DocumentCommand>(GetExpectedLegacyMenuItemPK());
				command.SU_IsPublished = false;

				newFactory.Save();

				var buffer = new TestNotificationBuffer();
				AssertNoExceptionThrown(() => new WhsPickDocumentsAutoPrinter(Pick, buffer).PrintDocument());
				AssertEquals("Error: Unable to find Document to print. Please make sure the Legacy 'Pick Documents Pack' Document is published.", buffer.LastEvent.Message);
			}
		}

		#endregion

		#region TestAutoPrintOrderCopyForMOPOnPick

		public void TestAutoPrintOrderCopyForMOPOnPick()
		{
			Pick.Warehouse.WW_AutoPrintOrderCopyForMOPOnPick = true;
			Pick.Warehouse.WW_AutoPrintOrderSummaryOnPick = false;
			Pick.Warehouse.WW_AutoPrintPickingNonPickedItems = false;
			Pick.Warehouse.WW_AutoPrintPickingShortfallItems = false;
			Pick.Warehouse.WW_AutoPrintPickingSlip = false;

			DocumentPrinter.PrintDocument();
			AssertEquals("Currently doesn't honour AutoPrint", 1, DocumentPrinter.LastDocumentRunner.LastRunReportInfosForTesting.Count);
		}

		#endregion

		#region TestAutoPrintOrderSummaryOnPick

		public void TestAutoPrintOrderSummaryOnPick()
		{
			Pick.Warehouse.WW_AutoPrintOrderSummaryOnPick = true;
			Pick.Warehouse.WW_AutoPrintOrderCopyForMOPOnPick = false;
			Pick.Warehouse.WW_AutoPrintPickingNonPickedItems = false;
			Pick.Warehouse.WW_AutoPrintPickingShortfallItems = false;
			Pick.Warehouse.WW_AutoPrintPickingSlip = false;

			DocumentPrinter.PrintDocument();
			AssertEquals("Currently doesn't honour AutoPrint", 1, DocumentPrinter.LastDocumentRunner.LastRunReportInfosForTesting.Count);
		}

		#endregion

		#region TestAutoPrintPickingNonPickedAndShortfallItems

		public void TestAutoPrintPickingNonPickedAndShortfallItems()
		{
			AssertEquals("Precondition: Pick has shortfall items", ZBool.True, Pick.HasShortfallItems);
			AssertEquals("Precondition: Pick has non picked items", ZBool.True, Pick.HasNonAllocatedOrderedInventory);

			Pick.Warehouse.WW_AutoPrintPickingNonPickedItems = true;
			Pick.Warehouse.WW_AutoPrintPickingShortfallItems = true;
			Pick.Warehouse.WW_AutoPrintOrderSummaryOnPick = false;
			Pick.Warehouse.WW_AutoPrintOrderCopyForMOPOnPick = false;
			Pick.Warehouse.WW_AutoPrintPickingSlip = false;

			DocumentPrinter.PrintDocument();
			AssertEquals("Currently doesn't honour AutoPrint", 2, DocumentPrinter.LastDocumentRunner.LastRunReportInfosForTesting.Count);
		}

		#endregion

		#region TestAutoPrintPickingSlip

		public void TestAutoPrintPickingSlip()
		{
			Pick.Warehouse.WW_AutoPrintPickingSlip = true;
			Pick.Warehouse.WW_AutoPrintOrderSummaryOnPick = false;
			Pick.Warehouse.WW_AutoPrintOrderCopyForMOPOnPick = false;
			Pick.Warehouse.WW_AutoPrintPickingNonPickedItems = false;
			Pick.Warehouse.WW_AutoPrintPickingShortfallItems = false;

			DocumentPrinter.PrintDocument();
			AssertEquals("Currently doesn't honour AutoPrint", 1, DocumentPrinter.LastDocumentRunner.LastRunReportInfosForTesting.Count);
		}

		#endregion

		#region TestPrepareDocumentWithAllOptionsSetToFalse

		public void TestPrepareDocumentWithAllOptionsSetToFalse()
		{
			Pick.Warehouse.WW_AutoPrintPickingSlip = false;
			Pick.Warehouse.WW_AutoPrintOrderSummaryOnPick = false;
			Pick.Warehouse.WW_AutoPrintOrderCopyForMOPOnPick = false;
			Pick.Warehouse.WW_AutoPrintPickingNonPickedItems = false;
			Pick.Warehouse.WW_AutoPrintPickingShortfallItems = false;

			DocumentPrinter.PrintDocument();
			AssertNull("Print all or nothing, none selected for autoprint, so should be nothing", DocumentPrinter.LastDocumentRunner);
		}

		#endregion

		#endregion

		#region TestUseLegacyVersion

		public void TestUseLegacyVersion()
		{
			var documentPrinter = new WhsPickDocumentsAutoPrinterForTest((WhsPick)Parent, Factory);

			AssertEquals("Precondition", true, DocumentsDataRegistry.Instance.UseNewDocBuilderWarehouseDocumentsOnly.Value);
			AssertEquals("UseLegacyVersion should be the inverse UseNewDocBuilderWarehouseDocumentsOnly", false, documentPrinter.UseLegacyVersionForTest());

			using (DocumentsDataRegistry.Instance.UseNewDocBuilderWarehouseDocumentsOnly.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("Precondition", false, DocumentsDataRegistry.Instance.UseNewDocBuilderWarehouseDocumentsOnly.Value);
				AssertEquals("UseLegacyVersion should be the inverse UseNewDocBuilderWarehouseDocumentsOnly", true, documentPrinter.UseLegacyVersionForTest());
			}
		}

		class WhsPickDocumentsAutoPrinterForTest : WhsPickDocumentsAutoPrinter
		{
			public WhsPickDocumentsAutoPrinterForTest(WhsPick parent, BusinessObjectFactory factory)
				: base(parent, new TestNotificationBuffer())
			{
			}

			public ZBool UseLegacyVersionForTest()
			{
				return this.UseLegacyVersion;
			}
		}

		#endregion

		#region TestDocumentMenuPK

		protected override ZGuid GetExpectedDocBuilderMenuItemPK()
		{
			return new ZGuid("15F5C2FC-0CEC-4D4F-8818-461592DD97BE");
		}

		protected override ZGuid GetExpectedLegacyMenuItemPK()
		{
			return new ZGuid("033a805b-67e6-49a4-a8d1-11bceb6a1039");
		}

		#endregion

		#region Implementation

		protected override ZString GetExpectedMenuItemName()
		{
			return "Pick Documents Pack";
		}

		protected override ZInt GetExpectedDocumentsToPrint()
		{
			return 5;
		}

		protected override IDocumentSupportable GetNewParentObject()
		{
			return Pick;
		}

		protected override WhsDocumentPrinter GetNewDocumentPrinter()
		{
			return new WhsPickDocumentsAutoPrinter((WhsPick)Parent, new TestNotificationBuffer());
		}

		#region Pick

		WhsPick Pick
		{
			get { return pick ?? (pick = SetUpPick()); }
		}

		WhsPick pick;

		WhsPick SetUpPick()
		{
			var order = Helper.CreateWhsOrder(Data.Org1, Data.Whs1);
			Helper.CreateWhsOrderLine(order, Data.Part1, 10m);

			var result = Helper.CreatePickNew(order);

			order.Warehouse.WW_AutoPrintOrderCopyForMOPOnPick = true;
			order.Warehouse.WW_AutoPrintOrderSummaryOnPick = true;
			order.Warehouse.WW_AutoPrintPickingNonPickedItems = true;
			order.Warehouse.WW_AutoPrintPickingShortfallItems = true;
			order.Warehouse.WW_AutoPrintPickingSlip = true;

			AssertEquals(true, result.HasNonAllocatedOrderedInventory);
			AssertEquals(true, result.HasShortfallItems);

			return result;
		}

		#endregion

		#region Data

		TestDataForInventory Data
		{
			get { return data ?? (data = SetUpData()); }
		}

		TestDataForInventory data;

		TestDataForInventory SetUpData()
		{
			var result = new TestDataForInventory(Factory);
			result.CreateSimpleInventory(false);

			return result;
		}

		#endregion

		#endregion
	}
}
